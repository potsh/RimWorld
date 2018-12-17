using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitEscapeShip : CaravanArrivalAction
	{
		private MapParent target;

		public override string Label => "VisitEscapeShip".Translate(target.Label);

		public override string ReportString => "CaravanVisiting".Translate(target.Label);

		public CaravanArrivalAction_VisitEscapeShip()
		{
		}

		public CaravanArrivalAction_VisitEscapeShip(EscapeShipComp escapeShip)
		{
			target = (MapParent)escapeShip.parent;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!(bool)floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (target != null && target.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(caravan, target);
		}

		public override void Arrived(Caravan caravan)
		{
			if (!target.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					DoArrivalAction(caravan);
				}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
			}
			else
			{
				DoArrivalAction(caravan);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
		}

		private void DoArrivalAction(Caravan caravan)
		{
			bool flag = !target.HasMap;
			if (flag)
			{
				target.SetFaction(Faction.OfPlayer);
			}
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(target.Tile, null);
			Pawn t = caravan.PawnsListForReading[0];
			CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.UnloadIndividually);
			if (flag)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				Find.LetterStack.ReceiveLetter("EscapeShipFoundLabel".Translate(), "EscapeShipFound".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(target.Map.Center, target.Map));
			}
			else
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(target), "LetterCaravanEnteredMap".Translate(caravan.Label, target).CapitalizeFirst(), LetterDefOf.NeutralEvent, t);
			}
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, MapParent escapeShip)
		{
			if (escapeShip == null || !escapeShip.Spawned || escapeShip.GetComponent<EscapeShipComp>() == null)
			{
				return false;
			}
			if (escapeShip.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(escapeShip.EnterCooldownDaysLeft().ToString("0.#")));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MapParent escapeShip)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, escapeShip), () => new CaravanArrivalAction_VisitEscapeShip(escapeShip.GetComponent<EscapeShipComp>()), "VisitEscapeShip".Translate(escapeShip.Label), caravan, escapeShip.Tile, escapeShip);
		}
	}
}
