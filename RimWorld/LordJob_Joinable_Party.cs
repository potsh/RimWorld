using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Joinable_Party : LordJob_VoluntarilyJoinable
	{
		private IntVec3 spot;

		private Pawn organizer;

		private Trigger_TicksPassed timeoutTrigger;

		public override bool AllowStartNewGatherings => false;

		public Pawn Organizer => organizer;

		public LordJob_Joinable_Party()
		{
		}

		public LordJob_Joinable_Party(IntVec3 spot, Pawn organizer)
		{
			this.spot = spot;
			this.organizer = organizer;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Party lordToil_Party = new LordToil_Party(spot);
			stateGraph.AddToil(lordToil_Party);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_Party, lordToil_End);
			transition.AddTrigger(new Trigger_TickCondition(() => ShouldBeCalledOff()));
			transition.AddTrigger(new Trigger_PawnKilled());
			transition.AddPreAction(new TransitionAction_Message("MessagePartyCalledOff".Translate(), MessageTypeDefOf.NegativeEvent, new TargetInfo(spot, base.Map)));
			stateGraph.AddTransition(transition);
			timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 15000));
			Transition transition2 = new Transition(lordToil_Party, lordToil_End);
			transition2.AddTrigger(timeoutTrigger);
			transition2.AddPreAction(new TransitionAction_Message("MessagePartyFinished".Translate(), MessageTypeDefOf.SituationResolved, new TargetInfo(spot, base.Map)));
			stateGraph.AddTransition(transition2);
			return stateGraph;
		}

		private bool ShouldBeCalledOff()
		{
			if (!PartyUtility.AcceptableGameConditionsToContinueParty(base.Map))
			{
				return true;
			}
			if (!spot.Roofed(base.Map) && !JoyUtility.EnjoyableOutsideNow(base.Map))
			{
				return true;
			}
			return false;
		}

		public override float VoluntaryJoinPriorityFor(Pawn p)
		{
			if (IsInvited(p))
			{
				if (!PartyUtility.ShouldPawnKeepPartying(p))
				{
					return 0f;
				}
				if (spot.IsForbidden(p))
				{
					return 0f;
				}
				if (!lord.ownedPawns.Contains(p) && IsPartyAboutToEnd())
				{
					return 0f;
				}
				return VoluntarilyJoinableLordJobJoinPriorities.PartyGuest;
			}
			return 0f;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref spot, "spot");
			Scribe_References.Look(ref organizer, "organizer");
		}

		public override string GetReport()
		{
			return "LordReportAttendingParty".Translate();
		}

		private bool IsPartyAboutToEnd()
		{
			if (timeoutTrigger.TicksLeft < 1200)
			{
				return true;
			}
			return false;
		}

		private bool IsInvited(Pawn p)
		{
			return lord.faction != null && p.Faction == lord.faction;
		}
	}
}
