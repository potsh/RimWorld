using System.Collections.Generic;

namespace Verse
{
	public static class GenAdj
	{
		public static IntVec3[] CardinalDirections;

		public static IntVec3[] CardinalDirectionsAndInside;

		public static IntVec3[] CardinalDirectionsAround;

		public static IntVec3[] DiagonalDirections;

		public static IntVec3[] DiagonalDirectionsAround;

		public static IntVec3[] AdjacentCells;

		public static IntVec3[] AdjacentCellsAndInside;

		public static IntVec3[] AdjacentCellsAround;

		public static IntVec3[] AdjacentCellsAroundBottom;

		private static List<IntVec3> adjRandomOrderList;

		private static List<IntVec3> validCells;

		static GenAdj()
		{
			CardinalDirections = new IntVec3[4];
			CardinalDirectionsAndInside = new IntVec3[5];
			CardinalDirectionsAround = new IntVec3[4];
			DiagonalDirections = new IntVec3[4];
			DiagonalDirectionsAround = new IntVec3[4];
			AdjacentCells = new IntVec3[8];
			AdjacentCellsAndInside = new IntVec3[9];
			AdjacentCellsAround = new IntVec3[8];
			AdjacentCellsAroundBottom = new IntVec3[9];
			validCells = new List<IntVec3>();
			SetupAdjacencyTables();
		}

		private static void SetupAdjacencyTables()
		{
			CardinalDirections[0] = new IntVec3(0, 0, 1);
			CardinalDirections[1] = new IntVec3(1, 0, 0);
			CardinalDirections[2] = new IntVec3(0, 0, -1);
			CardinalDirections[3] = new IntVec3(-1, 0, 0);
			CardinalDirectionsAndInside[0] = new IntVec3(0, 0, 1);
			CardinalDirectionsAndInside[1] = new IntVec3(1, 0, 0);
			CardinalDirectionsAndInside[2] = new IntVec3(0, 0, -1);
			CardinalDirectionsAndInside[3] = new IntVec3(-1, 0, 0);
			CardinalDirectionsAndInside[4] = new IntVec3(0, 0, 0);
			CardinalDirectionsAround[0] = new IntVec3(0, 0, -1);
			CardinalDirectionsAround[1] = new IntVec3(-1, 0, 0);
			CardinalDirectionsAround[2] = new IntVec3(0, 0, 1);
			CardinalDirectionsAround[3] = new IntVec3(1, 0, 0);
			DiagonalDirections[0] = new IntVec3(-1, 0, -1);
			DiagonalDirections[1] = new IntVec3(-1, 0, 1);
			DiagonalDirections[2] = new IntVec3(1, 0, 1);
			DiagonalDirections[3] = new IntVec3(1, 0, -1);
			DiagonalDirectionsAround[0] = new IntVec3(-1, 0, -1);
			DiagonalDirectionsAround[1] = new IntVec3(-1, 0, 1);
			DiagonalDirectionsAround[2] = new IntVec3(1, 0, 1);
			DiagonalDirectionsAround[3] = new IntVec3(1, 0, -1);
			AdjacentCells[0] = new IntVec3(0, 0, 1);
			AdjacentCells[1] = new IntVec3(1, 0, 0);
			AdjacentCells[2] = new IntVec3(0, 0, -1);
			AdjacentCells[3] = new IntVec3(-1, 0, 0);
			AdjacentCells[4] = new IntVec3(1, 0, -1);
			AdjacentCells[5] = new IntVec3(1, 0, 1);
			AdjacentCells[6] = new IntVec3(-1, 0, 1);
			AdjacentCells[7] = new IntVec3(-1, 0, -1);
			AdjacentCellsAndInside[0] = new IntVec3(0, 0, 1);
			AdjacentCellsAndInside[1] = new IntVec3(1, 0, 0);
			AdjacentCellsAndInside[2] = new IntVec3(0, 0, -1);
			AdjacentCellsAndInside[3] = new IntVec3(-1, 0, 0);
			AdjacentCellsAndInside[4] = new IntVec3(1, 0, -1);
			AdjacentCellsAndInside[5] = new IntVec3(1, 0, 1);
			AdjacentCellsAndInside[6] = new IntVec3(-1, 0, 1);
			AdjacentCellsAndInside[7] = new IntVec3(-1, 0, -1);
			AdjacentCellsAndInside[8] = new IntVec3(0, 0, 0);
			AdjacentCellsAround[0] = new IntVec3(0, 0, 1);
			AdjacentCellsAround[1] = new IntVec3(1, 0, 1);
			AdjacentCellsAround[2] = new IntVec3(1, 0, 0);
			AdjacentCellsAround[3] = new IntVec3(1, 0, -1);
			AdjacentCellsAround[4] = new IntVec3(0, 0, -1);
			AdjacentCellsAround[5] = new IntVec3(-1, 0, -1);
			AdjacentCellsAround[6] = new IntVec3(-1, 0, 0);
			AdjacentCellsAround[7] = new IntVec3(-1, 0, 1);
			AdjacentCellsAroundBottom[0] = new IntVec3(0, 0, -1);
			AdjacentCellsAroundBottom[1] = new IntVec3(-1, 0, -1);
			AdjacentCellsAroundBottom[2] = new IntVec3(-1, 0, 0);
			AdjacentCellsAroundBottom[3] = new IntVec3(-1, 0, 1);
			AdjacentCellsAroundBottom[4] = new IntVec3(0, 0, 1);
			AdjacentCellsAroundBottom[5] = new IntVec3(1, 0, 1);
			AdjacentCellsAroundBottom[6] = new IntVec3(1, 0, 0);
			AdjacentCellsAroundBottom[7] = new IntVec3(1, 0, -1);
			AdjacentCellsAroundBottom[8] = new IntVec3(0, 0, 0);
		}

