using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_AttackSettlement : TransportPodsArrivalAction
	{
		private SettlementBase settlement;

		private PawnsArrivalModeDef arrivalMode;

		public TransportPodsArrivalAction_AttackSettlement()
		{
		}

		public TransportPodsArrivalAction_AttackSettlement(SettlementBase settlement, PawnsArrivalModeDef arrivalMode)
		{
			this.settlement = settlement;
			this.arrivalMode = arrivalMode;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
			Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!(bool)floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (settlement != null && settlement.Tile != destinationTile)
			{
				return false;
			}
			return CanAttack(pods, settlement);
		}

		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !settlement.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			bool flag = !settlement.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
			string letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
			string letterText = "LetterTransportPodsLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
			SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
			if (flag)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget, settlement.Faction);
			arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
		}

		public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, SettlementBase settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Attackable)
			{
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyNonDownedColonist(pods))
			{
				return false;
			}
			if (settlement.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownDaysLeft().ToString("0.#")));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, SettlementBase settlement)
		{
			using (IEnumerator<FloatMenuOption> enumerator = TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportPodsArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop), "AttackAndDropAtEdge".Translate(settlement.Label), representative, settlement.Tile).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption f2 = enumerator.Current;
					yield return f2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<FloatMenuOption> enumerator2 = TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportPodsArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop), "AttackAndDropInCenter".Translate(settlement.Label), representative, settlement.Tile).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					FloatMenuOption f = enumerator2.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0219:
			/*Error near IL_021a: Unexpected return in MoveNext()*/;
		}
	}
}
