using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	internal class JobGiver_FightFiresNearPoint : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FightFiresNearPoint jobGiver_FightFiresNearPoint = (JobGiver_FightFiresNearPoint)base.DeepCopy(resolve);
			jobGiver_FightFiresNearPoint.maxDistFromPoint = maxDistFromPoint;
			return jobGiver_FightFiresNearPoint;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn2 = ((AttachableThing)t).parent as Pawn;
				if (pawn2 != null)
				{
					return false;
				}
				if (!pawn.CanReserve(t))
				{
					return false;
				}
				if (pawn.story.WorkTagIsDisabled(WorkTags.Firefighting))
				{
					return false;
				}
				return true;
			};
			Thing thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn), maxDistFromPoint, validator);
			if (thing != null)
			{
				return new Job(JobDefOf.BeatFire, thing);
			}
			return null;
		}
	}
}
