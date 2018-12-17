using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherPawns : JobDriver
	{
		private const TargetIndex AnimalOrSlaveInd = TargetIndex.A;

		private Pawn AnimalOrSlave => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = AnimalOrSlave;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002b: stateMachine*/)._0024this.Map.lordManager.lords.Contains(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002b: stateMachine*/)._0024this.job.lord));
			this.FailOn(() => ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0043: stateMachine*/)._0024this.AnimalOrSlave == null || ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0043: stateMachine*/)._0024this.AnimalOrSlave.GetLord() != ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0043: stateMachine*/)._0024this.job.lord);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A).FailOn(() => GatherAnimalsAndSlavesForCaravanUtility.IsFollowingAnyone(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0063: stateMachine*/)._0024this.AnimalOrSlave));
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private Toil SetFollowerToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				GatherAnimalsAndSlavesForCaravanUtility.SetFollower(AnimalOrSlave, pawn);
				RestUtility.WakeUp(pawn);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
