using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SiegeBlueprintPlacer
	{
		private static IntVec3 center;

		private static Faction faction;

		private static List<IntVec3> placedSandbagLocs = new List<IntVec3>();

		private const int MaxArtyCount = 2;

		public const float ArtyCost = 60f;

		private const int MinSandbagDistSquared = 36;

		private static readonly IntRange NumSandbagRange = new IntRange(2, 4);

		private static readonly IntRange SandbagLengthRange = new IntRange(2, 7);

		public static IEnumerable<Blueprint_Build> PlaceBlueprints(IntVec3 placeCenter, Map map, Faction placeFaction, float points)
		{
			center = placeCenter;
			faction = placeFaction;
			using (IEnumerator<Blueprint_Build> enumerator = PlaceSandbagBlueprints(map).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Blueprint_Build blue2 = enumerator.Current;
					yield return blue2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Blueprint_Build> enumerator2 = PlaceArtilleryBlueprints(points, map).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Blueprint_Build blue = enumerator2.Current;
					yield return blue;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0166:
			/*Error near IL_0167: Unexpected return in MoveNext()*/;
		}

		private static bool CanPlaceBlueprintAt(IntVec3 root, Rot4 rot, ThingDef buildingDef, Map map)
		{
			return GenConstruct.CanPlaceBlueprintAt(buildingDef, root, rot, map).Accepted;
		}

		private static IEnumerable<Blueprint_Build> PlaceSandbagBlueprints(Map map)
		{
			placedSandbagLocs.Clear();
			int numSandbags = NumSandbagRange.RandomInRange;
			for (int i = 0; i < numSandbags; i++)
			{
				IntVec3 bagRoot2 = FindSandbagRoot(map);
				if (!bagRoot2.IsValid)
				{
					break;
				}
				Rot4 growDirA = (bagRoot2.x <= center.x) ? Rot4.East : Rot4.West;
				Rot4 growDirB = (bagRoot2.z <= center.z) ? Rot4.North : Rot4.South;
				using (IEnumerator<Blueprint_Build> enumerator = MakeSandbagLine(bagRoot2, map, growDirA, SandbagLengthRange.RandomInRange).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Blueprint_Build bag2 = enumerator.Current;
						yield return bag2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				bagRoot2 += growDirB.FacingCell;
				using (IEnumerator<Blueprint_Build> enumerator2 = MakeSandbagLine(bagRoot2, map, growDirB, SandbagLengthRange.RandomInRange).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Blueprint_Build bag = enumerator2.Current;
						yield return bag;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0271:
			/*Error near IL_0272: Unexpected return in MoveNext()*/;
		}

		private static IEnumerable<Blueprint_Build> MakeSandbagLine(IntVec3 root, Map map, Rot4 growDir, int maxLength)
		{
			int i = 0;
			if (i < maxLength && CanPlaceBlueprintAt(root, Rot4.North, ThingDefOf.Sandbags, map))
			{
				yield return GenConstruct.PlaceBlueprintForBuild(ThingDefOf.Sandbags, root, map, Rot4.North, faction, null);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		private static IEnumerable<Blueprint_Build> PlaceArtilleryBlueprints(float points, Map map)
		{
			IEnumerable<ThingDef> artyDefs = from def in DefDatabase<ThingDef>.AllDefs
			where def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer")
			select def;
			int numArtillery2 = Mathf.RoundToInt(points / 60f);
			numArtillery2 = Mathf.Clamp(numArtillery2, 1, 2);
			int i = 0;
			if (i < numArtillery2)
			{
				Rot4 rot = Rot4.Random;
				ThingDef artyDef = artyDefs.RandomElement();
				IntVec3 artySpot = FindArtySpot(artyDef, rot, map);
				if (artySpot.IsValid)
				{
					yield return GenConstruct.PlaceBlueprintForBuild(artyDef, artySpot, map, rot, faction, ThingDefOf.Steel);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		private static IntVec3 FindSandbagRoot(Map map)
		{
			CellRect cellRect = CellRect.CenteredOn(center, 13);
			cellRect.ClipInsideMap(map);
			CellRect cellRect2 = CellRect.CenteredOn(center, 8);
			cellRect2.ClipInsideMap(map);
			int num = 0;
			goto IL_002d;
			IL_002d:
			IntVec3 randomCell;
			while (true)
			{
				num++;
				if (num > 200)
				{
					return IntVec3.Invalid;
				}
				randomCell = cellRect.RandomCell;
				if (!cellRect2.Contains(randomCell) && map.reachability.CanReach(randomCell, center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) && CanPlaceBlueprintAt(randomCell, Rot4.North, ThingDefOf.Sandbags, map))
				{
					bool flag = false;
					for (int i = 0; i < placedSandbagLocs.Count; i++)
					{
						float num2 = (float)(placedSandbagLocs[i] - randomCell).LengthHorizontalSquared;
						if (num2 < 36f)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						break;
					}
				}
			}
			return randomCell;
			IL_00f7:
			goto IL_002d;
		}

		private static IntVec3 FindArtySpot(ThingDef artyDef, Rot4 rot, Map map)
		{
			CellRect cellRect = CellRect.CenteredOn(center, 8);
			cellRect.ClipInsideMap(map);
			int num = 0;
			goto IL_0017;
			IL_0017:
			IntVec3 randomCell;
			do
			{
				num++;
				if (num > 200)
				{
					return IntVec3.Invalid;
				}
				randomCell = cellRect.RandomCell;
			}
			while (!map.reachability.CanReach(randomCell, center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) || randomCell.Roofed(map) || !CanPlaceBlueprintAt(randomCell, rot, artyDef, map));
			return randomCell;
			IL_007d:
			goto IL_0017;
		}
	}
}
