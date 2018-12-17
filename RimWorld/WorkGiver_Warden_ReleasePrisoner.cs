using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_ReleasePrisoner : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (pawn2.guest.interactionMode == PrisonerInteractionModeDefOf.Release && !pawn2.Downed && pawn2.Awake())
			{
				if (!RCellFinder.TryFindPrisonerReleaseCell(pawn2, pawn, out IntVec3 result))
				{
					return null;
				}
				Job job = new Job(JobDefOf.ReleasePrisoner, pawn2, result);
				job.count = 1;
				return job;
			}
			return null;
		}
	}
}
