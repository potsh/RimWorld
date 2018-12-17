using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class GenGuest
	{
		public static void PrisonerRelease(Pawn p)
		{
			if (p.ownership != null)
			{
				p.ownership.UnclaimAll();
			}
			if (p.Faction == Faction.OfPlayer || p.IsWildMan())
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasImprisoned);
				p.guest.SetGuestStatus(null);
				if (p.IsWildMan())
				{
					p.mindState.WildManEverReachedOutside = false;
				}
			}
			else
			{
				p.guest.Released = true;
				if (RCellFinder.TryFindBestExitSpot(p, out IntVec3 spot))
				{
					Job job = new Job(JobDefOf.Goto, spot);
					job.exitMapOnArrival = true;
					p.jobs.StartJob(job);
				}
			}
		}

		public static void AddPrisonerSoldThoughts(Pawn prisoner)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowPrisonerSold);
			}
		}
	}
}
