using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Party : LordToilData
	{
		public int ticksToNextPulse;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref ticksToNextPulse, "ticksToNextPulse", 0);
		}
	}
}
