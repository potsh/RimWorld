using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GotoTravelDestination : ThinkNode_JobGiver
	{
		private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		private Danger maxDanger = Danger.Some;

		private int jobMaxDuration = 999999;

		private bool exactCell;

		private IntRange WaitTicks = new IntRange(30, 80);

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GotoTravelDestination jobGiver_GotoTravelDestination = (JobGiver_GotoTravelDestination)base.DeepCopy(resolve);
			jobGiver_GotoTravelDestination.locomotionUrgency = locomotionUrgency;
			jobGiver_GotoTravelDestination.maxDanger = maxDanger;
			jobGiver_GotoTravelDestination.jobMaxDuration = jobMaxDuration;
			jobGiver_GotoTravelDestination.exactCell = exactCell;
			return jobGiver_GotoTravelDestination;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			if (pawn.mindState.nextMoveOrderIsWait && !exactCell)
			{
				Job job = new Job(JobDefOf.Wait_Wander);
				job.expiryInterval = WaitTicks.RandomInRange;
				return job;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			if (!pawn.CanReach(cell, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
			{
				return null;
			}
			if (exactCell && pawn.Position == cell)
			{
				return null;
			}
			IntVec3 c = cell;
			if (!exactCell)
			{
				c = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 6);
			}
			Job job2 = new Job(JobDefOf.Goto, c);
			job2.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
			job2.expiryInterval = jobMaxDuration;
			return job2;
		}
	}
}
