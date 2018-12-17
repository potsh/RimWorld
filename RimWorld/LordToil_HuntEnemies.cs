using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_HuntEnemies : LordToil
	{
		private LordToilData_HuntEnemies Data => (LordToilData_HuntEnemies)data;

		public override bool ForceHighStoryDanger => true;

		public LordToil_HuntEnemies(IntVec3 fallbackLocation)
		{
			data = new LordToilData_HuntEnemies();
			Data.fallbackLocation = fallbackLocation;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_HuntEnemies data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (!data.fallbackLocation.IsValid)
				{
					RCellFinder.TryFindRandomSpotJustOutsideColony(lord.ownedPawns[0], out data.fallbackLocation);
				}
				pawn.mindState.duty = new PawnDuty(DutyDefOf.HuntEnemiesIndividual);
				pawn.mindState.duty.focusSecond = data.fallbackLocation;
			}
		}
	}
}
