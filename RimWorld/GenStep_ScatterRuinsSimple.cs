using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterRuinsSimple : GenStep_Scatterer
	{
		public IntRange ShedSizeRange = new IntRange(3, 10);

		public IntRange WallLengthRange = new IntRange(4, 14);

		public override int SeedPart => 1348417666;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			return true;
		}

		protected bool CanPlaceAncientBuildingInRange(CellRect rect, Map map)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				if (cell.InBounds(map))
				{
					TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
					if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			ThingDef stuffDef = BaseGenUtility.RandomCheapWallStuff(null, notVeryFlammable: true);
			if (Rand.Bool)
			{
				bool @bool = Rand.Bool;
				int randomInRange = WallLengthRange.RandomInRange;
				CellRect cellRect = new CellRect(c.x, c.z, (!@bool) ? 1 : randomInRange, @bool ? 1 : randomInRange);
				if (CanPlaceAncientBuildingInRange(cellRect.ExpandedBy(1), map))
				{
					MakeLongWall(c, map, WallLengthRange.RandomInRange, @bool, stuffDef);
				}
			}
			else
			{
				CellRect rect = new CellRect(c.x, c.z, ShedSizeRange.RandomInRange, ShedSizeRange.RandomInRange).ClipInsideMap(map);
				if (CanPlaceAncientBuildingInRange(rect, map))
				{
					RimWorld.BaseGen.BaseGen.globalSettings.map = map;
					RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientRuins", rect);
					RimWorld.BaseGen.BaseGen.Generate();
				}
			}
		}

		private void TrySetCellAsWall(IntVec3 c, Map map, ThingDef stuffDef)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable)
				{
					return;
				}
			}
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				thingList[num].Destroy();
			}
			map.terrainGrid.SetTerrain(c, BaseGenUtility.CorrespondingTerrainDef(stuffDef, beautiful: true));
			Thing newThing = ThingMaker.MakeThing(ThingDefOf.Wall, stuffDef);
			GenSpawn.Spawn(newThing, c, map);
		}

		private void MakeLongWall(IntVec3 start, Map map, int extendDist, bool horizontal, ThingDef stuffDef)
		{
			TerrainDef newTerr = BaseGenUtility.CorrespondingTerrainDef(stuffDef, beautiful: true);
			IntVec3 intVec = start;
			for (int i = 0; i < extendDist; i++)
			{
				if (!intVec.InBounds(map))
				{
					break;
				}
				TrySetCellAsWall(intVec, map, stuffDef);
				if (Rand.Chance(0.4f))
				{
					for (int j = 0; j < 9; j++)
					{
						IntVec3 c = intVec + GenAdj.AdjacentCellsAndInside[j];
						if (c.InBounds(map) && Rand.Bool)
						{
							map.terrainGrid.SetTerrain(c, newTerr);
						}
					}
				}
				if (horizontal)
				{
					intVec.x++;
				}
				else
				{
					intVec.z++;
				}
			}
		}
	}
}
