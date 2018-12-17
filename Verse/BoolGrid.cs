using System;
using System.Collections.Generic;

namespace Verse
{
	public class BoolGrid : IExposable
	{
		private bool[] arr;

		private int trueCountInt;

		private int mapSizeX;

		private int mapSizeZ;

		public int TrueCount => trueCountInt;

		public IEnumerable<IntVec3> ActiveCells
		{
			get
			{
				if (trueCountInt != 0)
				{
					int i = 0;
					while (true)
					{
						if (i >= arr.Length)
						{
							yield break;
						}
						if (arr[i])
						{
							break;
						}
						i++;
					}
					yield return CellIndicesUtility.IndexToCell(i, mapSizeX);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public bool this[int index]
		{
			get
			{
				return arr[index];
			}
			set
			{
				Set(index, value);
			}
		}

		public bool this[IntVec3 c]
		{
			get
			{
				return arr[CellIndicesUtility.CellToIndex(c, mapSizeX)];
			}
			set
			{
				Set(c, value);
			}
		}

		public bool this[int x, int z]
		{
			get
			{
				return arr[CellIndicesUtility.CellToIndex(x, z, mapSizeX)];
			}
			set
			{
				Set(CellIndicesUtility.CellToIndex(x, z, mapSizeX), value);
			}
		}

		public BoolGrid()
		{
		}

		public BoolGrid(Map map)
		{
			ClearAndResizeTo(map);
		}

		public bool MapSizeMatches(Map map)
		{
			int num = mapSizeX;
			IntVec3 size = map.Size;
			int result;
			if (num == size.x)
			{
				int num2 = mapSizeZ;
				IntVec3 size2 = map.Size;
				result = ((num2 == size2.z) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		public void ClearAndResizeTo(Map map)
		{
			if (MapSizeMatches(map) && arr != null)
			{
				Clear();
			}
			else
			{
				IntVec3 size = map.Size;
				mapSizeX = size.x;
				IntVec3 size2 = map.Size;
				mapSizeZ = size2.z;
				arr = new bool[mapSizeX * mapSizeZ];
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref trueCountInt, "trueCount", 0);
			Scribe_Values.Look(ref mapSizeX, "mapSizeX", 0);
			Scribe_Values.Look(ref mapSizeZ, "mapSizeZ", 0);
			DataExposeUtility.BoolArray(ref arr, mapSizeX * mapSizeZ, "arr");
		}

		public void Clear()
		{
			Array.Clear(arr, 0, arr.Length);
			trueCountInt = 0;
		}

		public virtual void Set(IntVec3 c, bool value)
		{
			Set(CellIndicesUtility.CellToIndex(c, mapSizeX), value);
		}

		public virtual void Set(int index, bool value)
		{
			if (arr[index] != value)
			{
				arr[index] = value;
				if (value)
				{
					trueCountInt++;
				}
				else
				{
					trueCountInt--;
				}
			}
		}

		public void Invert()
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = !arr[i];
			}
			trueCountInt = arr.Length - trueCountInt;
		}
	}
}
