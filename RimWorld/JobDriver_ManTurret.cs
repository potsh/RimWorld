using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ManTurret : JobDriver
	{
		private const float ShellSearchRadius = 40f;

		private const int MaxPawnAmmoReservations = 10;

		private static bool GunNeedsLoading(Building b)
		{
			Building_TurretGun building_TurretGun = b as Building_TurretGun;
			if (building_TurretGun == null)
			{
				return false;
			}
			CompChangeableProjectile compChangeableProjectile = building_TurretGun.gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile == null || compChangeableProjectile.Loaded)
			{
				return false;
			}
			return true;
		}

		public static Thing FindAmmoForTurret(Pawn pawn, Building_TurretGun gun)
		{
			StorageSettings allowedShellsSettings = (!pawn.IsColonist) ? null : gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings;
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (t.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(t, 10, 1))
				{
					return false;
				}
				if (allowedShellsSettings != null && !allowedShellsSettings.AllowedToAccept(t))
				{
					return false;
				}
				return true;
			};
			return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, validator);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo targetA = base.job.targetA;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_004e: stateMachine*/;
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil loadIfNeeded = new Toil();
			loadIfNeeded.initAction = delegate
			{
				Pawn actor = loadIfNeeded.actor;
				Building building = (Building)actor.CurJob.targetA.Thing;
				Building_TurretGun building_TurretGun = building as Building_TurretGun;
				if (!GunNeedsLoading(building))
				{
					_003CMakeNewToils_003Ec__Iterator._0024this.JumpToToil(gotoTurret);
				}
				else
				{
					Thing thing = FindAmmoForTurret(_003CMakeNewToils_003Ec__Iterator._0024this.pawn, building_TurretGun);
					if (thing == null)
					{
						if (actor.Faction == Faction.OfPlayer)
						{
							Messages.Message("MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun.Label, actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(), building_TurretGun, MessageTypeDefOf.NegativeEvent);
						}
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					actor.CurJob.targetB = thing;
					actor.CurJob.count = 1;
				}
			};
			yield return loadIfNeeded;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
