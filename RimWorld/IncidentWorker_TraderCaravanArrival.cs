using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_TraderCaravanArrival : IncidentWorker_NeutralGroup
	{
		protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Trader;

		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && f.def.caravanTraderKinds.Any();
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			if (parms.faction != null && NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, parms.faction))
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryResolveParms(parms))
			{
				return false;
			}
			if (parms.faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			List<Pawn> list = SpawnPawns(parms);
			if (list.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].needs != null && list[i].needs.food != null)
				{
					list[i].needs.food.CurLevel = list[i].needs.food.MaxLevel;
				}
			}
			TraderKindDef traderKindDef = null;
			for (int j = 0; j < list.Count; j++)
			{
				Pawn pawn = list[j];
				if (pawn.TraderKind != null)
				{
					traderKindDef = pawn.TraderKind;
					break;
				}
			}
			string letterLabel = "LetterLabelTraderCaravanArrival".Translate(parms.faction.Name, traderKindDef.label).CapitalizeFirst();
			string letterText = "LetterTraderCaravanArrival".Translate(parms.faction.Name, traderKindDef.label).CapitalizeFirst();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, list[0], parms.faction);
			RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out IntVec3 result);
			LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(parms.faction, result);
			LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
			return true;
		}

		protected override void ResolveParmsPoints(IncidentParms parms)
		{
			parms.points = TraderCaravanUtility.GenerateGuardPoints();
		}
	}
}
