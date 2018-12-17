using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class VoluntarilyJoinableLordsStarter : IExposable
	{
		private Map map;

		private int lastLordStartTick = -999999;

		private bool startPartyASAP;

		private const int CheckStartPartyIntervalTicks = 5000;

		private const float StartPartyMTBDays = 40f;

		public VoluntarilyJoinableLordsStarter(Map map)
		{
			this.map = map;
		}

		public bool TryStartMarriageCeremony(Pawn firstFiance, Pawn secondFiance)
		{
			if (!RCellFinder.TryFindMarriageSite(firstFiance, secondFiance, out IntVec3 result))
			{
				return false;
			}
			LordMaker.MakeNewLord(firstFiance.Faction, new LordJob_Joinable_MarriageCeremony(firstFiance, secondFiance, result), map);
			Messages.Message("MessageNewMarriageCeremony".Translate(firstFiance.LabelShort, secondFiance.LabelShort, firstFiance.Named("PAWN1"), secondFiance.Named("PAWN2")), new TargetInfo(result, map), MessageTypeDefOf.PositiveEvent);
			lastLordStartTick = Find.TickManager.TicksGame;
			return true;
		}

		public bool TryStartParty()
		{
			Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, map);
			if (pawn == null)
			{
				return false;
			}
			if (!RCellFinder.TryFindPartySpot(pawn, out IntVec3 result))
			{
				return false;
			}
			LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_Party(result, pawn), map);
			Find.LetterStack.ReceiveLetter("LetterLabelNewParty".Translate(), "LetterNewParty".Translate(pawn.LabelShort, pawn), LetterDefOf.PositiveEvent, new TargetInfo(result, map));
			lastLordStartTick = Find.TickManager.TicksGame;
			startPartyASAP = false;
			return true;
		}

		public void VoluntarilyJoinableLordsStarterTick()
		{
			Tick_TryStartParty();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastLordStartTick, "lastLordStartTick", 0);
			Scribe_Values.Look(ref startPartyASAP, "startPartyASAP", defaultValue: false);
		}

		private void Tick_TryStartParty()
		{
			if (map.IsPlayerHome && Find.TickManager.TicksGame % 5000 == 0)
			{
				if (Rand.MTBEventOccurs(40f, 60000f, 5000f))
				{
					startPartyASAP = true;
				}
				if (startPartyASAP && Find.TickManager.TicksGame - lastLordStartTick >= 600000 && PartyUtility.AcceptableGameConditionsToStartParty(map))
				{
					TryStartParty();
				}
			}
		}
	}
}
