using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLeaving
	{
		private const float LeaveFraction_Kill = 0.5f;

		private const float LeaveFraction_Cancel = 1f;

		public const float LeaveFraction_DeconstructDefault = 0.75f;

		private const float LeaveFraction_FailConstruction = 0.5f;

		private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode)
		{
			DoLeavingsFor(diedThing, map, mode, diedThing.OccupiedRect());
		}

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, CellRect leavingsRect, Predicate<IntVec3> nearPlaceValidator = null)
		{
			if (Current.ProgramState == ProgramState.Playing || mode == DestroyMode.Refund)
			{
				switch (mode)
				{
				case DestroyMode.Vanish:
					return;
				case DestroyMode.KillFinalize:
					if (diedThing.def.filthLeaving != null)
					{
						for (int i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
						{
							for (int j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
							{
								IntVec3 c = new IntVec3(j, 0, i);
								FilthMaker.MakeFilth(c, map, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
							}
						}
					}
					break;
				}
				ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
				if (mode == DestroyMode.KillFinalize && diedThing.def.killedLeavings != null)
				{
					for (int k = 0; k < diedThing.def.killedLeavings.Count; k++)
					{
						Thing thing = ThingMaker.MakeThing(diedThing.def.killedLeavings[k].thingDef);
						thing.stackCount = diedThing.def.killedLeavings[k].count;
						thingOwner.TryAdd(thing);
					}
				}
				if (CanBuildingLeaveResources(diedThing, mode))
				{
					Frame frame = diedThing as Frame;
					if (frame != null)
					{
						for (int num = frame.resourceContainer.Count - 1; num >= 0; num--)
						{
							int num2 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(frame.resourceContainer[num].stackCount);
							if (num2 > 0)
							{
								frame.resourceContainer.TryTransferToContainer(frame.resourceContainer[num], thingOwner, num2);
							}
						}
						frame.resourceContainer.ClearAndDestroyContents();
					}
					else
					{
						List<ThingDefCountClass> list = diedThing.CostListAdjusted();
						for (int l = 0; l < list.Count; l++)
						{
							ThingDefCountClass thingDefCountClass = list[l];
							int num3 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(thingDefCountClass.count);
							if (num3 > 0 && mode == DestroyMode.KillFinalize && thingDefCountClass.thingDef.slagDef != null)
							{
								int count = thingDefCountClass.thingDef.slagDef.smeltProducts.First((ThingDefCountClass pro) => pro.thingDef == ThingDefOf.Steel).count;
								int num4 = num3 / 2 / 8;
								for (int m = 0; m < num4; m++)
								{
									thingOwner.TryAdd(ThingMaker.MakeThing(thingDefCountClass.thingDef.slagDef));
								}
								num3 -= num4 * count;
							}
							if (num3 > 0)
							{
								Thing thing2 = ThingMaker.MakeThing(thingDefCountClass.thingDef);
								thing2.stackCount = num3;
								thingOwner.TryAdd(thing2);
							}
						}
					}
				}
				List<IntVec3> list2 = leavingsRect.Cells.InRandomOrder().ToList();
				int num5 = 0;
				while (true)
				{
					if (thingOwner.Count <= 0)
					{
						return;
					}
					if (mode == DestroyMode.KillFinalize && !map.areaManager.Home[list2[num5]])
					{
						thingOwner[0].SetForbidden(value: true, warnOnFail: false);
					}
					ThingOwner<Thing> thingOwner2 = thingOwner;
					Thing thing3 = thingOwner[0];
					IntVec3 dropLoc = list2[num5];
					ThingPlaceMode mode2 = ThingPlaceMode.Near;
					Thing lastResultingThing = default(Thing);
					if (!thingOwner2.TryDrop(thing3, dropLoc, map, mode2, out lastResultingThing, null, nearPlaceValidator))
					{
						break;
					}
					num5++;
					if (num5 >= list2.Count)
					{
						num5 = 0;
					}
				}
				Log.Warning("Failed to place all leavings for destroyed thing " + diedThing + " at " + leavingsRect.CenterCell);
			}
		}

		public static void DoLeavingsFor(TerrainDef terrain, IntVec3 cell, Map map)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
				List<ThingDefCountClass> list = terrain.CostListAdjusted(null);
				for (int i = 0; i < list.Count; i++)
				{
					ThingDefCountClass thingDefCountClass = list[i];
					int num = GenMath.RoundRandom((float)thingDefCountClass.count * terrain.resourcesFractionWhenDeconstructed);
					if (num > 0)
					{
						Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
						thing.stackCount = num;
						thingOwner.TryAdd(thing);
					}
				}
				Thing lastResultingThing;
				do
				{
					if (thingOwner.Count <= 0)
					{
						return;
					}
				}
				while (thingOwner.TryDrop(thingOwner[0], cell, map, ThingPlaceMode.Near, out lastResultingThing));
				Log.Warning("Failed to place all leavings for removed terrain " + terrain + " at " + cell);
			}
		}

		public static bool CanBuildingLeaveResources(Thing destroyedThing, DestroyMode mode)
		{
			if (destroyedThing is Building)
			{
				if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
				{
					mode = DestroyMode.Cancel;
				}
				switch (mode)
				{
				case DestroyMode.Vanish:
					return false;
				case DestroyMode.WillReplace:
					return false;
				case DestroyMode.KillFinalize:
					return destroyedThing.def.leaveResourcesWhenKilled ? true : false;
				case DestroyMode.Deconstruct:
					return destroyedThing.def.resourcesFractionWhenDeconstructed != 0f;
				case DestroyMode.Cancel:
					return true;
				case DestroyMode.FailConstruction:
					return true;
				case DestroyMode.Refund:
					return true;
				default:
					throw new ArgumentException("Unknown destroy mode " + mode);
				}
			}
			return false;
		}

		private static Func<int, int> GetBuildingResourcesLeaveCalculator(Thing destroyedThing, DestroyMode mode)
		{
			if (CanBuildingLeaveResources(destroyedThing, mode))
			{
				if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
				{
					mode = DestroyMode.Cancel;
				}
				switch (mode)
				{
				case DestroyMode.Vanish:
					return (int count) => 0;
				case DestroyMode.WillReplace:
					return (int count) => 0;
				case DestroyMode.KillFinalize:
					return (int count) => GenMath.RoundRandom((float)count * 0.5f);
				case DestroyMode.Deconstruct:
					return (int count) => GenMath.RoundRandom(Mathf.Min((float)count * destroyedThing.def.resourcesFractionWhenDeconstructed, (float)(count - 1)));
				case DestroyMode.Cancel:
					return (int count) => GenMath.RoundRandom((float)count * 1f);
				case DestroyMode.FailConstruction:
					return (int count) => GenMath.RoundRandom((float)count * 0.5f);
				case DestroyMode.Refund:
					return (int count) => count;
				default:
					throw new ArgumentException("Unknown destroy mode " + mode);
				}
			}
			return (int count) => 0;
		}

		public static void DropFilthDueToDamage(Thing t, float damageDealt)
		{
			if (t.def.useHitPoints && t.Spawned && t.def.filthLeaving != null)
			{
				CellRect cellRect = t.OccupiedRect().ExpandedBy(1);
				tmpCellsCandidates.Clear();
				foreach (IntVec3 item in cellRect)
				{
					if (item.InBounds(t.Map) && item.Walkable(t.Map))
					{
						tmpCellsCandidates.Add(item);
					}
				}
				if (tmpCellsCandidates.Any())
				{
					int num = GenMath.RoundRandom(damageDealt * Mathf.Min(0.0166666675f, 1f / ((float)t.MaxHitPoints / 10f)));
					for (int i = 0; i < num; i++)
					{
						FilthMaker.MakeFilth(tmpCellsCandidates.RandomElement(), t.Map, t.def.filthLeaving);
					}
					tmpCellsCandidates.Clear();
				}
			}
		}
	}
}
