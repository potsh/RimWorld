using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_PlayBilliards : JoyGiver_InteractBuilding
	{
		protected override bool CanDoDuringParty => true;

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			if (!ThingHasStandableSpaceOnAllSides(t))
			{
				return null;
			}
			return new Job(def.jobDef, t);
		}

		public static bool ThingHasStandableSpaceOnAllSides(Thing t)
		{
			CellRect cellRect = t.OccupiedRect();
			CellRect.CellRectIterator iterator = cellRect.ExpandedBy(1).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!cellRect.Contains(current) && !current.Standable(t.Map))
				{
					return false;
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}
