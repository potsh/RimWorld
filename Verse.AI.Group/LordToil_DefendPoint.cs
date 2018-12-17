using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_DefendPoint : LordToil
	{
		private bool allowSatisfyLongNeeds = true;

		protected LordToilData_DefendPoint Data => (LordToilData_DefendPoint)data;

		public override IntVec3 FlagLoc => Data.defendPoint;

		public override bool AllowSatisfyLongNeeds => allowSatisfyLongNeeds;

		public LordToil_DefendPoint(bool canSatisfyLongNeeds = true)
		{
			allowSatisfyLongNeeds = canSatisfyLongNeeds;
			data = new LordToilData_DefendPoint();
		}

		public LordToil_DefendPoint(IntVec3 defendPoint, float defendRadius = 28f)
			: this()
		{
			Data.defendPoint = defendPoint;
			Data.defendRadius = defendRadius;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_DefendPoint data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint);
				lord.ownedPawns[i].mindState.duty.focusSecond = data.defendPoint;
				lord.ownedPawns[i].mindState.duty.radius = data.defendRadius;
			}
		}

		public void SetDefendPoint(IntVec3 defendPoint)
		{
			Data.defendPoint = defendPoint;
		}
	}
}
