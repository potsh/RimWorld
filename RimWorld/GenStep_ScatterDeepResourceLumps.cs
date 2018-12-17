using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterDeepResourceLumps : GenStep_Scatterer
	{
		public override int SeedPart => 1712041303;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!map.TileInfo.WaterCovered)
			{
				int num = CalculateFinalCount(map);
				for (int i = 0; i < num; i++)
				{
					if (!TryFindScatterCell(map, out IntVec3 result))
					{
						return;
					}
					ScatterAt(result, map);
					usedSpots.Add(result);
				}
				usedSpots.Clear();
			}
		}

		protected ThingDef ChooseThingDef()
		{
			return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight((ThingDef def) => def.deepCommonality);
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (NearUsedSpot(c, minSpacing))
			{
				return false;
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			ThingDef thingDef = ChooseThingDef();
			int numCells = Mathf.CeilToInt((float)thingDef.deepLumpSizeRange.RandomInRange);
			foreach (IntVec3 item in GridShapeMaker.IrregularLump(c, map, numCells))
			{
				if (!item.InNoBuildEdgeArea(map))
				{
					map.deepResourceGrid.SetAt(item, thingDef, thingDef.deepCountPerCell);
				}
			}
		}
	}
}
