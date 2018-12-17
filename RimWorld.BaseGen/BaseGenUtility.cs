using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public static class BaseGenUtility
	{
		private static List<IntVec3> bridgeCells = new List<IntVec3>();

		public static ThingDef RandomCheapWallStuff(Faction faction, bool notVeryFlammable = false)
		{
			TechLevel techLevel = faction?.def.techLevel ?? TechLevel.Spacer;
			return RandomCheapWallStuff(techLevel, notVeryFlammable);
		}

		public static ThingDef RandomCheapWallStuff(TechLevel techLevel, bool notVeryFlammable = false)
		{
			if (techLevel.IsNeolithicOrWorse())
			{
				return ThingDefOf.WoodLog;
			}
			return (from d in DefDatabase<ThingDef>.AllDefsListForReading
			where IsCheapWallStuff(d) && (!notVeryFlammable || d.BaseFlammability < 0.5f)
			select d).RandomElement();
		}

		public static bool IsCheapWallStuff(ThingDef d)
		{
			return d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall) && d.BaseMarketValue / d.VolumePerUnit < 5f;
		}

		public static ThingDef RandomHightechWallStuff()
		{
			if (Rand.Value < 0.15f)
			{
				return ThingDefOf.Plasteel;
			}
			return ThingDefOf.Steel;
		}

		public static TerrainDef RandomHightechFloorDef()
		{
			return Rand.Element(TerrainDefOf.Concrete, TerrainDefOf.Concrete, TerrainDefOf.PavedTile, TerrainDefOf.PavedTile, TerrainDefOf.MetalTile);
		}

		public static TerrainDef RandomBasicFloorDef(Faction faction, bool allowCarpet = false)
		{
			if (allowCarpet && (faction == null || !faction.def.techLevel.IsNeolithicOrWorse()) && Rand.Chance(0.1f))
			{
				return (from x in DefDatabase<TerrainDef>.AllDefsListForReading
				where x.IsCarpet
				select x).RandomElement();
			}
			return Rand.Element(TerrainDefOf.MetalTile, TerrainDefOf.PavedTile, TerrainDefOf.WoodPlankFloor, TerrainDefOf.TileSandstone);
		}

		public static TerrainDef CorrespondingTerrainDef(ThingDef stuffDef, bool beautiful)
		{
			TerrainDef terrainDef = null;
			List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].costList != null)
				{
					for (int j = 0; j < allDefsListForReading[i].costList.Count; j++)
					{
						if (allDefsListForReading[i].costList[j].thingDef == stuffDef && (terrainDef == null || ((!beautiful) ? (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)) : (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) < allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)))))
						{
							terrainDef = allDefsListForReading[i];
						}
					}
				}
			}
			if (terrainDef == null)
			{
				terrainDef = TerrainDefOf.Concrete;
			}
			return terrainDef;
		}

		public static TerrainDef RegionalRockTerrainDef(int tile, bool beautiful)
		{
			ThingDef thingDef = Find.World.NaturalRockTypesIn(tile).RandomElementWithFallback()?.building.mineableThing;
			ThingDef stuffDef = (thingDef == null || thingDef.butcherProducts == null || thingDef.butcherProducts.Count <= 0) ? null : thingDef.butcherProducts[0].thingDef;
			return CorrespondingTerrainDef(stuffDef, beautiful);
		}

		public static bool AnyDoorAdjacentCardinalTo(IntVec3 cell, Map map)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = cell + GenAdj.CardinalDirections[i];
				if (c.InBounds(map) && c.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AnyDoorAdjacentCardinalTo(CellRect rect, Map map)
		{
			foreach (IntVec3 item in rect.AdjacentCellsCardinal)
			{
				if (item.InBounds(map) && item.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static ThingDef WallStuffAt(IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def == ThingDefOf.Wall)
			{
				return edifice.Stuff;
			}
			return null;
		}

		public static void CheckSpawnBridgeUnder(ThingDef thingDef, IntVec3 c, Rot4 rot)
		{
			if (thingDef.category == ThingCategory.Building)
			{
				Map map = BaseGen.globalSettings.map;
				CellRect cellRect = GenAdj.OccupiedRect(c, rot, thingDef.size);
				bridgeCells.Clear();
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (!iterator.Current.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, iterator.Current, map, Rot4.North))
					{
						bridgeCells.Add(iterator.Current);
					}
					iterator.MoveNext();
				}
				if (bridgeCells.Any())
				{
					if (thingDef.size.x != 1 || thingDef.size.z != 1)
					{
						for (int num = bridgeCells.Count - 1; num >= 0; num--)
						{
							for (int i = 0; i < 8; i++)
							{
								IntVec3 intVec = bridgeCells[num] + GenAdj.AdjacentCells[i];
								if (!bridgeCells.Contains(intVec) && intVec.InBounds(map) && !intVec.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, intVec, map, Rot4.North))
								{
									bridgeCells.Add(intVec);
								}
							}
						}
					}
					for (int j = 0; j < bridgeCells.Count; j++)
					{
						map.terrainGrid.SetTerrain(bridgeCells[j], TerrainDefOf.Bridge);
					}
				}
			}
		}
	}
}
