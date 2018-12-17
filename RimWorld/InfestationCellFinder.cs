using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InfestationCellFinder
	{
		private struct LocationCandidate
		{
			public IntVec3 cell;

			public float score;

			public LocationCandidate(IntVec3 cell, float score)
			{
				this.cell = cell;
				this.score = score;
			}
		}

		private static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();

		private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();

		private static ByteGrid closedAreaSize;

		private static ByteGrid distToColonyBuilding;

		private const float MinRequiredScore = 7.5f;

		private const float MinMountainousnessScore = 0.17f;

		private const int MountainousnessScoreRadialPatternIdx = 700;

		private const int MountainousnessScoreRadialPatternSkip = 10;

		private const float MountainousnessScorePerRock = 1f;

		private const float MountainousnessScorePerThickRoof = 0.5f;

		private const float MinCellTempToSpawnHive = -17f;

		private const float MaxDistanceToColonyBuilding = 30f;

		private static List<Pair<IntVec3, float>> tmpCachedInfestationChanceCellColors;

		private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();

		private static List<IntVec3> tmpColonyBuildingsLocs = new List<IntVec3>();

		private static List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

		public static bool TryFindCell(out IntVec3 cell, Map map)
		{
			CalculateLocationCandidates(map);
			if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out LocationCandidate result))
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = CellFinder.FindNoWipeSpawnLocNear(result.cell, map, ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
			return true;
		}

		private static float GetScoreAt(IntVec3 cell, Map map)
		{
			if ((float)(int)distToColonyBuilding[cell] > 30f)
			{
				return 0f;
			}
			if (!cell.Walkable(map))
			{
				return 0f;
			}
			if (cell.Fogged(map))
			{
				return 0f;
			}
			if (CellHasBlockingThings(cell, map))
			{
				return 0f;
			}
			if (!cell.Roofed(map) || !cell.GetRoof(map).isThickRoof)
			{
				return 0f;
			}
			Region region = cell.GetRegion(map);
			if (region == null)
			{
				return 0f;
			}
			if (closedAreaSize[cell] < 2)
			{
				return 0f;
			}
			float temperature = cell.GetTemperature(map);
			if (temperature < -17f)
			{
				return 0f;
			}
			float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
			if (mountainousnessScoreAt < 0.17f)
			{
				return 0f;
			}
			int num = StraightLineDistToUnroofed(cell, map);
			float value = regionsDistanceToUnroofed.TryGetValue(region, out value) ? Mathf.Min(value, (float)num * 4f) : ((float)num * 1.15f);
			value = Mathf.Pow(value, 1.55f);
			float num2 = Mathf.InverseLerp(0f, 12f, (float)num);
			float num3 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GameGlowAt(cell));
			float num4 = 1f - Mathf.Clamp(DistToBlocker(cell, map) / 11f, 0f, 0.6f);
			float num5 = Mathf.InverseLerp(-17f, -7f, temperature);
			float f = value * num2 * num4 * mountainousnessScoreAt * num3 * num5;
			f = Mathf.Pow(f, 1.2f);
			if (f < 7.5f)
			{
				return 0f;
			}
			return f;
		}

		public static void DebugDraw()
		{
			if (DebugViewSettings.drawInfestationChance)
			{
				if (tmpCachedInfestationChanceCellColors == null)
				{
					tmpCachedInfestationChanceCellColors = new List<Pair<IntVec3, float>>();
				}
				if (Time.frameCount % 8 == 0)
				{
					tmpCachedInfestationChanceCellColors.Clear();
					Map currentMap = Find.CurrentMap;
					CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
					currentViewRect.ClipInsideMap(currentMap);
					currentViewRect = currentViewRect.ExpandedBy(1);
					CalculateTraversalDistancesToUnroofed(currentMap);
					CalculateClosedAreaSizeGrid(currentMap);
					CalculateDistanceToColonyBuildingGrid(currentMap);
					float num = 0.001f;
					int num2 = 0;
					while (true)
					{
						int num3 = num2;
						IntVec3 size = currentMap.Size;
						if (num3 >= size.z)
						{
							break;
						}
						int num4 = 0;
						while (true)
						{
							int num5 = num4;
							IntVec3 size2 = currentMap.Size;
							if (num5 >= size2.x)
							{
								break;
							}
							IntVec3 cell = new IntVec3(num4, 0, num2);
							float scoreAt = GetScoreAt(cell, currentMap);
							if (scoreAt > num)
							{
								num = scoreAt;
							}
							num4++;
						}
						num2++;
					}
					int num6 = 0;
					while (true)
					{
						int num7 = num6;
						IntVec3 size3 = currentMap.Size;
						if (num7 >= size3.z)
						{
							break;
						}
						int num8 = 0;
						while (true)
						{
							int num9 = num8;
							IntVec3 size4 = currentMap.Size;
							if (num9 >= size4.x)
							{
								break;
							}
							IntVec3 intVec = new IntVec3(num8, 0, num6);
							if (currentViewRect.Contains(intVec))
							{
								float scoreAt2 = GetScoreAt(intVec, currentMap);
								if (!(scoreAt2 <= 7.5f))
								{
									float second = GenMath.LerpDouble(7.5f, num, 0f, 1f, scoreAt2);
									tmpCachedInfestationChanceCellColors.Add(new Pair<IntVec3, float>(intVec, second));
								}
							}
							num8++;
						}
						num6++;
					}
				}
				for (int i = 0; i < tmpCachedInfestationChanceCellColors.Count; i++)
				{
					IntVec3 first = tmpCachedInfestationChanceCellColors[i].First;
					float second2 = tmpCachedInfestationChanceCellColors[i].Second;
					CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2)));
				}
			}
			else
			{
				tmpCachedInfestationChanceCellColors = null;
			}
		}

		private static void CalculateLocationCandidates(Map map)
		{
			locationCandidates.Clear();
			CalculateTraversalDistancesToUnroofed(map);
			CalculateClosedAreaSizeGrid(map);
			CalculateDistanceToColonyBuildingGrid(map);
			int num = 0;
			while (true)
			{
				int num2 = num;
				IntVec3 size = map.Size;
				if (num2 >= size.z)
				{
					break;
				}
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					IntVec3 size2 = map.Size;
					if (num4 >= size2.x)
					{
						break;
					}
					IntVec3 cell = new IntVec3(num3, 0, num);
					float scoreAt = GetScoreAt(cell, map);
					if (!(scoreAt <= 0f))
					{
						locationCandidates.Add(new LocationCandidate(cell, scoreAt));
					}
					num3++;
				}
				num++;
			}
		}

		private static bool CellHasBlockingThings(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn || thingList[i] is Hive || thingList[i] is TunnelHiveSpawner)
				{
					return true;
				}
				if (thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable && GenSpawn.SpawningWipes(ThingDefOf.Hive, thingList[i].def))
				{
					return true;
				}
			}
			return false;
		}

		private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
		{
			int num = 2147483647;
			for (int i = 0; i < 4; i++)
			{
				int num2 = 0;
				IntVec3 facingCell = new Rot4(i).FacingCell;
				int num3 = 0;
				while (true)
				{
					IntVec3 intVec = cell + facingCell * num3;
					if (!intVec.InBounds(map))
					{
						num2 = 2147483647;
						break;
					}
					num2 = num3;
					if (NoRoofAroundAndWalkable(intVec, map))
					{
						break;
					}
					num3++;
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			if (num == 2147483647)
			{
				IntVec3 size = map.Size;
				return size.x;
			}
			return num;
		}

		private static float DistToBlocker(IntVec3 cell, Map map)
		{
			int num = -2147483648;
			int num2 = -2147483648;
			for (int i = 0; i < 4; i++)
			{
				int num3 = 0;
				IntVec3 facingCell = new Rot4(i).FacingCell;
				int num4 = 0;
				while (true)
				{
					IntVec3 c = cell + facingCell * num4;
					num3 = num4;
					if (!c.InBounds(map) || !c.Walkable(map))
					{
						break;
					}
					num4++;
				}
				if (num3 > num)
				{
					num2 = num;
					num = num3;
				}
				else if (num3 > num2)
				{
					num2 = num3;
				}
			}
			return (float)Mathf.Min(num, num2);
		}

		private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
		{
			if (!cell.Walkable(map))
			{
				return false;
			}
			if (cell.Roofed(map))
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = new Rot4(i).FacingCell + cell;
				if (c.InBounds(map) && c.Roofed(map))
				{
					return false;
				}
			}
			return true;
		}

		private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < 700; i += 10)
			{
				IntVec3 c = cell + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
					{
						num += 1f;
					}
					else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
					{
						num += 0.5f;
					}
					num2++;
				}
			}
			return num / (float)num2;
		}

		private static void CalculateTraversalDistancesToUnroofed(Map map)
		{
			tempUnroofedRegions.Clear();
			int num = 0;
			while (true)
			{
				int num2 = num;
				IntVec3 size = map.Size;
				if (num2 >= size.z)
				{
					break;
				}
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					IntVec3 size2 = map.Size;
					if (num4 >= size2.x)
					{
						break;
					}
					IntVec3 intVec = new IntVec3(num3, 0, num);
					Region region = intVec.GetRegion(map);
					if (region != null && NoRoofAroundAndWalkable(intVec, map))
					{
						tempUnroofedRegions.Add(region);
					}
					num3++;
				}
				num++;
			}
			Dijkstra<Region>.Run(tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt((float)a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), regionsDistanceToUnroofed);
			tempUnroofedRegions.Clear();
		}

		private static void CalculateClosedAreaSizeGrid(Map map)
		{
			if (closedAreaSize == null)
			{
				closedAreaSize = new ByteGrid(map);
			}
			else
			{
				closedAreaSize.ClearAndResizeTo(map);
			}
			int num = 0;
			while (true)
			{
				int num2 = num;
				IntVec3 size = map.Size;
				if (num2 >= size.z)
				{
					break;
				}
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					IntVec3 size2 = map.Size;
					if (num4 >= size2.x)
					{
						break;
					}
					IntVec3 intVec = new IntVec3(num3, 0, num);
					if (closedAreaSize[num3, num] == 0 && !intVec.Impassable(map))
					{
						int area = 0;
						map.floodFiller.FloodFill(intVec, (Predicate<IntVec3>)((IntVec3 c) => !c.Impassable(map)), (Action<IntVec3>)delegate
						{
							area++;
						}, 2147483647, rememberParents: false, (IEnumerable<IntVec3>)null);
						area = Mathf.Min(area, 255);
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							closedAreaSize[c] = (byte)area;
						});
					}
					num3++;
				}
				num++;
			}
		}

		private static void CalculateDistanceToColonyBuildingGrid(Map map)
		{
			if (distToColonyBuilding == null)
			{
				distToColonyBuilding = new ByteGrid(map);
			}
			else if (!distToColonyBuilding.MapSizeMatches(map))
			{
				distToColonyBuilding.ClearAndResizeTo(map);
			}
			distToColonyBuilding.Clear(byte.MaxValue);
			tmpColonyBuildingsLocs.Clear();
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				tmpColonyBuildingsLocs.Add(allBuildingsColonist[i].Position);
			}
			Dijkstra<IntVec3>.Run(tmpColonyBuildingsLocs, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), delegate(IntVec3 a, IntVec3 b)
			{
				if (a.x == b.x || a.z == b.z)
				{
					return 1f;
				}
				return 1.41421354f;
			}, tmpDistanceResult);
			for (int j = 0; j < tmpDistanceResult.Count; j++)
			{
				distToColonyBuilding[tmpDistanceResult[j].Key] = (byte)Mathf.Min(tmpDistanceResult[j].Value, 254.999f);
			}
		}
	}
}
