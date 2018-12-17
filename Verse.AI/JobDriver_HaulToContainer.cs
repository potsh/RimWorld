using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_HaulToContainer : JobDriver
	{
		protected const TargetIndex CarryThingIndex = TargetIndex.A;

		protected const TargetIndex DestIndex = TargetIndex.B;

		protected const TargetIndex PrimaryDestIndex = TargetIndex.C;

		public Thing ThingToCarry => (Thing)job.GetTarget(TargetIndex.A);

		public Thing Container => (Thing)job.GetTarget(TargetIndex.B);

		private int Duration => (Container != null && Container is Building) ? Container.def.building.haulToContainerDuration : 0;

		public override string GetReport()
		{
			Thing thing = null;
			thing = ((pawn.CurJob != job || pawn.carryTracker.CarriedThing == null) ? base.TargetThingA : pawn.carryTracker.CarriedThing);
			if (thing == null || !job.targetB.HasThing)
			{
				return "ReportHaulingUnknown".Translate();
			}
			return "ReportHaulingTo".Translate(thing.Label, job.targetB.Thing.LabelShort.Named("DESTINATION"), thing.Named("THING"));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			pawn = base.pawn;
			target = base.job.GetTarget(TargetIndex.B);
			job = base.job;
			errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			base.pawn.ReserveAsManyAsPossible(base.job.GetTargetQueue(TargetIndex.A), base.job);
			base.pawn.ReserveAsManyAsPossible(base.job.GetTargetQueue(TargetIndex.B), base.job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => TransporterUtility.WasLoadingCanceled(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0065: stateMachine*/)._0024this.Container));
			this.FailOn(delegate
			{
				ThingOwner thingOwner = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_007d: stateMachine*/)._0024this.Container.TryGetInnerInteractableThingOwner();
				if (thingOwner != null && !thingOwner.CanAcceptAnyOf(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_007d: stateMachine*/)._0024this.ThingToCarry))
				{
					return true;
				}
				IHaulDestination haulDestination = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_007d: stateMachine*/)._0024this.Container as IHaulDestination;
				if (haulDestination != null && !haulDestination.Accepts(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_007d: stateMachine*/)._0024this.ThingToCarry))
				{
					return true;
				}
				return false;
			});
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return getToHaulTarget;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
