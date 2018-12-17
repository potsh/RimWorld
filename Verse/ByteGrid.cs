using System;

namespace Verse
{
	public sealed class ByteGrid : IExposable
	{
		private byte[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public byte this[IntVec3 c]
		{
			get
			{
				return grid[CellIndicesUtility.CellToIndex(c, mapSizeX)];
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				grid[num] = value;
			}
		}

		public byte this[int index]
		{
			get
			{
				return grid[index];
			}
			set
			{
				grid[index] = value;
			}
		}

		public byte this[int x, int z]
		{
			get
			{
				return grid[CellIndicesUtility.CellToIndex(x, z, mapSizeX)];
			}
			set
			{
				grid[CellIndicesUtility.CellToIndex(x, z, mapSizeX)] = value;
			}
		}

		public int CellsCount => grid.Length;

		public ByteGrid()
		{
		}

		public ByteGrid(Map map)
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
			if (MapSizeMatches(map) && grid != null)
			{
				Clear(0);
			}
			else
			{
				IntVec3 size = map.Size;
				mapSizeX = size.x;
				IntVec3 size2 = map.Size;
				mapSizeZ = size2.z;
				grid = new byte[mapSizeX * mapSizeZ];
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref mapSizeX, "mapSizeX", 0);
			Scribe_Values.Look(ref mapSizeZ, "mapSizeZ", 0);
			DataExposeUtility.ByteArray(ref grid, "grid");
		}

		public void Clear(byte value = 0)
		{
			if (value == 0)
			{
				Array.Clear(grid, 0, grid.Length);
			}
			else
			{
				for (int i = 0; i < grid.Length; i++)
				{
					grid[i] = value;
				}
			}
		}

		public void DebugDraw()
		{
			for (int i = 0; i < grid.Length; i++)
			{
				byte b = grid[i];
				if (b > 0)
				{
					IntVec3 c = CellIndicesUtility.IndexToCell(i, mapSizeX);
					CellRenderer.RenderCell(c, (float)(int)b / 255f * 0.5f);
				}
			}
		}
	}
}
