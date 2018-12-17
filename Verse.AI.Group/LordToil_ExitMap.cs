using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMap : LordToil
	{
		public override bool AllowSatisfyLongNeeds => false;

		public override bool AllowSelfTend => false;

		protected LordToilData_ExitMap Data => (LordToilData_ExitMap)data;

		public LordToil_ExitMap(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false)
		{
			data = new LordToilData_ExitMap();
			Data.locomotion = locomotion;
			Data.canDig = canDig;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_ExitMap data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.ExitMapBest);
				pawnDuty.locomotion = data.locomotion;
				pawnDuty.canDig = data.canDig;
				lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}
	}
}
