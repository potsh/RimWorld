using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Grower : WorkGiver_Scanner
	{
		protected static ThingDef wantedPlantDef;

		public override bool AllowUnreachable => true;

		protected virtual bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
		{
			return true;
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			Danger maxDanger = pawn.NormalMaxDanger();
			List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int k = 0; k < bList.Count; k++)
			{
				Building_PlantGrower b = bList[k] as Building_PlantGrower;
				if (b != null && ExtraRequirements(b, pawn) && !b.IsForbidden(pawn) && pawn.CanReach(b, PathEndMode.OnCell, maxDanger) && !b.IsBurning())
				{
					CellRect.CellRectIterator cri = b.OccupiedRect().GetIterator();
					if (!cri.Done())
					{
						yield return cri.Current;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					wantedPlantDef = null;
				}
			}
			wantedPlantDef = null;
			List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
			for (int j = 0; j < zonesList.Count; j++)
			{
				Zone_Growing growZone = zonesList[j] as Zone_Growing;
				if (growZone != null)
				{
					if (growZone.cells.Count == 0)
					{
						Log.ErrorOnce("Grow zone has 0 cells: " + growZone, -563487);
					}
					else if (ExtraRequirements(growZone, pawn) && !growZone.ContainsStaticFire && pawn.CanReach(growZone.Cells[0], PathEndMode.OnCell, maxDanger))
					{
						int i = 0;
						if (i < growZone.cells.Count)
						{
							yield return growZone.cells[i];
							/*Error: Unable to find new state assignment for yield return*/;
						}
						wantedPlantDef = null;
					}
				}
			}
			wantedPlantDef = null;
		}

		public static ThingDef CalculateWantedPlantDef(IntVec3 c, Map map)
		{
			return c.GetPlantToGrowSettable(map)?.GetPlantDefToGrow();
		}
	}
}
