using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DropCellFinder
	{
		public static IntVec3 RandomDropSpot(Map map)
		{
			return CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.Roofed(map) && !c.Fogged(map), map);
		}

		public static IntVec3 TradeDropSpot(Map map)
		{
			IEnumerable<Building> collection = from b in map.listerBuildings.allBuildingsColonist
			where b.def.IsCommsConsole
			select b;
			IEnumerable<Building> enumerable = from b in map.listerBuildings.allBuildingsColonist
			where b.def.IsOrbitalTradeBeacon
			select b;
			Building building = enumerable.FirstOrDefault((Building b) => !map.roofGrid.Roofed(b.Position) && AnyAdjacentGoodDropSpot(b.Position, map, allowFogged: false, canRoofPunch: false));
			IntVec3 result;
			if (building == null)
			{
				List<Building> list = new List<Building>();
				list.AddRange(enumerable);
				list.AddRange(collection);
				list.RemoveAll(delegate(Building b)
				{
					CompPowerTrader compPowerTrader = b.TryGetComp<CompPowerTrader>();
					return compPowerTrader != null && !compPowerTrader.PowerOn;
				});
				Predicate<IntVec3> validator = (IntVec3 c) => IsGoodDropSpot(c, map, allowFogged: false, canRoofPunch: false);
				if (!list.Any())
				{
					list.AddRange(map.listerBuildings.allBuildingsColonist);
					list.Shuffle();
					if (!list.Any())
					{
						return CellFinderLoose.RandomCellWith(validator, map);
					}
				}
				int num = 8;
				int num2;
				IntVec3 size;
				do
				{
					for (int i = 0; i < list.Count; i++)
					{
						IntVec3 position = list[i].Position;
						if (CellFinder.TryFindRandomCellNear(position, map, num, validator, out result))
						{
							return result;
						}
					}
					num = Mathf.RoundToInt((float)num * 1.1f);
					num2 = num;
					size = map.Size;
				}
				while (num2 <= size.x);
				Log.Error("Failed to generate trade drop center. Giving random.");
				return CellFinderLoose.RandomCellWith(validator, map);
			}
			result = building.Position;
			if (!TryFindDropSpotNear(result, map, out IntVec3 result2, allowFogged: false, canRoofPunch: false))
			{
				Log.Error("Could find no good TradeDropSpot near dropCenter " + result + ". Using a random standable unfogged cell.");
				return CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.Fogged(map), map);
			}
			return result2;
		}

		public static bool TryFindDropSpotNear(IntVec3 center, Map map, out IntVec3 result, bool allowFogged, bool canRoofPunch)
		{
			if (DebugViewSettings.drawDestSearch)
			{
				map.debugDrawer.FlashCell(center, 1f, "center");
			}
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!IsGoodDropSpot(c, map, allowFogged, canRoofPunch))
				{
					return false;
				}
				if (!map.reachability.CanReach(center, c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
				{
					return false;
				}
				return true;
			};
			int num = 5;
			do
			{
				if (CellFinder.TryFindRandomCellNear(center, map, num, validator, out result))
				{
					return true;
				}
				num += 3;
			}
			while (num <= 16);
			result = center;
			return false;
		}

		public static bool IsGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
		{
			if (!c.InBounds(map) || !c.Standable(map))
			{
				return false;
			}
			if (!CanPhysicallyDropInto(c, map, canRoofPunch))
			{
				if (DebugViewSettings.drawDestSearch)
				{
					map.debugDrawer.FlashCell(c, 0f, "phys");
				}
				return false;
			}
			if (Current.ProgramState == ProgramState.Playing && !allowFogged && c.Fogged(map))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is IActiveDropPod || thing is Skyfaller)
				{
					return false;
				}
				if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(ThingDefOf.ActiveDropPod, thing.def))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AnyAdjacentGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
		{
			return IsGoodDropSpot(c + IntVec3.North, map, allowFogged, canRoofPunch) || IsGoodDropSpot(c + IntVec3.East, map, allowFogged, canRoofPunch) || IsGoodDropSpot(c + IntVec3.South, map, allowFogged, canRoofPunch) || IsGoodDropSpot(c + IntVec3.West, map, allowFogged, canRoofPunch);
		}

		public static IntVec3 FindRaidDropCenterDistant(Map map)
		{
			Faction hostFaction = map.ParentFaction ?? Faction.OfPlayer;
			IEnumerable<Thing> first = map.mapPawns.FreeHumanlikesSpawnedOfFaction(hostFaction).Cast<Thing>();
			first = ((hostFaction != Faction.OfPlayer) ? first.Concat(from x in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
			where x.Faction == hostFaction
			select x) : first.Concat(map.listerBuildings.allBuildingsColonist.Cast<Thing>()));
			int num = 0;
			float num2 = 65f;
			goto IL_008e;
			IL_016a:
			goto IL_008e;
			IL_008e:
			IntVec3 intVec;
			while (true)
			{
				intVec = CellFinder.RandomCell(map);
				num++;
				if (CanPhysicallyDropInto(intVec, map, canRoofPunch: true) && !intVec.Fogged(map))
				{
					if (num > 300)
					{
						return intVec;
					}
					if (!intVec.Roofed(map))
					{
						num2 -= 0.2f;
						bool flag = false;
						foreach (Thing item in first)
						{
							if ((float)(intVec - item.Position).LengthHorizontalSquared < num2 * num2)
							{
								flag = true;
								break;
							}
						}
						if (!flag && map.reachability.CanReachFactionBase(intVec, hostFaction))
						{
							break;
						}
					}
				}
			}
			return intVec;
		}

		public static bool TryFindRaidDropCenterClose(out IntVec3 spot, Map map)
		{
			Faction parentFaction = map.ParentFaction;
			if (parentFaction == null)
			{
				return RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CanPhysicallyDropInto(x, map, canRoofPunch: true) && !x.Fogged(map) && x.Standable(map), map, out spot);
			}
			int num = 0;
			do
			{
				IntVec3 result = IntVec3.Invalid;
				if (map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).Count() > 0)
				{
					result = map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).RandomElement().Position;
				}
				else
				{
					if (parentFaction == Faction.OfPlayer)
					{
						List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
						for (int i = 0; i < allBuildingsColonist.Count && !TryFindDropSpotNear(allBuildingsColonist[i].Position, map, out result, allowFogged: true, canRoofPunch: true); i++)
						{
						}
					}
					else
					{
						List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
						for (int j = 0; j < list.Count && (list[j].Faction != parentFaction || !TryFindDropSpotNear(list[j].Position, map, out result, allowFogged: true, canRoofPunch: true)); j++)
						{
						}
					}
					if (!result.IsValid)
					{
						RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CanPhysicallyDropInto(x, map, canRoofPunch: true) && !x.Fogged(map) && x.Standable(map), map, out result);
					}
				}
				spot = CellFinder.RandomClosewalkCellNear(result, map, 10);
				if (CanPhysicallyDropInto(spot, map, canRoofPunch: true) && !spot.Fogged(map))
				{
					return true;
				}
				num++;
			}
			while (num <= 300);
			spot = CellFinderLoose.RandomCellWith((IntVec3 c) => CanPhysicallyDropInto(c, map, canRoofPunch: true), map);
			return false;
		}

		private static bool CanPhysicallyDropInto(IntVec3 c, Map map, bool canRoofPunch)
		{
			if (!c.Walkable(map))
			{
				return false;
			}
			RoofDef roof = c.GetRoof(map);
			if (roof != null)
			{
				if (!canRoofPunch)
				{
					return false;
				}
				if (roof.isThickRoof)
				{
					return false;
				}
			}
			return true;
		}
	}
}
