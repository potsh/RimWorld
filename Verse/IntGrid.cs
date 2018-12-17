using System;

namespace Verse
{
	public sealed class IntGrid
	{
		private int[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public int this[IntVec3 c]
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

		public int this[int index]
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

		public int this[int x, int z]
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

		public IntGrid()
		{
		}

		public IntGrid(Map map)
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
				Clear();
			}
			else
			{
				IntVec3 size = map.Size;
				mapSizeX = size.x;
				IntVec3 size2 = map.Size;
				mapSizeZ = size2.z;
				grid = new int[mapSizeX * mapSizeZ];
			}
		}

		public void Clear(int value = 0)
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
				int num = grid[i];
				if (num > 0)
				{
					IntVec3 c = CellIndicesUtility.IndexToCell(i, mapSizeX);
					CellRenderer.RenderCell(c, (float)(num % 100) / 100f * 0.5f);
				}
			}
		}
	}
}
