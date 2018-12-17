using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FillFermentingBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex WortInd = TargetIndex.B;

		private const int Duration = 200;

		protected Building_FermentingBarrel Barrel => (Building_FermentingBarrel)job.GetTarget(TargetIndex.A).Thing;

		protected Thing Wort => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Barrel;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = Wort;
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
			this.FailOnBurningImmobile(TargetIndex.A);
			AddEndCondition(() => (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_005d: stateMachine*/)._0024this.Barrel.SpaceLeftForWort > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
			yield return Toils_General.DoAtomic(delegate
			{
				((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006f: stateMachine*/)._0024this.job.count = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006f: stateMachine*/)._0024this.Barrel.SpaceLeftForWort;
			});
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
