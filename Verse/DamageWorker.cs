using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class DamageWorker
	{
		public class DamageResult
		{
			public bool wounded;

			public bool headshot;

			public bool deflected;

			public bool deflectedByMetalArmor;

			public bool diminished;

			public bool diminishedByMetalArmor;

			public Thing hitThing;

			public List<BodyPartRecord> parts;

			public List<Hediff> hediffs;

			public float totalDamageDealt;

			public BodyPartRecord LastHitPart
			{
				get
				{
					if (parts == null)
					{
						return null;
					}
					if (parts.Count <= 0)
					{
						return null;
					}
					return parts[parts.Count - 1];
				}
			}

			public void AddPart(Thing hitThing, BodyPartRecord part)
			{
				if (this.hitThing != null && this.hitThing != hitThing)
				{
					Log.ErrorOnce("Single damage worker referring to multiple things; will cause issues with combat log", 30667935);
				}
				this.hitThing = hitThing;
				if (parts == null)
				{
					parts = new List<BodyPartRecord>();
				}
				parts.Add(part);
			}

			public void AddHediff(Hediff hediff)
			{
				if (hediffs == null)
				{
					hediffs = new List<Hediff>();
				}
				hediffs.Add(hediff);
			}

			public void AssociateWithLog(LogEntry_DamageResult log)
			{
				if (log != null)
				{
					Pawn hitPawn = hitThing as Pawn;
					if (hitPawn != null)
					{
						List<BodyPartRecord> list = null;
						List<bool> recipientPartsDestroyed = null;
						if (!parts.NullOrEmpty() && hitPawn != null)
						{
							list = parts.Distinct().ToList();
							recipientPartsDestroyed = (from part in list
							select hitPawn.health.hediffSet.GetPartHealth(part) <= 0f).ToList();
						}
						log.FillTargets(list, recipientPartsDestroyed, deflected);
					}
					if (hediffs != null)
					{
						for (int i = 0; i < hediffs.Count; i++)
						{
							hediffs[i].combatLogEntry = new WeakReference<LogEntry>(log);
							hediffs[i].combatLogText = log.ToGameStringFromPOV(null);
						}
					}
				}
			}
		}

		public DamageDef def;

		private const float ExplosionCamShakeMultiplier = 4f;

		private static List<Thing> thingsToAffect = new List<Thing>();

		private static List<IntVec3> openCells = new List<IntVec3>();

		private static List<IntVec3> adjWallCells = new List<IntVec3>();

		public virtual DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageResult damageResult = new DamageResult();
			if (victim.SpawnedOrAnyParentSpawned)
			{
				ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType, victim.MapHeld);
			}
			if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
			{
				float num = dinfo.Amount;
				if (victim.def.category == ThingCategory.Building)
				{
					num *= dinfo.Def.buildingDamageFactor;
				}
				if (victim.def.category == ThingCategory.Plant)
				{
					num *= dinfo.Def.plantDamageFactor;
				}
				damageResult.totalDamageDealt = (float)Mathf.Min(victim.HitPoints, GenMath.RoundRandom(num));
				victim.HitPoints -= Mathf.RoundToInt(damageResult.totalDamageDealt);
				if (victim.HitPoints <= 0)
				{
					victim.HitPoints = 0;
					victim.Kill(dinfo);
				}
			}
			return damageResult;
		}

		public virtual void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
		{
			if (def.explosionHeatEnergyPerCell > 1.401298E-45f)
			{
				GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
			}
			MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
			if (explosion.Map == Find.CurrentMap)
			{
				float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
				Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
			}
			ExplosionVisualEffectCenter(explosion);
		}

		protected virtual void ExplosionVisualEffectCenter(Explosion explosion)
		{
			for (int i = 0; i < 4; i++)
			{
				MoteMaker.ThrowSmoke(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, explosion.radius * 0.6f);
			}
			if (def.explosionInteriorMote != null)
			{
				int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
				for (int j = 0; j < num; j++)
				{
					MoteMaker.ThrowExplosionInteriorMote(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, def.explosionInteriorMote);
				}
			}
		}

		public virtual void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			if (def.explosionCellMote != null && canThrowMotes)
			{
				float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
				Color color = Color.Lerp(def.explosionColorCenter, def.explosionColorEdge, t);
				MoteMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellMote, color);
			}
			thingsToAffect.Clear();
			float num = -3.40282347E+38f;
			bool flag = false;
			List<Thing> list = explosion.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal)
				{
					thingsToAffect.Add(thing);
					if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
					{
						flag = true;
						num = thing.def.Altitude;
					}
				}
			}
			for (int j = 0; j < thingsToAffect.Count; j++)
			{
				if (thingsToAffect[j].def.Altitude >= num)
				{
					ExplosionDamageThing(explosion, thingsToAffect[j], damagedThings, c);
				}
			}
			if (!flag)
			{
				ExplosionDamageTerrain(explosion, c);
			}
			if (def.explosionSnowMeltAmount > 0.0001f)
			{
				float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / explosion.radius;
				if (num2 > 0f)
				{
					explosion.Map.snowGrid.AddDepth(c, (0f - num2) * def.explosionSnowMeltAmount);
				}
			}
			if (def == DamageDefOf.Bomb || def == DamageDefOf.Flame)
			{
				List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
				for (int k = 0; k < list2.Count; k++)
				{
					RectTrigger rectTrigger = (RectTrigger)list2[k];
					if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
					{
						rectTrigger.ActivatedBy(null);
					}
				}
			}
		}

		protected virtual void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, IntVec3 cell)
		{
			if (t.def.category != ThingCategory.Mote && t.def.category != ThingCategory.Ethereal && !damagedThings.Contains(t))
			{
				damagedThings.Add(t);
				if (def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
				{
					t.Destroy();
				}
				else
				{
					float num = (!(t.Position == explosion.Position)) ? (t.Position - explosion.Position).AngleFlat : ((float)Rand.RangeInclusive(0, 359));
					DamageDef damageDef = def;
					float amount = (float)explosion.GetDamageAmountAt(cell);
					float armorPenetrationAt = explosion.GetArmorPenetrationAt(cell);
					float angle = num;
					Thing instigator = explosion.instigator;
					ThingDef weapon = explosion.weapon;
					DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetrationAt, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
					if (def.explosionAffectOutsidePartsOnly)
					{
						dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
					}
					BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
					Pawn pawn = t as Pawn;
					if (pawn != null)
					{
						battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, def);
						Find.BattleLog.Add(battleLogEntry_ExplosionImpact);
					}
					DamageResult damageResult = t.TakeDamage(dinfo);
					damageResult.AssociateWithLog(battleLogEntry_ExplosionImpact);
					if (pawn != null && damageResult.wounded && pawn.stances != null)
					{
						pawn.stances.StaggerFor(95);
					}
				}
			}
		}

		protected virtual void ExplosionDamageTerrain(Explosion explosion, IntVec3 c)
		{
			if (def == DamageDefOf.Bomb && explosion.Map.terrainGrid.CanRemoveTopLayerAt(c))
			{
				TerrainDef terrain = c.GetTerrain(explosion.Map);
				if (!(terrain.destroyOnBombDamageThreshold < 0f))
				{
					float num = (float)explosion.GetDamageAmountAt(c);
					if (num >= terrain.destroyOnBombDamageThreshold)
					{
						explosion.Map.terrainGrid.Notify_TerrainDestroyed(c);
					}
				}
			}
		}

		public IEnumerable<IntVec3> ExplosionCellsToHit(Explosion explosion)
		{
			return ExplosionCellsToHit(explosion.Position, explosion.Map, explosion.radius);
		}

		public virtual IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius)
		{
			openCells.Clear();
			adjWallCells.Clear();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && GenSight.LineOfSight(center, intVec, map, skipFirstCell: true))
				{
					openCells.Add(intVec);
				}
			}
			for (int j = 0; j < openCells.Count; j++)
			{
				IntVec3 intVec2 = openCells[j];
				if (intVec2.Walkable(map))
				{
					for (int k = 0; k < 4; k++)
					{
						IntVec3 intVec3 = intVec2 + GenAdj.CardinalDirections[k];
						if (intVec3.InHorDistOf(center, radius) && intVec3.InBounds(map) && !intVec3.Standable(map) && intVec3.GetEdifice(map) != null && !openCells.Contains(intVec3) && adjWallCells.Contains(intVec3))
						{
							adjWallCells.Add(intVec3);
						}
					}
				}
			}
			return openCells.Concat(adjWallCells);
		}
	}
}
