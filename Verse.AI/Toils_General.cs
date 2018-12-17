using RimWorld;
using System;

namespace Verse.AI
{
	public static class Toils_General
	{
		public static Toil Wait(int ticks, TargetIndex face = TargetIndex.None)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.pather.StopDead();
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = ticks;
			if (face != 0)
			{
				toil.handlingFacing = true;
				toil.tickAction = delegate
				{
					toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(face));
				};
			}
			return toil;
		}

		public static Toil WaitWith(TargetIndex targetInd, int ticks, bool useProgressBar = false, bool maintainPosture = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.pather.StopDead();
				Pawn pawn = toil.actor.CurJob.GetTarget(targetInd).Thing as Pawn;
				if (pawn != null)
				{
					if (pawn == toil.actor)
					{
						Log.Warning("Executing WaitWith toil but otherPawn is the same as toil.actor");
					}
					else
					{
						Pawn pawn2 = pawn;
						int ticks2 = ticks;
						bool maintainPosture2 = maintainPosture;
						PawnUtility.ForceWait(pawn2, ticks2, null, maintainPosture2);
					}
				}
			};
			toil.FailOnDespawnedOrNull(targetInd);
			toil.FailOnCannotTouch(targetInd, PathEndMode.Touch);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = ticks;
			if (useProgressBar)
			{
				toil.WithProgressBarToilDelay(targetInd);
			}
			return toil;
		}

		public static Toil RemoveDesignationsOnThing(TargetIndex ind, DesignationDef def)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.Map.designationManager.RemoveAllDesignationsOn(toil.actor.jobs.curJob.GetTarget(ind).Thing);
			};
			return toil;
		}

		public static Toil ClearTarget(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.GetActor().CurJob.SetTarget(ind, null);
			};
			return toil;
		}

		public static Toil PutCarriedThingInInventory()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.GetActor();
				if (actor.carryTracker.CarriedThing != null && !actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer))
				{
					actor.carryTracker.TryDropCarriedThing(actor.Position, actor.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out Thing _);
				}
			};
			return toil;
		}

		public static Toil Do(Action action)
		{
			Toil toil = new Toil();
			toil.initAction = action;
			return toil;
		}

		public static Toil DoAtomic(Action action)
		{
			Toil toil = new Toil();
			toil.initAction = action;
			toil.atomicWithPrevious = true;
			return toil;
		}

		public static Toil Open(TargetIndex openableInd)
		{
			Toil open = new Toil();
			open.initAction = delegate
			{
				Pawn actor = open.actor;
				Thing thing = actor.CurJob.GetTarget(openableInd).Thing;
				actor.Map.designationManager.DesignationOn(thing, DesignationDefOf.Open)?.Delete();
				IOpenable openable = (IOpenable)thing;
				if (openable.CanOpen)
				{
					openable.Open();
					actor.records.Increment(RecordDefOf.ContainersOpened);
				}
			};
			open.defaultCompleteMode = ToilCompleteMode.Instant;
			return open;
		}

		public static Toil Label()
		{
			Toil toil = new Toil();
			toil.atomicWithPrevious = true;
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
