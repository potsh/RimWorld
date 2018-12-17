using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_TurretGun : Building_Turret
	{
		protected int burstCooldownTicksLeft;

		protected int burstWarmupTicksLeft;

		protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

		private bool holdFire;

		public Thing gun;

		protected TurretTop top;

		protected CompPowerTrader powerComp;

		protected CompMannable mannableComp;

		private const int TryStartShootSomethingIntervalTicks = 10;

		public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

		public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

		public override LocalTargetInfo CurrentTarget => currentTargetInt;

		private bool WarmingUp => burstWarmupTicksLeft > 0;

		public override Verb AttackVerb => GunCompEq.PrimaryVerb;

		private bool PlayerControlled => (base.Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

		private bool CanSetForcedTarget => mannableComp != null && PlayerControlled;

		private bool CanToggleHoldFire => PlayerControlled;

		private bool IsMortar => def.building.IsMortar;

		private bool IsMortarOrProjectileFliesOverhead => AttackVerb.ProjectileFliesOverhead() || IsMortar;

		private bool CanExtractShell
		{
			get
			{
				if (!PlayerControlled)
				{
					return false;
				}
				return gun.TryGetComp<CompChangeableProjectile>()?.Loaded ?? false;
			}
		}

		private bool MannedByColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

		private bool MannedByNonColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

		public Building_TurretGun()
		{
			top = new TurretTop(this);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			mannableComp = GetComp<CompMannable>();
		}

		public override void PostMake()
		{
			base.PostMake();
			MakeGun();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			ResetCurrentTarget();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
			Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
			Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
			Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
			Scribe_Deep.Look(ref gun, "gun");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.TurretPostLoadInit(this);
				UpdateGunVerbs();
			}
		}

		public override bool ClaimableBy(Faction by)
		{
			if (!base.ClaimableBy(by))
			{
				return false;
			}
			if (mannableComp != null && mannableComp.ManningPawn != null)
			{
				return false;
			}
			if (powerComp != null && powerComp.PowerOn)
			{
				return false;
			}
			return true;
		}

		public override void OrderAttack(LocalTargetInfo targ)
		{
			if (!targ.IsValid)
			{
				if (forcedTarget.IsValid)
				{
					ResetForcedTarget();
				}
			}
			else if ((targ.Cell - base.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
			{
				Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
			}
			else if ((targ.Cell - base.Position).LengthHorizontal > AttackVerb.verbProps.range)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				if (forcedTarget != targ)
				{
					forcedTarget = targ;
					if (burstCooldownTicksLeft <= 0)
					{
						TryStartShootSomething(canBeginBurstImmediately: false);
					}
				}
				if (holdFire)
				{
					Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (CanExtractShell && MannedByColonist)
			{
				CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
				if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
				{
					ExtractShell();
				}
			}
			if (forcedTarget.IsValid && !CanSetForcedTarget)
			{
				ResetForcedTarget();
			}
			if (!CanToggleHoldFire)
			{
				holdFire = false;
			}
			if (forcedTarget.ThingDestroyed)
			{
				ResetForcedTarget();
			}
			if ((powerComp == null || powerComp.PowerOn) && (mannableComp == null || mannableComp.MannedNow) && base.Spawned)
			{
				GunCompEq.verbTracker.VerbsTick();
				if (!stunner.Stunned && AttackVerb.state != VerbState.Bursting)
				{
					if (WarmingUp)
					{
						burstWarmupTicksLeft--;
						if (burstWarmupTicksLeft == 0)
						{
							BeginBurst();
						}
					}
					else
					{
						if (burstCooldownTicksLeft > 0)
						{
							burstCooldownTicksLeft--;
						}
						if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
						{
							TryStartShootSomething(canBeginBurstImmediately: true);
						}
					}
					top.TurretTopTick();
				}
			}
			else
			{
				ResetCurrentTarget();
			}
		}

		protected void TryStartShootSomething(bool canBeginBurstImmediately)
		{
			if (!base.Spawned || (holdFire && CanToggleHoldFire) || (AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !AttackVerb.Available())
			{
				ResetCurrentTarget();
			}
			else
			{
				bool isValid = currentTargetInt.IsValid;
				if (forcedTarget.IsValid)
				{
					currentTargetInt = forcedTarget;
				}
				else
				{
					currentTargetInt = TryFindNewTarget();
				}
				if (!isValid && currentTargetInt.IsValid)
				{
					SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				if (currentTargetInt.IsValid)
				{
					if (def.building.turretBurstWarmupTime > 0f)
					{
						burstWarmupTicksLeft = def.building.turretBurstWarmupTime.SecondsToTicks();
					}
					else if (canBeginBurstImmediately)
					{
						BeginBurst();
					}
					else
					{
						burstWarmupTicksLeft = 1;
					}
				}
				else
				{
					ResetCurrentTarget();
				}
			}
		}

		protected LocalTargetInfo TryFindNewTarget()
		{
			IAttackTargetSearcher attackTargetSearcher = TargSearcher();
			Faction faction = attackTargetSearcher.Thing.Faction;
			float range = AttackVerb.verbProps.range;
			if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate(Building x)
			{
				float num = AttackVerb.verbProps.EffectiveMinRange(x, this);
				float num2 = (float)x.Position.DistanceToSquared(base.Position);
				return num2 > num * num && num2 < range * range;
			}).TryRandomElement(out Building result))
			{
				return result;
			}
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
			if (!AttackVerb.ProjectileFliesOverhead())
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToAll;
				targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
			}
			if (AttackVerb.IsIncendiary())
			{
				targetScanFlags |= TargetScanFlags.NeedNonBurning;
			}
			return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, IsValidTarget);
		}

		private IAttackTargetSearcher TargSearcher()
		{
			if (mannableComp != null && mannableComp.MannedNow)
			{
				return mannableComp.ManningPawn;
			}
			return this;
		}

		private bool IsValidTarget(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (AttackVerb.ProjectileFliesOverhead())
				{
					RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
					if (roofDef != null && roofDef.isThickRoof)
					{
						return false;
					}
				}
				if (mannableComp == null)
				{
					return !GenAI.MachinesLike(base.Faction, pawn);
				}
				if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
			return true;
		}

		protected void BeginBurst()
		{
			AttackVerb.TryStartCastOn(CurrentTarget);
			OnAttackedTarget(CurrentTarget);
		}

		protected void BurstComplete()
		{
			burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
		}

		protected float BurstCooldownTime()
		{
			if (def.building.turretBurstCooldownTime >= 0f)
			{
				return def.building.turretBurstCooldownTime;
			}
			return AttackVerb.verbProps.defaultCooldownTime;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (AttackVerb.verbProps.minRange > 0f)
			{
				stringBuilder.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
			}
			if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
			{
				stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}
			else if (base.Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
			{
				stringBuilder.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
			}
			CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null)
			{
				if (compChangeableProjectile.Loaded)
				{
					stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
				}
				else
				{
					stringBuilder.AppendLine("ShellNotLoaded".Translate());
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override void Draw()
		{
			top.DrawTurret();
			base.Draw();
		}

		public override void DrawExtraSelectionOverlays()
		{
			float range = AttackVerb.verbProps.range;
			if (range < 90f)
			{
				GenDraw.DrawRadiusRing(base.Position, range);
			}
			float num = AttackVerb.verbProps.EffectiveMinRange(allowAdjacentShot: true);
			if (num < 90f && num > 0.1f)
			{
				GenDraw.DrawRadiusRing(base.Position, num);
			}
			if (WarmingUp)
			{
				int degreesWide = (int)((float)burstWarmupTicksLeft * 0.5f);
				GenDraw.DrawAimPie(this, CurrentTarget, degreesWide, (float)def.size.x * 0.5f);
			}
			if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
			{
				Vector3 b = (!forcedTarget.HasThing) ? forcedTarget.Cell.ToVector3Shifted() : forcedTarget.Thing.TrueCenter();
				Vector3 a = this.TrueCenter();
				b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = b.y;
				GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (CanExtractShell)
			{
				CompChangeableProjectile changeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandExtractShell".Translate(),
					defaultDesc = "CommandExtractShellDesc".Translate(),
					icon = changeableProjectile.LoadedShell.uiIcon,
					iconAngle = changeableProjectile.LoadedShell.uiIconAngle,
					iconOffset = changeableProjectile.LoadedShell.uiIconOffset,
					iconDrawScale = GenUI.IconDrawScale(changeableProjectile.LoadedShell),
					action = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_018d: stateMachine*/)._0024this.ExtractShell();
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (CanSetForcedTarget)
			{
				Command_VerbTarget attack = new Command_VerbTarget
				{
					defaultLabel = "CommandSetForceAttackTarget".Translate(),
					defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
					verb = AttackVerb,
					hotKey = KeyBindingDefOf.Misc4
				};
				if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
				{
					attack.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
				}
				yield return (Gizmo)attack;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (forcedTarget.IsValid)
			{
				Command_Action stop = new Command_Action
				{
					defaultLabel = "CommandStopForceAttack".Translate(),
					defaultDesc = "CommandStopForceAttackDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
					action = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0333: stateMachine*/)._0024this.ResetForcedTarget();
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				};
				if (!forcedTarget.IsValid)
				{
					stop.Disable("CommandStopAttackFailNotForceAttacking".Translate());
				}
				stop.hotKey = KeyBindingDefOf.Misc5;
				yield return (Gizmo)stop;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (CanToggleHoldFire)
			{
				yield return (Gizmo)new Command_Toggle
				{
					defaultLabel = "CommandHoldFire".Translate(),
					defaultDesc = "CommandHoldFireDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
					hotKey = KeyBindingDefOf.Misc6,
					toggleAction = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_040f: stateMachine*/)._0024this.holdFire = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_040f: stateMachine*/)._0024this.holdFire;
						if (((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_040f: stateMachine*/)._0024this.holdFire)
						{
							((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_040f: stateMachine*/)._0024this.ResetForcedTarget();
						}
					},
					isActive = (() => ((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0426: stateMachine*/)._0024this.holdFire)
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0460:
			/*Error near IL_0461: Unexpected return in MoveNext()*/;
		}

		private void ExtractShell()
		{
			GenPlace.TryPlaceThing(gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near);
		}

		private void ResetForcedTarget()
		{
			forcedTarget = LocalTargetInfo.Invalid;
			burstWarmupTicksLeft = 0;
			if (burstCooldownTicksLeft <= 0)
			{
				TryStartShootSomething(canBeginBurstImmediately: false);
			}
		}

		private void ResetCurrentTarget()
		{
			currentTargetInt = LocalTargetInfo.Invalid;
			burstWarmupTicksLeft = 0;
		}

		public void MakeGun()
		{
			gun = ThingMaker.MakeThing(def.building.turretGunDef);
			UpdateGunVerbs();
		}

		private void UpdateGunVerbs()
		{
			List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = BurstComplete;
			}
		}
	}
}
