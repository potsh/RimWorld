using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_JumpInWater : ThinkNode_JobGiver
	{
		private const float ActivateChance = 1f;

		private readonly IntRange MaxDistance = new IntRange(10, 16);

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 1f)
			{
				IntVec3 position = pawn.Position;
				Predicate<IntVec3> validator = (IntVec3 pos) => pos.GetTerrain(pawn.Map).extinguishesFire;
				Map map = pawn.Map;
				int randomInRange = MaxDistance.RandomInRange;
				IntVec3 result = default(IntVec3);
				if (RCellFinder.TryFindRandomCellNearWith(position, validator, map, out result, 5, randomInRange))
				{
					return new Job(JobDefOf.Goto, result);
				}
			}
			return null;
		}
	}
}
