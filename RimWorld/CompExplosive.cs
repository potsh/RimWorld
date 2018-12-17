using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompExplosive : ThingComp
	{
		public bool wickStarted;

		protected int wickTicksLeft;

		private Thing instigator;

		public bool destroyedThroughDetonation;

		protected Sustainer wickSoundSustainer;

		public CompProperties_Explosive Props => (CompProperties_Explosive)props;

		protected int StartWickThreshold => Mathf.RoundToInt(Props.startWickHitPointsPercent * (float)parent.MaxHitPoints);

		private bool CanEverExplodeFromDamage
		{
			get
			{
				if (Props.chanceNeverExplodeFromDamage < 1E-05f)
				{
					return true;
				}
				Rand.PushState();
				Rand.Seed = parent.thingIDNumber.GetHashCode();
				bool result = Rand.Value < Props.chanceNeverExplodeFromDamage;
				Rand.PopState();
				return result;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref instigator, "instigator");
			Scribe_Values.Look(ref wickStarted, "wickStarted", defaultValue: false);
			Scribe_Values.Look(ref wickTicksLeft, "wickTicksLeft", 0);
			Scribe_Values.Look(ref destroyedThroughDetonation, "destroyedThroughDetonation", defaultValue: false);
		}

		public override void CompTick()
		{
			if (wickStarted)
			{
				if (wickSoundSustainer == null)
				{
					StartWickSustainer();
				}
				else
				{
					wickSoundSustainer.Maintain();
				}
				wickTicksLeft--;
				if (wickTicksLeft <= 0)
				{
					Detonate(parent.MapHeld);
				}
			}
		}

		private void StartWickSustainer()
		{
			SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
			wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}

		private void EndWickSustainer()
		{
			if (wickSoundSustainer != null)
			{
				wickSoundSustainer.End();
				wickSoundSustainer = null;
			}
		}

		public override void PostDraw()
		{
			if (wickStarted)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.BurningWick);
			}
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (CanEverExplodeFromDamage)
			{
				if (dinfo.Def.ExternalViolenceFor(parent) && dinfo.Amount >= (float)parent.HitPoints && CanExplodeFromDamageType(dinfo.Def))
				{
					if (parent.MapHeld != null)
					{
						Detonate(parent.MapHeld);
						if (parent.Destroyed)
						{
							absorbed = true;
						}
					}
				}
				else if (!wickStarted && Props.startWickOnDamageTaken != null && dinfo.Def == Props.startWickOnDamageTaken)
				{
					StartWick(dinfo.Instigator);
				}
			}
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (CanEverExplodeFromDamage && CanExplodeFromDamageType(dinfo.Def) && !parent.Destroyed)
			{
				if (wickStarted && dinfo.Def == DamageDefOf.Stun)
				{
					StopWick();
				}
				else if (!wickStarted && parent.HitPoints <= StartWickThreshold && dinfo.Def.ExternalViolenceFor(parent))
				{
					StartWick(dinfo.Instigator);
				}
			}
		}

		public void StartWick(Thing instigator = null)
		{
			if (!wickStarted && !(ExplosiveRadius() <= 0f))
			{
				this.instigator = instigator;
				wickStarted = true;
				wickTicksLeft = Props.wickTicks.RandomInRange;
				StartWickSustainer();
				GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(parent, Props.explosiveDamageType);
			}
		}

		public void StopWick()
		{
			wickStarted = false;
			instigator = null;
		}

		public float ExplosiveRadius()
		{
			CompProperties_Explosive props = Props;
			float num = props.explosiveRadius;
			if (parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0f)
			{
				num += Mathf.Sqrt((float)(parent.stackCount - 1) * props.explosiveExpandPerStackcount);
			}
			if (props.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
			{
				num += Mathf.Sqrt(parent.GetComp<CompRefuelable>().Fuel * props.explosiveExpandPerFuel);
			}
			return num;
		}

		protected void Detonate(Map map)
		{
			if (parent.SpawnedOrAnyParentSpawned)
			{
				CompProperties_Explosive props = Props;
				float num = ExplosiveRadius();
				if (props.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
				{
					parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
				}
				if (props.destroyThingOnExplosionSize <= num && !parent.Destroyed)
				{
					destroyedThroughDetonation = true;
					parent.Kill();
				}
				EndWickSustainer();
				wickStarted = false;
				if (map == null)
				{
					Log.Warning("Tried to detonate CompExplosive in a null map.");
				}
				else
				{
					if (props.explosionEffect != null)
					{
						Effecter effecter = props.explosionEffect.Spawn();
						effecter.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
						effecter.Cleanup();
					}
					IntVec3 positionHeld = parent.PositionHeld;
					float radius = num;
					DamageDef explosiveDamageType = props.explosiveDamageType;
					Thing thing = instigator ?? parent;
					int damageAmountBase = props.damageAmountBase;
					float armorPenetrationBase = props.armorPenetrationBase;
					SoundDef explosionSound = props.explosionSound;
					ThingDef postExplosionSpawnThingDef = props.postExplosionSpawnThingDef;
					float postExplosionSpawnChance = props.postExplosionSpawnChance;
					int postExplosionSpawnThingCount = props.postExplosionSpawnThingCount;
					GenExplosion.DoExplosion(positionHeld, map, radius, explosiveDamageType, thing, damageAmountBase, armorPenetrationBase, explosionSound, null, null, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, props.applyDamageToExplosionCellsNeighbors, props.preExplosionSpawnThingDef, props.preExplosionSpawnChance, props.preExplosionSpawnThingCount, props.chanceToStartFire, props.damageFalloff);
				}
			}
		}

		private bool CanExplodeFromDamageType(DamageDef damage)
		{
			return Props.requiredDamageTypeToExplode == null || Props.requiredDamageTypeToExplode == damage;
		}
	}
}
