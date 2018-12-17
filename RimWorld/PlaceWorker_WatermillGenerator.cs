using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_WatermillGenerator : PlaceWorker
	{
		private static List<Thing> waterMills = new List<Thing>();

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			foreach (IntVec3 item in CompPowerPlantWater.GroundCells(loc, rot))
			{
				if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.Heavy))
				{
					return new AcceptanceReport("TerrainCannotSupport_TerrainAffordance".Translate(checkingDef, TerrainAffordanceDefOf.Heavy));
				}
			}
			if (!WaterCellsPresent(loc, rot, map))
			{
				return new AcceptanceReport("MustBeOnMovingWater".Translate());
			}
			return true;
		}

		private bool WaterCellsPresent(IntVec3 loc, Rot4 rot, Map map)
		{
			foreach (IntVec3 item in CompPowerPlantWater.WaterCells(loc, rot))
			{
				if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
				{
					return false;
				}
			}
			return true;
		}

		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol)
		{
			GenDraw.DrawFieldEdges(CompPowerPlantWater.GroundCells(loc, rot).ToList(), Color.white);
			Color color = (!WaterCellsPresent(loc, rot, Find.CurrentMap)) ? Designator_Place.CannotPlaceColor.ToOpaque() : Designator_Place.CanPlaceColor.ToOpaque();
			GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterCells(loc, rot).ToList(), color);
			bool flag = false;
			CellRect cellRect = CompPowerPlantWater.WaterUseRect(loc, rot);
			waterMills.AddRange(Find.CurrentMap.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator).Cast<Thing>());
			waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
			where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
			select t);
			waterMills.AddRange(from t in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
			where t.def.entityDefToBuild == ThingDefOf.WatermillGenerator
			select t);
			foreach (Thing waterMill in waterMills)
			{
				GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(waterMill.Position, waterMill.Rotation).ToList(), new Color(0.2f, 0.2f, 1f));
				if (cellRect.Overlaps(CompPowerPlantWater.WaterUseRect(waterMill.Position, waterMill.Rotation)))
				{
					flag = true;
				}
			}
			waterMills.Clear();
			Color color2 = (!flag) ? Designator_Place.CanPlaceColor.ToOpaque() : new Color(1f, 0.6f, 0f);
			if (!flag || Time.realtimeSinceStartup % 0.4f < 0.2f)
			{
				GenDraw.DrawFieldEdges(CompPowerPlantWater.WaterUseCells(loc, rot).ToList(), color2);
			}
		}

		public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
		{
			yield return TerrainAffordanceDefOf.Heavy;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
