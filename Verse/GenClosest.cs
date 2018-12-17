using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class GenClosest
	{
		private const int DefaultLocalTraverseRegionsBeforeGlobal = 30;

		private static bool EarlyOutSearch(IntVec3 start, Map map, ThingRequest thingReq, IEnumerable<Thing> customGlobalSearchSet, Predicate<Thing> validator)
		{
			if (thingReq.group == ThingRequestGroup.Everything)
			{
				Log.Error("Cannot do ClosestThingReachable searching everything without restriction.");
				return true;
			}
			if (!start.InBounds(map))
			{
				Log.Error("Did FindClosestThing with start out of bounds (" + start + "), thingReq=" + thingReq);
				return true;
			}
			if (thingReq.group == ThingRequestGroup.Nothing)
			{
				return true;
			}
			if (customGlobalSearchSet == null && !thingReq.IsUndefined && map.listerThings.ThingsMatching(thingReq).Count == 0)
			{
				return true;
			}
			return false;
		}

		public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1, bool forceGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			bool flag = searchRegionsMax < 0 || forceGlobalSearch;
			if (!flag && customGlobalSearchSet != null)
			{
				Log.ErrorOnce("searchRegionsMax >= 0 && customGlobalSearchSet != null && !forceGlobalSearch. customGlobalSearchSet will never be used.", 634984);
			}
			if (!flag && !thingReq.IsUndefined && !thingReq.CanBeFoundInRegion)
			{
				Log.ErrorOnce("ClosestThingReachable with thing request group " + thingReq.group + " and global search not allowed. This will never find anything because this group is never stored in regions. Either allow global search or don't call this method at all.", 518498981);
				return null;
			}
			if (EarlyOutSearch(root, map, thingReq, customGlobalSearchSet, validator))
			{
				return null;
			}
			Thing thing = null;
			bool flag2 = false;
			if (!thingReq.IsUndefined && thingReq.CanBeFoundInRegion)
			{
				int num = (searchRegionsMax <= 0) ? 30 : searchRegionsMax;
				thing = RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, searchRegionsMin, num, maxDistance, out int regionsSeen, traversableRegionTypes, ignoreEntirelyForbiddenRegions);
				flag2 = (thing == null && regionsSeen < num);
			}
			if (thing == null && flag && !flag2)
			{
				if (traversableRegionTypes != RegionType.Set_Passable)
				{
					Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767);
				}
				Predicate<Thing> validator2 = delegate(Thing t)
				{
					if (!map.reachability.CanReach(root, t, peMode, traverseParams))
					{
						return false;
					}
					if (validator != null && !validator(t))
					{
						return false;
					}
					return true;
				};
				IEnumerable<Thing> searchSet = customGlobalSearchSet ?? map.listerThings.ThingsMatching(thingReq);
				thing = ClosestThing_Global(root, searchSet, maxDistance, validator2);
			}
			return thing;
		}

		public static Thing ClosestThing_Regionwise_ReachablePrioritized(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, int minRegions = 24, int maxRegions = 30)
		{
			if (!thingReq.IsUndefined && !thingReq.CanBeFoundInRegion)
			{
				Log.ErrorOnce("ClosestThing_Regionwise_ReachablePrioritized with thing request group " + thingReq.group + ". This will never find anything because this group is never stored in regions. Most likely a global search should have been used.", 738476712);
				return null;
			}
			if (EarlyOutSearch(root, map, thingReq, null, validator))
			{
				return null;
			}
			if (maxRegions < minRegions)
			{
				Log.ErrorOnce("maxRegions < minRegions", 754343);
			}
			Thing result = null;
			if (!thingReq.IsUndefined)
			{
				result = RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance, out int _);
			}
			return result;
		}

		public static Thing RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Thing> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance, out int regionsSeen, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			regionsSeen = 0;
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
			{
				Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.");
				return null;
			}
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater)
			{
				Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThingsNotWater. Use ClosestThingGlobal.");
				return null;
			}
			if (!req.IsUndefined && !req.CanBeFoundInRegion)
			{
				Log.ErrorOnce("RegionwiseBFSWorker with thing request group " + req.group + ". This group is never stored in regions. Most likely a global search should have been used.", 385766189);
				return null;
			}
			Region region = root.GetRegion(map, traversableRegionTypes);
			if (region == null)
			{
				return null;
			}
			float maxDistSquared = maxDistance * maxDistance;
			RegionEntryPredicate entryCondition = delegate(Region from, Region to)
			{
				if (!to.Allows(traverseParams, isDestination: false))
				{
					return false;
				}
				return maxDistance > 5000f || to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared;
			};
			Thing closestThing = null;
			float closestDistSquared = 9999999f;
			float bestPrio = -3.40282347E+38f;
			int regionsSeenScan = 0;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (RegionTraverser.ShouldCountRegion(r))
				{
					regionsSeenScan++;
				}
				if (!r.IsDoorway && !r.Allows(traverseParams, isDestination: true))
				{
					return false;
				}
				if (!ignoreEntirelyForbiddenRegions || !r.IsForbiddenEntirely(traverseParams.pawn))
				{
					List<Thing> list = r.ListerThings.ThingsMatching(req);
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = list[i];
						if (ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, peMode, traverseParams.pawn))
						{
							float num = (priorityGetter == null) ? 0f : priorityGetter(thing);
							if (!(num < bestPrio))
							{
								float num2 = (float)(thing.Position - root).LengthHorizontalSquared;
								if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared && (validator == null || validator(thing)))
								{
									closestThing = thing;
									closestDistSquared = num2;
									bestPrio = num;
								}
							}
						}
					}
				}
				return regionsSeenScan >= minRegions && closestThing != null;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
			regionsSeen = regionsSeenScan;
			return closestThing;
		}

		public static Thing ClosestThing_Global(IntVec3 center, IEnumerable searchSet, float maxDistance = 99999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null)
		{
			if (searchSet != null)
			{
				float num = 2.14748365E+09f;
				Thing result = null;
				float num2 = -3.40282347E+38f;
				float num3 = maxDistance * maxDistance;
				IEnumerator enumerator = searchSet.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Thing thing = (Thing)enumerator.Current;
						if (thing.Spawned)
						{
							float num4 = (float)(center - thing.Position).LengthHorizontalSquared;
							if (!(num4 > num3) && (priorityGetter != null || num4 < num) && (validator == null || validator(thing)))
							{
								float num5 = 0f;
								if (priorityGetter != null)
								{
									num5 = priorityGetter(thing);
									if (num5 < num2 || (num5 == num2 && num4 >= num))
									{
										continue;
									}
								}
								result = thing;
								num = num4;
								num2 = num5;
							}
						}
					}
					return result;
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			return null;
		}

		public static Thing ClosestThing_Global_Reachable(IntVec3 center, Map map, IEnumerable<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null)
		{
			if (searchSet == null)
			{
				return null;
			}
			int num = 0;
			int num2 = 0;
			Thing result = null;
			float num3 = -3.40282347E+38f;
			float num4 = maxDistance * maxDistance;
			float num5 = 2.14748365E+09f;
			foreach (Thing item in searchSet)
			{
				if (item.Spawned)
				{
					num2++;
					float num6 = (float)(center - item.Position).LengthHorizontalSquared;
					if (!(num6 > num4) && (priorityGetter != null || num6 < num5) && map.reachability.CanReach(center, item, peMode, traverseParams) && (validator == null || validator(item)))
					{
						float num7 = 0f;
						if (priorityGetter != null)
						{
							num7 = priorityGetter(item);
							if (num7 < num3 || (num7 == num3 && num6 >= num5))
							{
								continue;
							}
						}
						result = item;
						num5 = num6;
						num3 = num7;
						num++;
					}
				}
			}
			return result;
		}
	}
}
