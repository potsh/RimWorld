using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DropPodUtility
	{
		private static List<List<Thing>> tempList = new List<List<Thing>>();

		public static void MakeDropPodAt(IntVec3 c, Map map, ActiveDropPodInfo info)
		{
			ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
			activeDropPod.Contents = info;
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.DropPodIncoming, activeDropPod, c, map);
		}

		public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			tempList.Clear();
			foreach (Thing thing in things)
			{
				List<Thing> list = new List<Thing>();
				list.Add(thing);
				tempList.Add(list);
			}
			DropThingGroupsNear(dropCenter, map, tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch);
			tempList.Clear();
		}

		public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			foreach (List<Thing> thingsGroup in thingsGroups)
			{
				if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out IntVec3 result, allowFogged: true, canRoofPunch))
				{
					Log.Warning("DropThingsNear failed to find a place to drop " + thingsGroup.FirstOrDefault() + " near " + dropCenter + ". Dropping on random square instead.");
					result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map), map);
				}
				for (int i = 0; i < thingsGroup.Count; i++)
				{
					thingsGroup[i].SetForbidden(value: true, warnOnFail: false);
				}
				if (instaDrop)
				{
					foreach (Thing item in thingsGroup)
					{
						GenPlace.TryPlaceThing(item, result, map, ThingPlaceMode.Near);
					}
				}
				else
				{
					ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
					foreach (Thing item2 in thingsGroup)
					{
						activeDropPodInfo.innerContainer.TryAdd(item2);
					}
					activeDropPodInfo.openDelay = openDelay;
					activeDropPodInfo.leaveSlag = leaveSlag;
					MakeDropPodAt(result, map, activeDropPodInfo);
				}
			}
		}
	}
}
