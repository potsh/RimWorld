using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class RCellFinder
	{
		private static List<Region> regions = new List<Region>();

		private static HashSet<Thing> tmpBuildings = new HashSet<Thing>();

		public static IntVec3 BestOrderedGotoDestNear(IntVec3 root, Pawn searcher)
		{
			Map map = searcher.Map;
			Predicate<IntVec3> predicate = delegate(IntVec3 c)
			{
				if (!map.pawnDestinationReservationManager.CanReserve(c, searcher, draftedOnly: true) || !c.Standable(map) || !searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
				{
					return false;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && pawn != searcher && pawn.RaceProps.Humanlike)
					{
						return false;
					}
				}
				return true;
			};
			if (predicate(root))
			{
				return root;
			}
			int num = 1;
			IntVec3 result = default(IntVec3);
			float num2 = -1000f;
			bool flag = false;
			int num3 = GenRadial.NumCellsInRadius(30f);
			while (true)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[num];
				if (predicate(intVec))
				{
					float num4 = CoverUtility.TotalSurroundingCoverScore(intVec, map);
					if (num4 > num2)
					{
						num2 = num4;
						result = intVec;
						flag = true;
					}
				}
				if (num >= 8 && flag)
				{
					break;
				}
				num++;
				if (num >= num3)
				{
					return searcher.Position;
				}
			}
			return result;
		}

		public static bool TryFindBestExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAllDestroyableThings && !pawn.Map.reachability.CanReachMapEdge(pawn.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBash: true)))
			{
				return TryFindRandomPawnEntryCell(out spot, pawn.Map, 0f, allowFogged: true, delegate(IntVec3 x)
				{
					Pawn pawn2 = pawn;
					LocalTargetInfo dest = x;
					PathEndMode peMode = PathEndMode.OnCell;
					Danger maxDanger = Danger.Deadly;
					TraverseMode mode2 = mode;
					return pawn2.CanReach(dest, peMode, maxDanger, canBash: false, mode2);
				});
			}
			int num = 0;
			int num2 = 0;
			IntVec3 intVec;
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					spot = pawn.Position;
					return false;
				}
				IntVec3 result;
				bool flag = CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, num, null, out result);
				num += 4;
				if (flag)
				{
					int num3 = result.x;
					intVec = new IntVec3(0, 0, result.z);
					IntVec3 size = pawn.Map.Size;
					if (size.z - result.z < num3)
					{
						IntVec3 size2 = pawn.Map.Size;
						num3 = size2.z - result.z;
						int x2 = result.x;
						IntVec3 size3 = pawn.Map.Size;
						intVec = new IntVec3(x2, 0, size3.z - 1);
					}
					IntVec3 size4 = pawn.Map.Size;
					if (size4.x - result.x < num3)
					{
						IntVec3 size5 = pawn.Map.Size;
						num3 = size5.x - result.x;
						IntVec3 size6 = pawn.Map.Size;
						intVec = new IntVec3(size6.x - 1, 0, result.z);
					}
					if (result.z < num3)
					{
						intVec = new IntVec3(result.x, 0, 0);
					}
					if (intVec.Standable(pawn.Map) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, canBash: true, mode))
					{
						break;
					}
				}
			}
			spot = intVec;
			return true;
		}

		public static bool TryFindRandomExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			Danger danger = Danger.Some;
			int num = 0;
			goto IL_0004;
			IL_0004:
			IntVec3 intVec;
			while (true)
			{
				num++;
				if (num > 40)
				{
					spot = pawn.Position;
					return false;
				}
				if (num > 15)
				{
					danger = Danger.Deadly;
				}
				intVec = CellFinder.RandomCell(pawn.Map);
				int num2 = Rand.RangeInclusive(0, 3);
				if (num2 == 0)
				{
					intVec.x = 0;
				}
				if (num2 == 1)
				{
					IntVec3 size = pawn.Map.Size;
					intVec.x = size.x - 1;
				}
				if (num2 == 2)
				{
					intVec.z = 0;
				}
				if (num2 == 3)
				{
					IntVec3 size2 = pawn.Map.Size;
					intVec.z = size2.z - 1;
				}
				if (intVec.Standable(pawn.Map))
				{
					LocalTargetInfo dest = intVec;
					PathEndMode peMode = PathEndMode.OnCell;
					Danger maxDanger = danger;
					TraverseMode mode2 = mode;
					if (pawn.CanReach(dest, peMode, maxDanger, canBash: false, mode2))
					{
						break;
					}
				}
			}
			spot = intVec;
			return true;
			IL_00ee:
			goto IL_0004;
		}

		public static bool TryFindExitSpotNear(Pawn pawn, IntVec3 near, float radius, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAllDestroyableThings && CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly), out spot))
			{
				return true;
			}
			return CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, delegate(IntVec3 x)
			{
				Pawn pawn2 = pawn;
				LocalTargetInfo dest = x;
				PathEndMode peMode = PathEndMode.OnCell;
				Danger maxDanger = Danger.Deadly;
				TraverseMode mode2 = mode;
				return pawn2.CanReach(dest, peMode, maxDanger, canBash: false, mode2);
			}, out spot);
		}

		public static IntVec3 RandomWanderDestFor(Pawn pawn, IntVec3 root, float radius, Func<Pawn, IntVec3, IntVec3, bool> validator, Danger maxDanger)
		{
			if (radius > 12f)
			{
				Log.Warning("wanderRadius of " + radius + " is greater than Region.GridSize of " + 12 + " and will break.");
			}
			bool flag = UnityData.isDebugBuild && DebugViewSettings.drawDestSearch;
			if (root.GetRegion(pawn.Map) != null)
			{
				int maxRegions = Mathf.Max((int)radius / 3, 13);
				CellFinder.AllRegionsNear(regions, root.GetRegion(pawn.Map), maxRegions, TraverseParms.For(pawn), (Region reg) => reg.extentsClose.ClosestDistSquaredTo(root) <= radius * radius);
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(root, 0.6f, "root");
				}
				if (regions.Count > 0)
				{
					for (int i = 0; i < 35; i++)
					{
						IntVec3 intVec = IntVec3.Invalid;
						for (int j = 0; j < 5; j++)
						{
							IntVec3 randomCell = regions.RandomElementByWeight((Region reg) => (float)reg.CellCount).RandomCell;
							if ((float)randomCell.DistanceToSquared(root) <= radius * radius)
							{
								intVec = randomCell;
								break;
							}
						}
						if (!intVec.IsValid)
						{
							if (flag)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.32f, "distance");
							}
						}
						else
						{
							if (CanWanderToCell(intVec, pawn, root, validator, i, maxDanger))
							{
								if (flag)
								{
									pawn.Map.debugDrawer.FlashCell(intVec, 0.9f, "go!");
								}
								regions.Clear();
								return intVec;
							}
							if (flag)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.6f, "validation");
							}
						}
					}
				}
				regions.Clear();
			}
			if (!CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn) && (validator == null || validator(pawn, c, root)), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 20, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 30, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 5, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out IntVec3 result))
			{
				result = pawn.Position;
			}
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(result, 0.4f, "fallback");
			}
			return result;
		}

		private static bool CanWanderToCell(IntVec3 c, Pawn pawn, IntVec3 root, Func<Pawn, IntVec3, IntVec3, bool> validator, int tryIndex, Danger maxDanger)
		{
			bool flag = UnityData.isDebugBuild && DebugViewSettings.drawDestSearch;
			if (!c.Walkable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0f, "walk");
				}
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "forbid");
				}
				return false;
			}
			if (tryIndex < 10 && !c.Standable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "stand");
				}
				return false;
			}
			if (!pawn.CanReach(c, PathEndMode.OnCell, maxDanger))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.6f, "reach");
				}
				return false;
			}
			if (PawnUtility.KnownDangerAt(c, pawn.Map, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.1f, "trap");
				}
				return false;
			}
			if (tryIndex < 10)
			{
				if (c.GetTerrain(pawn.Map).avoidWander)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.39f, "terr");
					}
					return false;
				}
				if (pawn.Map.pathGrid.PerceivedPathCostAt(c) > 20)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.4f, "pcost");
					}
					return false;
				}
				if ((int)c.GetDangerFor(pawn, pawn.Map) > 1)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.4f, "danger");
					}
					return false;
				}
			}
			else if (tryIndex < 15 && c.GetDangerFor(pawn, pawn.Map) == Danger.Deadly)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.4f, "deadly");
				}
				return false;
			}
			if (!pawn.Map.pawnDestinationReservationManager.CanReserve(c, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.75f, "resvd");
				}
				return false;
			}
			if (validator != null && !validator(pawn, c, root))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.15f, "valid");
				}
				return false;
			}
			if (c.GetDoor(pawn.Map) != null)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.32f, "door");
				}
				return false;
			}
			if (c.ContainsStaticFire(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.9f, "fire");
				}
				return false;
			}
			return true;
		}

		public static bool TryFindGoodAdjacentSpotToTouch(Pawn toucher, Thing touchee, out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder())
			{
				if (item.Standable(toucher.Map) && !PawnUtility.KnownDangerAt(item, toucher.Map, toucher))
				{
					result = item;
					return true;
				}
			}
			foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder())
			{
				if (item2.Walkable(toucher.Map))
				{
					result = item2;
					return true;
				}
			}
			result = touchee.Position;
			return false;
		}

		public static bool TryFindRandomPawnEntryCell(out IntVec3 result, Map map, float roadChance, bool allowFogged = false, Predicate<IntVec3> extraValidator = null)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && !map.roofGrid.Roofed(c) && map.reachability.CanReachColony(c) && c.GetRoom(map).TouchesMapEdge && (allowFogged || !c.Fogged(map)) && (extraValidator == null || extraValidator(c)), map, roadChance, out result);
		}

		public static bool TryFindPrisonerReleaseCell(Pawn prisoner, Pawn warden, out IntVec3 result)
		{
			if (prisoner.Map != warden.Map)
			{
				result = IntVec3.Invalid;
				return false;
			}
			Region region = prisoner.GetRegion();
			if (region == null)
			{
				result = default(IntVec3);
				return false;
			}
			TraverseParms traverseParms = TraverseParms.For(warden);
			bool needMapEdge = prisoner.Faction != warden.Faction;
			IntVec3 foundResult = IntVec3.Invalid;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (needMapEdge)
				{
					if (!r.Room.TouchesMapEdge)
					{
						return false;
					}
				}
				else if (r.Room.isPrisonCell)
				{
					return false;
				}
				foundResult = r.RandomCell;
				return true;
			};
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), regionProcessor, 999);
			if (foundResult.IsValid)
			{
				result = foundResult;
				return true;
			}
			result = default(IntVec3);
			return false;
		}

		public static IntVec3 RandomAnimalSpawnCell_MapGen(Map map)
		{
			int numStand = 0;
			int numRoom = 0;
			int numTouch = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					numStand++;
					return false;
				}
				if (c.GetTerrain(map).avoidWander)
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (room == null)
				{
					numRoom++;
					return false;
				}
				if (!room.TouchesMapEdge)
				{
					numTouch++;
					return false;
				}
				return true;
			};
			if (!CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out IntVec3 result))
			{
				result = CellFinder.RandomCell(map);
				Log.Warning("RandomAnimalSpawnCell_MapGen failed: numStand=" + numStand + ", numRoom=" + numRoom + ", numTouch=" + numTouch + ". PlayerStartSpot=" + MapGenerator.PlayerStartSpot + ". Returning " + result);
			}
			return result;
		}

		public static bool TryFindSkygazeCell(IntVec3 root, Pawn searcher, out IntVec3 result)
		{
			Predicate<IntVec3> cellValidator = (IntVec3 c) => !c.Roofed(searcher.Map) && !c.GetTerrain(searcher.Map).avoidWander;
			IntVec3 unused;
			Predicate<Region> validator = (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(searcher) && r.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out unused);
			TraverseParms traverseParms = TraverseParms.For(searcher);
			if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(searcher.Map), traverseParms, validator, 300, out Region result2))
			{
				result = root;
				return false;
			}
			Region reg = CellFinder.RandomRegionNear(result2, 14, traverseParms, validator, searcher);
			return reg.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out result);
		}

		public static bool TryFindTravelDestFrom(IntVec3 root, Map map, out IntVec3 travelDest)
		{
			travelDest = root;
			bool flag = false;
			Predicate<IntVec3> cellValidator = (IntVec3 c) => map.reachability.CanReach(root, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.None) && !map.roofGrid.Roofed(c);
			if (root.x == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 c)
				{
					int x2 = c.x;
					IntVec3 size4 = map.Size;
					return x2 == size4.x - 1 && cellValidator(c);
				}, map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			else
			{
				int x = root.x;
				IntVec3 size = map.Size;
				if (x == size.x - 1)
				{
					flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == 0 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
				}
				else if (root.z == 0)
				{
					flag = CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 c)
					{
						int z2 = c.z;
						IntVec3 size3 = map.Size;
						return z2 == size3.z - 1 && cellValidator(c);
					}, map, CellFinder.EdgeRoadChance_Always, out travelDest);
				}
				else
				{
					int z = root.z;
					IntVec3 size2 = map.Size;
					if (z == size2.z - 1)
					{
						flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == 0 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
					}
				}
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 10000 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 2500 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			return flag;
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 originCell, Map map, out IntVec3 result)
		{
			return TryFindRandomSpotJustOutsideColony(originCell, map, null, out result);
		}

		public static bool TryFindRandomSpotJustOutsideColony(Pawn searcher, out IntVec3 result)
		{
			return TryFindRandomSpotJustOutsideColony(searcher.Position, searcher.Map, searcher, out result);
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 root, Map map, Pawn searcher, out IntVec3 result, Predicate<IntVec3> extraValidator = null)
		{
			bool desperate = false;
			int minColonyBuildingsLOS = 0;
			int walkRadius = 0;
			int walkRadiusMaxImpassable = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (!room.PsychologicallyOutdoors || !room.TouchesMapEdge)
				{
					return false;
				}
				if (room == null || room.CellCount < 60)
				{
					return false;
				}
				if (root.IsValid)
				{
					TraverseParms traverseParams = (searcher == null) ? ((TraverseParms)TraverseMode.PassDoors) : TraverseParms.For(searcher);
					if (!map.reachability.CanReach(root, c, PathEndMode.Touch, traverseParams))
					{
						return false;
					}
				}
				if (!desperate && !map.reachability.CanReachColony(c))
				{
					return false;
				}
				if (extraValidator != null && !extraValidator(c))
				{
					return false;
				}
				int num = 0;
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(c, walkRadius).GetIterator();
				while (!iterator.Done())
				{
					Room room2 = iterator.Current.GetRoom(map);
					if (room2 != room)
					{
						num++;
						if (!desperate && room2 != null && room2.IsDoorway)
						{
							return false;
						}
					}
					if (num > walkRadiusMaxImpassable)
					{
						return false;
					}
					iterator.MoveNext();
				}
				if (minColonyBuildingsLOS > 0)
				{
					int colonyBuildingsLOSFound = 0;
					tmpBuildings.Clear();
					RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => true, delegate(Region reg)
					{
						Faction ofPlayer = Faction.OfPlayer;
						List<Thing> list = reg.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
						for (int l = 0; l < list.Count; l++)
						{
							Thing thing = list[l];
							if (thing.Faction == ofPlayer && thing.Position.InHorDistOf(c, 16f) && GenSight.LineOfSight(thing.Position, c, map, skipFirstCell: true) && !tmpBuildings.Contains(thing))
							{
								tmpBuildings.Add(thing);
								colonyBuildingsLOSFound++;
								if (colonyBuildingsLOSFound >= minColonyBuildingsLOS)
								{
									return true;
								}
							}
						}
						return false;
					}, 12);
					tmpBuildings.Clear();
					if (colonyBuildingsLOSFound < minColonyBuildingsLOS)
					{
						return false;
					}
				}
				return true;
			};
			IEnumerable<Building> source = from b in map.listerBuildings.allBuildingsColonist
			where b.def.building.ai_chillDestination
			select b;
			for (int i = 0; i < 120; i++)
			{
				Building result2 = null;
				if (!source.TryRandomElement(out result2))
				{
					break;
				}
				desperate = (i > 60);
				walkRadius = 6 - i / 20;
				walkRadiusMaxImpassable = 6 - i / 20;
				minColonyBuildingsLOS = 5 - i / 30;
				if (CellFinder.TryFindRandomCellNear(result2.Position, map, 10, validator, out result, 50))
				{
					return true;
				}
			}
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int j = 0; j < 120; j++)
			{
				Building result3 = null;
				if (!allBuildingsColonist.TryRandomElement(out result3))
				{
					break;
				}
				desperate = (j > 60);
				walkRadius = 6 - j / 20;
				walkRadiusMaxImpassable = 6 - j / 20;
				minColonyBuildingsLOS = 4 - j / 30;
				if (CellFinder.TryFindRandomCellNear(result3.Position, map, 15, validator, out result, 50))
				{
					return true;
				}
			}
			for (int k = 0; k < 50; k++)
			{
				Pawn result4 = null;
				if (!map.mapPawns.FreeColonistsAndPrisonersSpawned.TryRandomElement(out result4))
				{
					break;
				}
				desperate = (k > 25);
				walkRadius = 3;
				walkRadiusMaxImpassable = 6;
				minColonyBuildingsLOS = 0;
				if (CellFinder.TryFindRandomCellNear(result4.Position, map, 15, validator, out result, 50))
				{
					return true;
				}
			}
			desperate = true;
			walkRadius = 3;
			walkRadiusMaxImpassable = 6;
			minColonyBuildingsLOS = 0;
			if (CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out result))
			{
				return true;
			}
			return false;
		}

		public static bool TryFindRandomCellInRegionUnforbidden(this Region reg, Pawn pawn, Predicate<IntVec3> validator, out IntVec3 result)
		{
			if (reg == null)
			{
				throw new ArgumentNullException("reg");
			}
			if (reg.IsForbiddenEntirely(pawn))
			{
				result = IntVec3.Invalid;
				return false;
			}
			return reg.TryFindRandomCellInRegion((IntVec3 c) => !c.IsForbidden(pawn) && (validator == null || validator(c)), out result);
		}

		public static bool TryFindDirectFleeDestination(IntVec3 root, float dist, Pawn pawn, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				result = root + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * dist);
				if (result.Walkable(pawn.Map) && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, pawn.Map, skipFirstCell: true))
				{
					return true;
				}
			}
			Region region = pawn.GetRegion();
			for (int j = 0; j < 30; j++)
			{
				Region region2 = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn));
				IntVec3 randomCell = region2.RandomCell;
				if (randomCell.Walkable(pawn.Map) && (float)(root - randomCell).LengthHorizontalSquared > dist * dist)
				{
					using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, randomCell, pawn))
					{
						if (PawnPathUtility.TryFindCellAtIndex(path, (int)dist + 3, out result))
						{
							return true;
						}
					}
				}
			}
			result = pawn.Position;
			return false;
		}

		public static bool TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(IntVec3 pos, Map map, float minDistToColony, out IntVec3 result)
		{
			int num = 30;
			CellRect cellRect = CellRect.CenteredOn(map.Center, num);
			cellRect.ClipInsideMap(map);
			List<IntVec3> list = new List<IntVec3>();
			if (minDistToColony > 0f)
			{
				foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
				{
					list.Add(item.Position);
				}
				foreach (Building item2 in map.listerBuildings.allBuildingsColonist)
				{
					list.Add(item2.Position);
				}
			}
			float num2 = minDistToColony * minDistToColony;
			int num3 = 0;
			goto IL_00c9;
			IL_00c9:
			while (true)
			{
				num3++;
				if (num3 > 50)
				{
					int num4 = num;
					IntVec3 size = map.Size;
					if (num4 > size.x)
					{
						break;
					}
					num = (int)((float)num * 1.5f);
					cellRect = CellRect.CenteredOn(map.Center, num);
					cellRect.ClipInsideMap(map);
					num3 = 0;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (randomCell.Standable(map) && map.reachability.CanReach(randomCell, pos, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Deadly))
				{
					bool flag = false;
					for (int i = 0; i < list.Count; i++)
					{
						if ((float)(list[i] - randomCell).LengthHorizontalSquared < num2)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						result = randomCell;
						return true;
					}
				}
			}
			result = pos;
			return false;
			IL_01ac:
			goto IL_00c9;
		}

		public static bool TryFindRandomCellNearTheCenterOfTheMapWith(Predicate<IntVec3> validator, Map map, out IntVec3 result)
		{
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			int startingSearchRadius = Mathf.Clamp(Mathf.Max(x, size2.z) / 20, 3, 25);
			return TryFindRandomCellNearWith(map.Center, validator, map, out result, startingSearchRadius);
		}

		public static bool TryFindRandomCellNearWith(IntVec3 near, Predicate<IntVec3> validator, Map map, out IntVec3 result, int startingSearchRadius = 5, int maxSearchRadius = int.MaxValue)
		{
			int num = startingSearchRadius;
			CellRect cellRect = CellRect.CenteredOn(near, num);
			cellRect.ClipInsideMap(map);
			int num2 = 0;
			goto IL_0016;
			IL_0016:
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					if (num >= maxSearchRadius)
					{
						break;
					}
					int num3 = num;
					IntVec3 size = map.Size;
					if (num3 > size.x * 2)
					{
						int num4 = num;
						IntVec3 size2 = map.Size;
						if (num4 > size2.z * 2)
						{
							break;
						}
					}
					num = Mathf.Min((int)((float)num * 1.5f), maxSearchRadius);
					cellRect = CellRect.CenteredOn(near, num);
					cellRect.ClipInsideMap(map);
					num2 = 0;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (validator(randomCell))
				{
					result = randomCell;
					return true;
				}
			}
			result = near;
			return false;
			IL_00a5:
			goto IL_0016;
		}

		public static IntVec3 SpotToChewStandingNear(Pawn pawn, Thing ingestible)
		{
			IntVec3 root = pawn.Position;
			Room rootRoom = pawn.GetRoom();
			bool desperate = false;
			bool ignoreDanger = false;
			float maxDist = 4f;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if ((float)(root - c).LengthHorizontalSquared > maxDist * maxDist)
				{
					return false;
				}
				if (pawn.HostFaction != null && c.GetRoom(pawn.Map) != rootRoom)
				{
					return false;
				}
				if (!desperate)
				{
					if (!c.Standable(pawn.Map))
					{
						return false;
					}
					if (GenPlace.HaulPlaceBlockerIn(null, c, pawn.Map, checkBlueprintsAndFrames: false) != null)
					{
						return false;
					}
					if (c.GetRegion(pawn.Map).type == RegionType.Portal)
					{
						return false;
					}
				}
				if (!ignoreDanger && c.GetDangerFor(pawn, pawn.Map) != Danger.None)
				{
					return false;
				}
				if (c.ContainsStaticFire(pawn.Map) || c.ContainsTrap(pawn.Map))
				{
					return false;
				}
				if (!pawn.Map.pawnDestinationReservationManager.CanReserve(c, pawn))
				{
					return false;
				}
				if (!Toils_Ingest.TryFindAdjacentIngestionPlaceSpot(c, ingestible.def, pawn, out IntVec3 _))
				{
					return false;
				}
				return true;
			};
			int maxRegions = 1;
			Region region = pawn.GetRegion();
			for (int i = 0; i < 30; i++)
			{
				switch (i)
				{
				case 1:
					desperate = true;
					break;
				case 2:
					desperate = false;
					maxRegions = 4;
					break;
				case 6:
					desperate = true;
					break;
				case 10:
					desperate = false;
					maxDist = 8f;
					maxRegions = 12;
					break;
				case 15:
					desperate = true;
					break;
				case 20:
					maxDist = 15f;
					maxRegions = 16;
					break;
				case 26:
					maxDist = 5f;
					maxRegions = 4;
					ignoreDanger = true;
					break;
				case 29:
					maxDist = 15f;
					maxRegions = 16;
					break;
				}
				Region reg = CellFinder.RandomRegionNear(region, maxRegions, TraverseParms.For(pawn));
				if (reg.TryFindRandomCellInRegionUnforbidden(pawn, validator, out IntVec3 result))
				{
					if (DebugViewSettings.drawDestSearch)
					{
						pawn.Map.debugDrawer.FlashCell(result, 0.5f, "go!");
					}
					return result;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					pawn.Map.debugDrawer.FlashCell(result, 0f, i.ToString());
				}
			}
			return region.RandomCell;
		}

		public static bool TryFindMarriageSite(Pawn firstFiance, Pawn secondFiance, out IntVec3 result)
		{
			if (!firstFiance.CanReach(secondFiance, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				result = IntVec3.Invalid;
				return false;
			}
			Map map = firstFiance.Map;
			if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MarriageSpot)
			where MarriageSpotUtility.IsValidMarriageSpotFor(x.Position, firstFiance, secondFiance)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noMarriageSpotValidator = delegate(IntVec3 cell)
			{
				IntVec3 c = cell + LordToil_MarriageCeremony.OtherFianceNoMarriageSpotCellOffset;
				if (!c.InBounds(map))
				{
					return false;
				}
				if (c.IsForbidden(firstFiance) || c.IsForbidden(secondFiance))
				{
					return false;
				}
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = cell.GetRoom(map);
				if (room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount < 10)
				{
					return false;
				}
				return true;
			};
			foreach (CompGatherSpot item in map.gatherSpotLister.activeSpots.InRandomOrder())
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(item.parent.Position, item.parent.Map, 4);
					if (MarriageSpotUtility.IsValidMarriageSpotFor(intVec, firstFiance, secondFiance) && noMarriageSpotValidator(intVec))
					{
						result = intVec;
						return true;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(firstFiance.Position, firstFiance.Map, 25, (IntVec3 cell) => MarriageSpotUtility.IsValidMarriageSpotFor(cell, firstFiance, secondFiance) && noMarriageSpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindPartySpot(Pawn organizer, out IntVec3 result)
		{
			bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
			Map map = organizer.Map;
			Predicate<IntVec3> baseValidator = delegate(IntVec3 cell)
			{
				if (!cell.Standable(map))
				{
					return false;
				}
				if (cell.GetDangerFor(organizer, map) != Danger.None)
				{
					return false;
				}
				if (!enjoyableOutside && !cell.Roofed(map))
				{
					return false;
				}
				if (cell.IsForbidden(organizer))
				{
					return false;
				}
				if (!organizer.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
				{
					return false;
				}
				bool flag = cell.GetRoom(map)?.isPrisonCell ?? false;
				if (organizer.IsPrisoner != flag)
				{
					return false;
				}
				if (!PartyUtility.EnoughPotentialGuestsToStartParty(map, cell))
				{
					return false;
				}
				return true;
			};
			if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.PartySpot)
			where baseValidator(x.Position)
			select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noPartySpotValidator = delegate(IntVec3 cell)
			{
				Room room = cell.GetRoom(map);
				if (room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount < 10)
				{
					return false;
				}
				return true;
			};
			foreach (CompGatherSpot item in map.gatherSpotLister.activeSpots.InRandomOrder())
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(item.parent.Position, item.parent.Map, 4);
					if (baseValidator(intVec) && noPartySpotValidator(intVec))
					{
						result = intVec;
						return true;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(organizer.Position, organizer.Map, 25, (IntVec3 cell) => baseValidator(cell) && noPartySpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot, Map map)
		{
			if (!entrySpot.IsValid)
			{
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 result))
				{
					result = CellFinder.RandomCell(map);
				}
				Log.Error("Tried to find a siege position from an invalid cell. Using " + result);
				return result;
			}
			IntVec3 result2;
			for (int num = 70; num >= 20; num -= 10)
			{
				if (TryFindSiegePosition(entrySpot, (float)num, map, out result2))
				{
					return result2;
				}
			}
			if (TryFindSiegePosition(entrySpot, 100f, map, out result2))
			{
				return result2;
			}
			Log.Error("Could not find siege spot from " + entrySpot + ", using " + entrySpot);
			return entrySpot;
		}

		private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, out IntVec3 result)
		{
			CellRect cellRect = CellRect.CenteredOn(entrySpot, 60);
			cellRect.ClipInsideMap(map);
			cellRect = cellRect.ContractedBy(14);
			List<IntVec3> list = new List<IntVec3>();
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				list.Add(item.Position);
			}
			foreach (Building allBuildingsColonistCombatTarget in map.listerBuildings.allBuildingsColonistCombatTargets)
			{
				list.Add(allBuildingsColonistCombatTarget.Position);
			}
			float num = minDistToColony * minDistToColony;
			int num2 = 0;
			goto IL_00bc;
			IL_00bc:
			while (true)
			{
				num2++;
				if (num2 > 200)
				{
					break;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (randomCell.Standable(map) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light) && map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some) && map.reachability.CanReachColony(randomCell))
				{
					bool flag = false;
					for (int i = 0; i < list.Count; i++)
					{
						if ((float)(list[i] - randomCell).LengthHorizontalSquared < num)
						{
							flag = true;
							break;
						}
					}
					if (!flag && !randomCell.Roofed(map))
					{
						int num3 = 0;
						CellRect.CellRectIterator iterator = CellRect.CenteredOn(randomCell, 10).ClipInsideMap(map).GetIterator();
						while (!iterator.Done())
						{
							if (randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
							{
								num3++;
							}
							iterator.MoveNext();
						}
						if (num3 >= 35)
						{
							result = randomCell;
							return true;
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
			IL_022d:
			goto IL_00bc;
		}
	}
}
