using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Open : JobDriver
	{
		public const int OpenTicks = 300;

		private IOpenable Openable => (IOpenable)job.targetA.Thing;

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
			yield return new Toil
			{
				initAction = delegate
				{
					if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003e: stateMachine*/)._0024this.Openable.CanOpen)
					{
						((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003e: stateMachine*/)._0024this.Map.designationManager.DesignationOn(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003e: stateMachine*/)._0024this.job.targetA.Thing, DesignationDefOf.Open)?.Delete();
					}
				}
			}.FailOnDespawnedOrNull(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
