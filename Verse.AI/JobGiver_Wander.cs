using RimWorld;
using System;

namespace Verse.AI
{
	public abstract class JobGiver_Wander : ThinkNode_JobGiver
	{
		protected float wanderRadius;

		protected Func<Pawn, IntVec3, IntVec3, bool> wanderDestValidator;

		protected IntRange ticksBetweenWandersRange = new IntRange(20, 100);

		protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

		protected Danger maxDanger = Danger.None;

		protected int expiryInterval = -1;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Wander jobGiver_Wander = (JobGiver_Wander)base.DeepCopy(resolve);
			jobGiver_Wander.wanderRadius = wanderRadius;
			jobGiver_Wander.wanderDestValidator = wanderDestValidator;
			jobGiver_Wander.ticksBetweenWandersRange = ticksBetweenWandersRange;
			jobGiver_Wander.locomotionUrgency = locomotionUrgency;
			jobGiver_Wander.maxDanger = maxDanger;
			jobGiver_Wander.expiryInterval = expiryInterval;
			return jobGiver_Wander;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = pawn.CurJob != null && pawn.CurJob.def == JobDefOf.GotoWander;
			bool nextMoveOrderIsWait = pawn.mindState.nextMoveOrderIsWait;
			if (!flag)
			{
				pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
			}
			if (nextMoveOrderIsWait && !flag)
			{
				Job job = new Job(JobDefOf.Wait_Wander);
				job.expiryInterval = ticksBetweenWandersRange.RandomInRange;
				return job;
			}
			IntVec3 exactWanderDest = GetExactWanderDest(pawn);
			if (!exactWanderDest.IsValid)
			{
				pawn.mindState.nextMoveOrderIsWait = false;
				return null;
			}
			Job job2 = new Job(JobDefOf.GotoWander, exactWanderDest);
			job2.locomotionUrgency = locomotionUrgency;
			job2.expiryInterval = expiryInterval;
			job2.checkOverrideOnExpire = true;
			return job2;
		}

		protected virtual IntVec3 GetExactWanderDest(Pawn pawn)
		{
			IntVec3 wanderRoot = GetWanderRoot(pawn);
			return RCellFinder.RandomWanderDestFor(pawn, wanderRoot, wanderRadius, wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, maxDanger));
		}

		protected abstract IntVec3 GetWanderRoot(Pawn pawn);
	}
}
