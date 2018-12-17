using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public struct CellRect : IEquatable<CellRect>
	{
		public struct Enumerator : IEnumerator<IntVec3>, IEnumerator, IDisposable
		{
			private CellRect ir;

			private int x;

			private int z;

			object IEnumerator.Current
			{
				get
				{
					return new IntVec3(x, 0, z);
				}
			}

			public IntVec3 Current => new IntVec3(x, 0, z);

			public Enumerator(CellRect ir)
			{
				this.ir = ir;
				x = ir.minX - 1;
				z = ir.minZ;
			}

			public bool MoveNext()
			{
				x++;
				if (x > ir.maxX)
				{
					x = ir.minX;
					z++;
				}
				if (z > ir.maxZ)
				{
					return false;
				}
				return true;
			}

			public void Reset()
			{
				x = ir.minX - 1;
				z = ir.minZ;
			}

			void IDisposable.Dispose()
			{
			}
		}

		public struct CellRectIterator
		{
			private int maxX;

			private int minX;

			private int maxZ;

			private int x;

			private int z;

			public IntVec3 Current => new IntVec3(x, 0, z);

			public CellRectIterator(CellRect cr)
			{
				minX = cr.minX;
				maxX = cr.maxX;
				maxZ = cr.maxZ;
				x = cr.minX;
				z = cr.minZ;
			}

			public void MoveNext()
			{
				x++;
				if (x > maxX)
				{
					x = minX;
					z++;
				}
			}

			public bool Done()
			{
				return z > maxZ;
			}
		}

		public int minX;

		public int maxX;

		public int minZ;

		public int maxZ;

		public static CellRect Empty => new CellRect(0, 0, 0, 0);

		public bool IsEmpty => Width <= 0 || Height <= 0;

		public int Area => Width * Height;

		public int Width
		{
			get
			{
				if (minX > maxX)
				{
					return 0;
				}
				return maxX - minX + 1;
			}
			set
			{
				maxX = minX + Mathf.Max(value, 0) - 1;
			}
		}

		public int Height
		{
			get
			{
				if (minZ > maxZ)
				{
					return 0;
				}
				return maxZ - minZ + 1;
			}
			set
			{
				maxZ = minZ + Mathf.Max(value, 0) - 1;
			}
		}

		public IEnumerable<IntVec3> Corners
		{
			get
			{
				if (!IsEmpty)
				{
					yield return new IntVec3(minX, 0, minZ);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public IntVec3 BottomLeft => new IntVec3(minX, 0, minZ);

		public IntVec3 TopRight => new IntVec3(maxX, 0, maxZ);

		public IntVec3 RandomCell => new IntVec3(Rand.RangeInclusive(minX, maxX), 0, Rand.RangeInclusive(minZ, maxZ));

		public IntVec3 CenterCell => new IntVec3(minX + Width / 2, 0, minZ + Height / 2);

		public Vector3 CenterVector3 => new Vector3((float)minX + (float)Width / 2f, 0f, (float)minZ + (float)Height / 2f);

		public Vector3 RandomVector3 => new Vector3(Rand.Range((float)minX, (float)maxX + 1f), 0f, Rand.Range((float)minZ, (float)maxZ + 1f));

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				int z = minZ;
				int x;
				while (true)
				{
					if (z > maxZ)
					{
						yield break;
					}
					x = minX;
					if (x <= maxX)
					{
						break;
					}
					z++;
				}
				yield return new IntVec3(x, 0, z);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public IEnumerable<IntVec2> Cells2D
		{
			get
			{
				int z = minZ;
				int x;
				while (true)
				{
					if (z > maxZ)
					{
						yield break;
					}
					x = minX;
					if (x <= maxX)
					{
						break;
					}
					z++;
				}
				yield return new IntVec2(x, z);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public IEnumerable<IntVec3> EdgeCells
		{
			get
			{
				if (!IsEmpty)
				{
					int x4 = minX;
					int z4 = minZ;
					if (x4 <= maxX)
					{
						yield return new IntVec3(x4, 0, z4);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					x4--;
					z4++;
					if (z4 <= maxZ)
					{
						yield return new IntVec3(x4, 0, z4);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					z4--;
					x4--;
					if (x4 >= minX)
					{
						yield return new IntVec3(x4, 0, z4);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					x4++;
					z4--;
					if (z4 > minZ)
					{
						yield return new IntVec3(x4, 0, z4);
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
		}

		public int EdgeCellsCount
		{
			get
			{
				if (Area == 0)
				{
					return 0;
				}
				if (Area == 1)
				{
					return 1;
				}
				return Width * 2 + (Height - 2) * 2;
			}
		}

		public IEnumerable<IntVec3> AdjacentCellsCardinal
		{
			get
			{
				if (!IsEmpty)
				{
					int x = minX;
					if (x <= maxX)
					{
						yield return new IntVec3(x, 0, minZ - 1);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					int z = minZ;
					if (z <= maxZ)
					{
						yield return new IntVec3(minX - 1, 0, z);
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
		}

		public CellRect(int minX, int minZ, int width, int height)
		{
			this.minX = minX;
			this.minZ = minZ;
			maxX = minX + width - 1;
			maxZ = minZ + height - 1;
		}

		public CellRectIterator GetIterator()
		{
			return new CellRectIterator(this);
		}

		public static bool operator ==(CellRect lhs, CellRect rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(CellRect lhs, CellRect rhs)
		{
			return !(lhs == rhs);
		}

		public static CellRect WholeMap(Map map)
		{
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			return new CellRect(0, 0, x, size2.z);
		}

		public static CellRect FromLimits(int minX, int minZ, int maxX, int maxZ)
		{
			CellRect result = default(CellRect);
			result.minX = Mathf.Min(minX, maxX);
			result.minZ = Mathf.Min(minZ, maxZ);
			result.maxX = Mathf.Max(maxX, minX);
			result.maxZ = Mathf.Max(maxZ, minZ);
			return result;
		}

		public static CellRect FromLimits(IntVec3 first, IntVec3 second)
		{
			CellRect result = default(CellRect);
			result.minX = Mathf.Min(first.x, second.x);
			result.minZ = Mathf.Min(first.z, second.z);
			result.maxX = Mathf.Max(first.x, second.x);
			result.maxZ = Mathf.Max(first.z, second.z);
			return result;
		}

		public static CellRect CenteredOn(IntVec3 center, int radius)
		{
			CellRect result = default(CellRect);
			result.minX = center.x - radius;
			result.maxX = center.x + radius;
			result.minZ = center.z - radius;
			result.maxZ = center.z + radius;
			return result;
		}

		public static CellRect CenteredOn(IntVec3 center, int width, int height)
		{
			CellRect result = default(CellRect);
			result.minX = center.x - width / 2;
			result.minZ = center.z - height / 2;
			result.maxX = result.minX + width - 1;
			result.maxZ = result.minZ + height - 1;
			return result;
		}

		public static CellRect ViewRect(Map map)
		{
			if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != map || WorldRendererUtility.WorldRenderedNow)
			{
				return Empty;
			}
			return Find.CameraDriver.CurrentViewRect;
		}

		public static CellRect SingleCell(IntVec3 c)
		{
			return new CellRect(c.x, c.z, 1, 1);
		}

		public bool InBounds(Map map)
		{
			int result;
			if (minX >= 0 && minZ >= 0)
			{
				int num = maxX;
				IntVec3 size = map.Size;
				if (num < size.x)
				{
					int num2 = maxZ;
					IntVec3 size2 = map.Size;
					result = ((num2 < size2.z) ? 1 : 0);
					goto IL_004a;
				}
			}
			result = 0;
			goto IL_004a;
			IL_004a:
			return (byte)result != 0;
		}

		public bool FullyContainedWithin(CellRect within)
		{
			CellRect rhs = this;
			rhs.ClipInsideRect(within);
			return this == rhs;
		}

		public bool Overlaps(CellRect other)
		{
			if (IsEmpty || other.IsEmpty)
			{
				return false;
			}
			return minX <= other.maxX && maxX >= other.minX && maxZ >= other.minZ && minZ <= other.maxZ;
		}

		public bool IsOnEdge(IntVec3 c)
		{
			return (c.x == minX && c.z >= minZ && c.z <= maxZ) || (c.x == maxX && c.z >= minZ && c.z <= maxZ) || (c.z == minZ && c.x >= minX && c.x <= maxX) || (c.z == maxZ && c.x >= minX && c.x <= maxX);
		}

		public bool IsOnEdge(IntVec3 c, int edgeWidth)
		{
			if (!Contains(c))
			{
				return false;
			}
			return c.x < minX + edgeWidth || c.z < minZ + edgeWidth || c.x >= maxX + 1 - edgeWidth || c.z >= maxZ + 1 - edgeWidth;
		}

		public bool IsCorner(IntVec3 c)
		{
			return (c.x == minX && c.z == minZ) || (c.x == maxX && c.z == minZ) || (c.x == minX && c.z == maxZ) || (c.x == maxX && c.z == maxZ);
		}

		public Rot4 GetClosestEdge(IntVec3 c)
		{
			int num = Mathf.Abs(c.x - minX);
			int num2 = Mathf.Abs(c.x - maxX);
			int num3 = Mathf.Abs(c.z - maxZ);
			int num4 = Mathf.Abs(c.z - minZ);
			return GenMath.MinBy(Rot4.West, (float)num, Rot4.East, (float)num2, Rot4.North, (float)num3, Rot4.South, (float)num4);
		}

		public CellRect ClipInsideMap(Map map)
		{
			if (minX < 0)
			{
				minX = 0;
			}
			if (minZ < 0)
			{
				minZ = 0;
			}
			int num = maxX;
			IntVec3 size = map.Size;
			if (num > size.x - 1)
			{
				IntVec3 size2 = map.Size;
				maxX = size2.x - 1;
			}
			int num2 = maxZ;
			IntVec3 size3 = map.Size;
			if (num2 > size3.z - 1)
			{
				IntVec3 size4 = map.Size;
				maxZ = size4.z - 1;
			}
			return this;
		}

		public CellRect ClipInsideRect(CellRect otherRect)
		{
			if (minX < otherRect.minX)
			{
				minX = otherRect.minX;
			}
			if (maxX > otherRect.maxX)
			{
				maxX = otherRect.maxX;
			}
			if (minZ < otherRect.minZ)
			{
				minZ = otherRect.minZ;
			}
			if (maxZ > otherRect.maxZ)
			{
				maxZ = otherRect.maxZ;
			}
			return this;
		}

		public bool Contains(IntVec3 c)
		{
			return c.x >= minX && c.x <= maxX && c.z >= minZ && c.z <= maxZ;
		}

		public float ClosestDistSquaredTo(IntVec3 c)
		{
			if (Contains(c))
			{
				return 0f;
			}
			if (c.x < minX)
			{
				if (c.z < minZ)
				{
					return (float)(c - new IntVec3(minX, 0, minZ)).LengthHorizontalSquared;
				}
				if (c.z > maxZ)
				{
					return (float)(c - new IntVec3(minX, 0, maxZ)).LengthHorizontalSquared;
				}
				return (float)((minX - c.x) * (minX - c.x));
			}
			if (c.x > maxX)
			{
				if (c.z < minZ)
				{
					return (float)(c - new IntVec3(maxX, 0, minZ)).LengthHorizontalSquared;
				}
				if (c.z > maxZ)
				{
					return (float)(c - new IntVec3(maxX, 0, maxZ)).LengthHorizontalSquared;
				}
				return (float)((c.x - maxX) * (c.x - maxX));
			}
			if (c.z < minZ)
			{
				return (float)((minZ - c.z) * (minZ - c.z));
			}
			return (float)((c.z - maxZ) * (c.z - maxZ));
		}

		public IntVec3 ClosestCellTo(IntVec3 c)
		{
			if (Contains(c))
			{
				return c;
			}
			if (c.x < minX)
			{
				if (c.z < minZ)
				{
					return new IntVec3(minX, 0, minZ);
				}
				if (c.z > maxZ)
				{
					return new IntVec3(minX, 0, maxZ);
				}
				return new IntVec3(minX, 0, c.z);
			}
			if (c.x > maxX)
			{
				if (c.z < minZ)
				{
					return new IntVec3(maxX, 0, minZ);
				}
				if (c.z > maxZ)
				{
					return new IntVec3(maxX, 0, maxZ);
				}
				return new IntVec3(maxX, 0, c.z);
			}
			if (c.z < minZ)
			{
				return new IntVec3(c.x, 0, minZ);
			}
			return new IntVec3(c.x, 0, maxZ);
		}

		public IEnumerable<IntVec3> GetEdgeCells(Rot4 dir)
		{
			if (dir == Rot4.North)
			{
				int x2 = minX;
				if (x2 <= maxX)
				{
					yield return new IntVec3(x2, 0, maxZ);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			else if (dir == Rot4.South)
			{
				int x = minX;
				if (x <= maxX)
				{
					yield return new IntVec3(x, 0, minZ);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			else if (dir == Rot4.West)
			{
				int z2 = minZ;
				if (z2 <= maxZ)
				{
					yield return new IntVec3(minX, 0, z2);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			else if (dir == Rot4.East)
			{
				int z = minZ;
				if (z <= maxZ)
				{
					yield return new IntVec3(maxX, 0, z);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public bool TryFindRandomInnerRectTouchingEdge(IntVec2 size, out CellRect rect, Predicate<CellRect> predicate = null)
		{
			if (Width < size.x || Height < size.z)
			{
				rect = Empty;
				return false;
			}
			if (size.x <= 0 || size.z <= 0 || IsEmpty)
			{
				rect = Empty;
				return false;
			}
			CellRect cellRect = this;
			cellRect.maxX -= size.x - 1;
			cellRect.maxZ -= size.z - 1;
			if (cellRect.EdgeCells.Where(delegate(IntVec3 x)
			{
				if (predicate == null)
				{
					return true;
				}
				CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
				return predicate(obj);
			}).TryRandomElement(out IntVec3 result))
			{
				rect = new CellRect(result.x, result.z, size.x, size.z);
				return true;
			}
			rect = Empty;
			return false;
		}

		public bool TryFindRandomInnerRect(IntVec2 size, out CellRect rect, Predicate<CellRect> predicate = null)
		{
			if (Width < size.x || Height < size.z)
			{
				rect = Empty;
				return false;
			}
			if (size.x <= 0 || size.z <= 0 || IsEmpty)
			{
				rect = Empty;
				return false;
			}
			CellRect cellRect = this;
			cellRect.maxX -= size.x - 1;
			cellRect.maxZ -= size.z - 1;
			if (cellRect.Cells.Where(delegate(IntVec3 x)
			{
				if (predicate == null)
				{
					return true;
				}
				CellRect obj = new CellRect(x.x, x.z, size.x, size.z);
				return predicate(obj);
			}).TryRandomElement(out IntVec3 result))
			{
				rect = new CellRect(result.x, result.z, size.x, size.z);
				return true;
			}
			rect = Empty;
			return false;
		}

		public CellRect ExpandedBy(int dist)
		{
			CellRect result = this;
			result.minX -= dist;
			result.minZ -= dist;
			result.maxX += dist;
			result.maxZ += dist;
			return result;
		}

		public CellRect ContractedBy(int dist)
		{
			return ExpandedBy(-dist);
		}

		public int IndexOf(IntVec3 location)
		{
			return location.x - minX + (location.z - minZ) * Width;
		}

		public void DebugDraw()
		{
			float y = AltitudeLayer.MetaOverlays.AltitudeFor();
			Vector3 vector = new Vector3((float)minX, y, (float)minZ);
			Vector3 vector2 = new Vector3((float)minX, y, (float)(maxZ + 1));
			Vector3 vector3 = new Vector3((float)(maxX + 1), y, (float)(maxZ + 1));
			Vector3 vector4 = new Vector3((float)(maxX + 1), y, (float)minZ);
			GenDraw.DrawLineBetween(vector, vector2);
			GenDraw.DrawLineBetween(vector2, vector3);
			GenDraw.DrawLineBetween(vector3, vector4);
			GenDraw.DrawLineBetween(vector4, vector);
		}

		public IEnumerator<IntVec3> GetEnumerator()
		{
			return new Enumerator(this);
		}

		public override string ToString()
		{
			return "(" + minX + "," + minZ + "," + maxX + "," + maxZ + ")";
		}

		public static CellRect FromString(string str)
		{
			str = str.TrimStart('(');
			str = str.TrimEnd(')');
			string[] array = str.Split(',');
			int num = Convert.ToInt32(array[0]);
			int num2 = Convert.ToInt32(array[1]);
			int num3 = Convert.ToInt32(array[2]);
			int num4 = Convert.ToInt32(array[3]);
			return new CellRect(num, num2, num3 - num + 1, num4 - num2 + 1);
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombineInt(seed, minX);
			seed = Gen.HashCombineInt(seed, maxX);
			seed = Gen.HashCombineInt(seed, minZ);
			return Gen.HashCombineInt(seed, maxZ);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CellRect))
			{
				return false;
			}
			return Equals((CellRect)obj);
		}

		public bool Equals(CellRect other)
		{
			return minX == other.minX && maxX == other.maxX && minZ == other.minZ && maxZ == other.maxZ;
		}
	}
}
