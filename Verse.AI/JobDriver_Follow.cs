using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_Follow : JobDriver
	{
		private const TargetIndex FolloweeInd = TargetIndex.A;

		private const int Distance = 4;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return new Toil
			{
				tickAction = delegate
				{
					Pawn pawn = (Pawn)((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.job.GetTarget(TargetIndex.A).Thing;
					if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.Position.InHorDistOf(pawn.Position, 4f) || !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.Position.WithinRegions(pawn.Position, ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.Map, 2, TraverseParms.For(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn)))
					{
						if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
						{
							((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.EndJobWith(JobCondition.Incompletable);
						}
						else if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Moving || ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.Destination != pawn)
						{
							((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003f: stateMachine*/)._0024this.pawn.pather.StartPath(pawn, PathEndMode.Touch);
						}
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
