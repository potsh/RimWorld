using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Party : LordToil
	{
		private IntVec3 spot;

		private int ticksPerPartyPulse = 600;

		private const int DefaultTicksPerPartyPulse = 600;

		private LordToilData_Party Data => (LordToilData_Party)data;

		public LordToil_Party(IntVec3 spot, int ticksPerPartyPulse = 600)
		{
			this.spot = spot;
			this.ticksPerPartyPulse = ticksPerPartyPulse;
			data = new LordToilData_Party();
			Data.ticksToNextPulse = ticksPerPartyPulse;
		}

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return DutyDefOf.Party.hook;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Party, spot);
			}
		}

		public override void LordToilTick()
		{
			if (--Data.ticksToNextPulse <= 0)
			{
				Data.ticksToNextPulse = ticksPerPartyPulse;
				List<Pawn> ownedPawns = lord.ownedPawns;
				for (int i = 0; i < ownedPawns.Count; i++)
				{
					if (PartyUtility.InPartyArea(ownedPawns[i].Position, spot, base.Map))
					{
						ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AttendedParty);
						LordJob_Joinable_Party lordJob_Joinable_Party = lord.LordJob as LordJob_Joinable_Party;
						if (lordJob_Joinable_Party != null)
						{
							TaleRecorder.RecordTale(TaleDefOf.AttendedParty, ownedPawns[i], lordJob_Joinable_Party.Organizer);
						}
					}
				}
			}
		}
	}
}
