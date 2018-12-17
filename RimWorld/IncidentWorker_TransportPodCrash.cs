using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_TransportPodCrash : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> things = ThingSetMakerDefOf.RefugeePod.root.Generate();
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			Pawn pawn = FindPawn(things);
			pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
			string title = "LetterLabelRefugeePodCrash".Translate();
			string str = "RefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			str += "\n\n";
			str = ((pawn.Faction == null) ? (str + "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn)) : ((!pawn.Faction.HostileTo(Faction.OfPlayer)) ? (str + "RefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn)) : (str + "RefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn))));
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref str, ref title, pawn);
			Find.LetterStack.ReceiveLetter(title, str, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map));
			ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
			activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things);
			activeDropPodInfo.openDelay = 180;
			activeDropPodInfo.leaveSlag = true;
			DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);
			return true;
		}

		private Pawn FindPawn(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				Pawn pawn = things[i] as Pawn;
				if (pawn != null)
				{
					return pawn;
				}
				Corpse corpse = things[i] as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
			}
			return null;
		}
	}
}
