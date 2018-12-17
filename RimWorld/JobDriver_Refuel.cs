using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Refuel : JobDriver
	{
		private const TargetIndex RefuelableInd = TargetIndex.A;

		private const TargetIndex FuelInd = TargetIndex.B;

		private const int RefuelingDuration = 240;

		protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

		protected CompRefuelable RefuelableComp => Refuelable.TryGetComp<CompRefuelable>();

		protected Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Refuelable;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = Fuel;
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				result = (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			AddEndCondition(() => (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0050: stateMachine*/)._0024this.RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
			AddFailCondition(() => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0067: stateMachine*/)._0024this.job.playerForced && !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0067: stateMachine*/)._0024this.RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
			yield return Toils_General.DoAtomic(delegate
			{
				((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0079: stateMachine*/)._0024this.job.count = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0079: stateMachine*/)._0024this.RefuelableComp.GetFuelCountToFullyRefuel();
			});
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
