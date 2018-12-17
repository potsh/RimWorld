using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_CaravanMeeting : IncidentWorker
	{
		private const int MapSize = 100;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (parms.target is Map)
			{
				return true;
			}
			Faction faction;
			return CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile) && TryFindFaction(out faction);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (parms.target is Map)
			{
				return IncidentDefOf.TravelerGroup.Worker.TryExecute(parms);
			}
			Caravan caravan = (Caravan)parms.target;
			if (!TryFindFaction(out Faction faction))
			{
				return false;
			}
			CameraJumper.TryJumpAndSelect(caravan);
			List<Pawn> pawns = GenerateCaravanPawns(faction);
			Caravan metCaravan = CaravanMaker.MakeCaravan(pawns, faction, -1, addToWorldPawnsIfNotAlready: false);
			string text = "CaravanMeeting".Translate(caravan.Name, faction.Name, PawnUtility.PawnKindsToCommaList(metCaravan.PawnsListForReading, useAnd: true)).CapitalizeFirst();
			DiaNode diaNode = new DiaNode(text);
			Pawn bestPlayerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
			if (metCaravan.CanTradeNow)
			{
				DiaOption diaOption = new DiaOption("CaravanMeeting_Trade".Translate());
				diaOption.action = delegate
				{
					Find.WindowStack.Add(new Dialog_Trade(bestPlayerNegotiator, metCaravan));
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(metCaravan.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithOtherCaravan".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
				};
				if (bestPlayerNegotiator == null)
				{
					diaOption.Disable("CaravanMeeting_TradeIncapable".Translate());
				}
				diaNode.options.Add(diaOption);
			}
			DiaOption diaOption2 = new DiaOption("CaravanMeeting_Attack".Translate());
			diaOption2.action = delegate
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					Pawn t = caravan.PawnsListForReading[0];
					Faction faction3 = faction;
					Faction ofPlayer = Faction.OfPlayer;
					FactionRelationKind kind = FactionRelationKind.Hostile;
					string reason = "GoodwillChangedReason_AttackedCaravan".Translate();
					faction3.TrySetRelationKind(ofPlayer, kind, canSendLetter: true, reason, t);
					Map map = CaravanIncidentUtility.GetOrGenerateMapForIncident(caravan, new IntVec3(100, 1, 100), WorldObjectDefOf.AttackedNonPlayerCaravan);
					MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out IntVec3 playerSpot, out IntVec3 enemySpot);
					CaravanEnterMapUtility.Enter(caravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(playerSpot, map, 12), CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
					List<Pawn> list = metCaravan.PawnsListForReading.ToList();
					CaravanEnterMapUtility.Enter(metCaravan, map, (Pawn p) => CellFinder.RandomClosewalkCellNear(enemySpot, map, 12));
					LordMaker.MakeNewLord(faction, new LordJob_DefendAttackedTraderCaravan(list[0].Position), map, list);
					Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
					CameraJumper.TryJumpAndSelect(t);
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(list, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
				}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
			};
			diaOption2.resolveTree = true;
			diaNode.options.Add(diaOption2);
			DiaOption diaOption3 = new DiaOption("CaravanMeeting_MoveOn".Translate());
			diaOption3.action = delegate
			{
				RemoveAllPawnsAndPassToWorld(metCaravan);
			};
			diaOption3.resolveTree = true;
			diaNode.options.Add(diaOption3);
			string text2 = "CaravanMeetingTitle".Translate(caravan.Label);
			WindowStack windowStack = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction faction2 = faction;
			bool delayInteractivity = true;
			string title = text2;
			windowStack.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, faction2, delayInteractivity, radioMode: false, title));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, text2, faction));
			return true;
		}

		private bool TryFindFaction(out Faction faction)
		{
			return (from x in Find.FactionManager.AllFactionsListForReading
			where !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && !x.def.hidden && x.def.humanlikeFaction && x.def.caravanTraderKinds.Any()
			select x).TryRandomElement(out faction);
		}

		private List<Pawn> GenerateCaravanPawns(Faction faction)
		{
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Trader;
			pawnGroupMakerParms.faction = faction;
			pawnGroupMakerParms.points = TraderCaravanUtility.GenerateGuardPoints();
			pawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
			return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
		}

		private void RemoveAllPawnsAndPassToWorld(Caravan caravan)
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Find.WorldPawns.PassToWorld(pawnsListForReading[i]);
			}
			caravan.RemoveAllPawns();
		}
	}
}
