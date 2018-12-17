using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderColony : JobGiver_Wander
	{
		private static List<IntVec3> gatherSpots = new List<IntVec3>();

		public JobGiver_WanderColony()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
			wanderDestValidator = ((Pawn pawn, IntVec3 loc, IntVec3 root) => true);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike)
			{
				gatherSpots.Clear();
				for (int i = 0; i < pawn.Map.gatherSpotLister.activeSpots.Count; i++)
				{
					IntVec3 position = pawn.Map.gatherSpotLister.activeSpots[i].parent.Position;
					if (!position.IsForbidden(pawn) && pawn.CanReach(position, PathEndMode.Touch, Danger.None))
					{
						gatherSpots.Add(position);
					}
				}
				if (gatherSpots.Count > 0)
				{
					return gatherSpots.RandomElement();
				}
			}
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			int num;
			if (allBuildingsColonist.Count > 0)
			{
				num = 0;
				goto IL_00f3;
			}
			goto IL_022c;
			IL_022c:
			if ((from c in pawn.Map.mapPawns.FreeColonistsSpawned
			where !c.Position.IsForbidden(pawn) && pawn.CanReach(c.Position, PathEndMode.Touch, Danger.None)
			select c).TryRandomElement(out Pawn result))
			{
				return result.Position;
			}
			return pawn.Position;
			IL_0227:
			goto IL_00f3;
			IL_00f3:
			while (true)
			{
				num++;
				if (num > 20)
				{
					break;
				}
				Building building = allBuildingsColonist.RandomElement();
				if ((building.def == ThingDefOf.Wall || building.def.building.ai_chillDestination) && !building.Position.IsForbidden(pawn) && pawn.Map.areaManager.Home[building.Position])
				{
					int num2 = 15 + num * 2;
					if ((pawn.Position - building.Position).LengthHorizontalSquared <= num2 * num2)
					{
						IntVec3 intVec = GenAdjFast.AdjacentCells8Way(building).RandomElement();
						if (intVec.Standable(building.Map) && !intVec.IsForbidden(pawn) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.None) && !intVec.IsInPrisonCell(pawn.Map))
						{
							return intVec;
						}
					}
				}
			}
			goto IL_022c;
		}
	}
}