		public static List<IntVec3> AdjacentCells8WayRandomized()
		{
			if (adjRandomOrderList == null)
			{
				adjRandomOrderList = new List<IntVec3>();
				for (int i = 0; i < 8; i++)
				{
					adjRandomOrderList.Add(AdjacentCells[i]);
				}
			}
			adjRandomOrderList.Shuffle();
			return adjRandomOrderList;
		}

		public static IEnumerable<IntVec3> CellsOccupiedBy(Thing t)
		{
			if (t.def.size.x == 1 && t.def.size.z == 1)
			{
				yield return t.Position;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			using (IEnumerator<IntVec3> enumerator = CellsOccupiedBy(t.Position, t.Rotation, t.def.size).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IntVec3 c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_013d:
			/*Error near IL_013e: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<IntVec3> CellsOccupiedBy(IntVec3 center, Rot4 rotation, IntVec2 size)
		{
			AdjustForRotation(ref center, ref size, rotation);
			int minX = center.x - (size.x - 1) / 2;
			int minZ = center.z - (size.z - 1) / 2;
			int maxX = minX + size.x - 1;
			int maxZ = minZ + size.z - 1;
			int j = minX;
			int i;
			while (true)
			{
				if (j > maxX)
				{
					yield break;
				}
				i = minZ;
				if (i <= maxZ)
				{
					break;
				}
				j++;
			}
			yield return new IntVec3(j, 0, i);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static IEnumerable<IntVec3> CellsAdjacent8Way(TargetInfo pack)
		{
			if (pack.HasThing)
			{
				using (IEnumerator<IntVec3> enumerator = CellsAdjacent8Way(pack.Thing).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						IntVec3 c = enumerator.Current;
						yield return c;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			else
			{
				int i = 0;
				if (i < 8)
				{
					yield return pack.Cell + AdjacentCells[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_013c:
			/*Error near IL_013d: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<IntVec3> CellsAdjacent8Way(Thing t)
		{
			return CellsAdjacent8Way(t.Position, t.Rotation, t.def.size);
		}

		public static IEnumerable<IntVec3> CellsAdjacent8Way(IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize)
		{
			AdjustForRotation(ref thingCenter, ref thingSize, thingRot);
			int minX = thingCenter.x - (thingSize.x - 1) / 2 - 1;
			int num = minX + thingSize.x + 1;
			int minZ = thingCenter.z - (thingSize.z - 1) / 2 - 1;
			int num2 = minZ + thingSize.z + 1;
			IntVec3 cur = new IntVec3(minX - 1, 0, minZ);
			cur.x++;
			yield return cur;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static IEnumerable<IntVec3> CellsAdjacentCardinal(Thing t)
		{
			return CellsAdjacentCardinal(t.Position, t.Rotation, t.def.size);
		}

		public static IEnumerable<IntVec3> CellsAdjacentCardinal(IntVec3 center, Rot4 rot, IntVec2 size)
		{
			AdjustForRotation(ref center, ref size, rot);
			int minX = center.x - (size.x - 1) / 2 - 1;
			int num = minX + size.x + 1;
			int minZ = center.z - (size.z - 1) / 2 - 1;
			int num2 = minZ + size.z + 1;
			IntVec3 cur = new IntVec3(minX, 0, minZ);
			cur.x++;
			yield return cur;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static IEnumerable<IntVec3> CellsAdjacentAlongEdge(IntVec3 thingCent, Rot4 thingRot, IntVec2 thingSize, LinkDirections dir)
		{
			AdjustForRotation(ref thingCent, ref thingSize, thingRot);
			int minX = thingCent.x - (thingSize.x - 1) / 2 - 1;
			int minZ = thingCent.z - (thingSize.z - 1) / 2 - 1;
			int maxX = minX + thingSize.x + 1;
			int maxZ = minZ + thingSize.z + 1;
			if (dir == LinkDirections.Down)
			{
				int x2 = minX;
				if (x2 <= maxX)
				{
					yield return new IntVec3(x2, thingCent.y, minZ - 1);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (dir == LinkDirections.Up)
			{
				int x = minX;
				if (x <= maxX)
				{
					yield return new IntVec3(x, thingCent.y, maxZ + 1);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (dir == LinkDirections.Left)
			{
				int z2 = minZ;
				if (z2 <= maxZ)
				{
					yield return new IntVec3(minX - 1, thingCent.y, z2);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (dir == LinkDirections.Right)
			{
				int z = minZ;
				if (z <= maxZ)
				{
					yield return new IntVec3(maxX + 1, thingCent.y, z);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<IntVec3> CellsAdjacent8WayAndInside(this Thing thing)
		{
			IntVec3 center = thing.Position;
			IntVec2 size = thing.def.size;
			Rot4 rotation = thing.Rotation;
			AdjustForRotation(ref center, ref size, rotation);
			int minX = center.x - (size.x - 1) / 2 - 1;
			int minZ = center.z - (size.z - 1) / 2 - 1;
			int maxX = minX + size.x + 1;
			int maxZ = minZ + size.z + 1;
			int j = minX;
			int i;
			while (true)
			{
				if (j > maxX)
				{
					yield break;
				}
				i = minZ;
				if (i <= maxZ)
				{
					break;
				}
				j++;
			}
			yield return new IntVec3(j, 0, i);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static void GetAdjacentCorners(LocalTargetInfo target, out IntVec3 BL, out IntVec3 TL, out IntVec3 TR, out IntVec3 BR)
		{
			if (target.HasThing)
			{
				GetAdjacentCorners(target.Thing.OccupiedRect(), out BL, out TL, out TR, out BR);
			}
			else
			{
				GetAdjacentCorners(CellRect.SingleCell(target.Cell), out BL, out TL, out TR, out BR);
			}
		}

		private static void GetAdjacentCorners(CellRect rect, out IntVec3 BL, out IntVec3 TL, out IntVec3 TR, out IntVec3 BR)
		{
			BL = new IntVec3(rect.minX - 1, 0, rect.minZ - 1);
			TL = new IntVec3(rect.minX - 1, 0, rect.maxZ + 1);
			TR = new IntVec3(rect.maxX + 1, 0, rect.maxZ + 1);
			BR = new IntVec3(rect.maxX + 1, 0, rect.minZ - 1);
		}

		public static IntVec3 RandomAdjacentCell8Way(this IntVec3 root)
		{
			return root + AdjacentCells[Rand.RangeInclusive(0, 7)];
		}

		public static IntVec3 RandomAdjacentCellCardinal(this IntVec3 root)
		{
			return root + CardinalDirections[Rand.RangeInclusive(0, 3)];
		}

		public static IntVec3 RandomAdjacentCell8Way(this Thing t)
		{
			CellRect cellRect = t.OccupiedRect();
			CellRect cellRect2 = cellRect.ExpandedBy(1);
			IntVec3 randomCell;
			do
			{
				randomCell = cellRect2.RandomCell;
			}
			while (cellRect.Contains(randomCell));
			return randomCell;
		}

		public static IntVec3 RandomAdjacentCellCardinal(this Thing t)
		{
			CellRect cellRect = t.OccupiedRect();
			IntVec3 randomCell = cellRect.RandomCell;
			if (Rand.Value < 0.5f)
			{
				if (Rand.Value < 0.5f)
				{
					randomCell.x = cellRect.minX - 1;
				}
				else
				{
					randomCell.x = cellRect.maxX + 1;
				}
			}
			else if (Rand.Value < 0.5f)
			{
				randomCell.z = cellRect.minZ - 1;
			}
			else
			{
				randomCell.z = cellRect.maxZ + 1;
			}
			return randomCell;
		}

		public static bool TryFindRandomAdjacentCell8WayWithRoomGroup(Thing t, out IntVec3 result)
		{
			return TryFindRandomAdjacentCell8WayWithRoomGroup(t.Position, t.Rotation, t.def.size, t.Map, out result);
		}

		public static bool TryFindRandomAdjacentCell8WayWithRoomGroup(IntVec3 center, Rot4 rot, IntVec2 size, Map map, out IntVec3 result)
		{
			AdjustForRotation(ref center, ref size, rot);
			validCells.Clear();
			foreach (IntVec3 item in CellsAdjacent8Way(center, rot, size))
			{
				if (item.InBounds(map) && item.GetRoomGroup(map) != null)
				{
					validCells.Add(item);
				}
			}
			return validCells.TryRandomElement(out result);
		}

		public static bool AdjacentTo8WayOrInside(this IntVec3 me, LocalTargetInfo other)
		{
			if (other.HasThing)
			{
				return me.AdjacentTo8WayOrInside(other.Thing);
			}
			return me.AdjacentTo8WayOrInside(other.Cell);
		}

		public static bool AdjacentTo8Way(this IntVec3 me, IntVec3 other)
		{
			int num = me.x - other.x;
			int num2 = me.z - other.z;
			if (num == 0 && num2 == 0)
			{
				return false;
			}
			if (num < 0)
			{
				num *= -1;
			}
			if (num2 < 0)
			{
				num2 *= -1;
			}
			return num <= 1 && num2 <= 1;
		}

		public static bool AdjacentTo8WayOrInside(this IntVec3 me, IntVec3 other)
		{
			int num = me.x - other.x;
			int num2 = me.z - other.z;
			if (num < 0)
			{
				num *= -1;
			}
			if (num2 < 0)
			{
				num2 *= -1;
			}
			return num <= 1 && num2 <= 1;
		}

		public static bool IsAdjacentToCardinalOrInside(this IntVec3 me, CellRect other)
		{
			if (other.IsEmpty)
			{
				return false;
			}
			CellRect cellRect = other.ExpandedBy(1);
			return cellRect.Contains(me) && !cellRect.IsCorner(me);
		}

		public static bool IsAdjacentToCardinalOrInside(this Thing t1, Thing t2)
		{
			return IsAdjacentToCardinalOrInside(t1.OccupiedRect(), t2.OccupiedRect());
		}

		public static bool IsAdjacentToCardinalOrInside(CellRect rect1, CellRect rect2)
		{
			if (rect1.IsEmpty || rect2.IsEmpty)
			{
				return false;
			}
			CellRect cellRect = rect1.ExpandedBy(1);
			int minX = cellRect.minX;
			int maxX = cellRect.maxX;
			int minZ = cellRect.minZ;
			int maxZ = cellRect.maxZ;
			int i = minX;
			int num = minZ;
			for (; i <= maxX; i++)
			{
				if (rect2.Contains(new IntVec3(i, 0, num)) && (i != minX || num != minZ) && (i != minX || num != maxZ) && (i != maxX || num != minZ) && (i != maxX || num != maxZ))
				{
					return true;
				}
			}
			i--;
			for (num++; num <= maxZ; num++)
			{
				if (rect2.Contains(new IntVec3(i, 0, num)) && (i != minX || num != minZ) && (i != minX || num != maxZ) && (i != maxX || num != minZ) && (i != maxX || num != maxZ))
				{
					return true;
				}
			}
			num--;
			for (i--; i >= minX; i--)
			{
				if (rect2.Contains(new IntVec3(i, 0, num)) && (i != minX || num != minZ) && (i != minX || num != maxZ) && (i != maxX || num != minZ) && (i != maxX || num != maxZ))
				{
					return true;
				}
			}
			i++;
			for (num--; num > minZ; num--)
			{
				if (rect2.Contains(new IntVec3(i, 0, num)) && (i != minX || num != minZ) && (i != minX || num != maxZ) && (i != maxX || num != minZ) && (i != maxX || num != maxZ))
				{
					return true;
				}
			}
			return false;
		}

		public static bool AdjacentTo8WayOrInside(this IntVec3 root, Thing t)
		{
			return root.AdjacentTo8WayOrInside(t.Position, t.Rotation, t.def.size);
		}

		public static bool AdjacentTo8WayOrInside(this IntVec3 root, IntVec3 center, Rot4 rot, IntVec2 size)
		{
			AdjustForRotation(ref center, ref size, rot);
			int num = center.x - (size.x - 1) / 2 - 1;
			int num2 = center.z - (size.z - 1) / 2 - 1;
			int num3 = num + size.x + 1;
			int num4 = num2 + size.z + 1;
			if (root.x >= num && root.x <= num3 && root.z >= num2 && root.z <= num4)
			{
				return true;
			}
			return false;
		}

		public static bool AdjacentTo8WayOrInside(this Thing a, Thing b)
		{
			return AdjacentTo8WayOrInside(a.OccupiedRect(), b.OccupiedRect());
		}

		public static bool AdjacentTo8WayOrInside(CellRect rect1, CellRect rect2)
		{
			if (rect1.IsEmpty || rect2.IsEmpty)
			{
				return false;
			}
			return rect1.ExpandedBy(1).Overlaps(rect2);
		}

		public static bool IsInside(this IntVec3 root, Thing t)
		{
			return IsInside(root, t.Position, t.Rotation, t.def.size);
		}

		public static bool IsInside(IntVec3 root, IntVec3 center, Rot4 rot, IntVec2 size)
		{
			AdjustForRotation(ref center, ref size, rot);
			int num = center.x - (size.x - 1) / 2;
			int num2 = center.z - (size.z - 1) / 2;
			int num3 = num + size.x - 1;
			int num4 = num2 + size.z - 1;
			if (root.x >= num && root.x <= num3 && root.z >= num2 && root.z <= num4)
			{
				return true;
			}
			return false;
		}

		public static CellRect OccupiedRect(this Thing t)
		{
			return OccupiedRect(t.Position, t.Rotation, t.def.size);
		}

		public static CellRect OccupiedRect(IntVec3 center, Rot4 rot, IntVec2 size)
		{
			AdjustForRotation(ref center, ref size, rot);
			return new CellRect(center.x - (size.x - 1) / 2, center.z - (size.z - 1) / 2, size.x, size.z);
		}

		public static void AdjustForRotation(ref IntVec3 center, ref IntVec2 size, Rot4 rot)
		{
			if (size.x != 1 || size.z != 1)
			{
				if (rot.IsHorizontal)
				{
					int x = size.x;
					size.x = size.z;
					size.z = x;
				}
				switch (rot.AsInt)
				{
				case 0:
					break;
				case 1:
					if (size.z % 2 == 0)
					{
						center.z--;
					}
					break;
				case 2:
					if (size.x % 2 == 0)
					{
						center.x--;
					}
					if (size.z % 2 == 0)
					{
						center.z--;
					}
					break;
				case 3:
					if (size.x % 2 == 0)
					{
						center.x--;
					}
					break;
				}
			}
		}
	}
}
