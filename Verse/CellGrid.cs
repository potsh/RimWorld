namespace Verse
{
	public class CellGrid
	{
		private int[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public IntVec3 this[IntVec3 c]
		{
			get
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				return CellIndicesUtility.IndexToCell(grid[num], mapSizeX);
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				grid[num] = CellIndicesUtility.CellToIndex(value, mapSizeX);
			}
		}

		public IntVec3 this[int index]
		{
			get
			{
				return CellIndicesUtility.IndexToCell(grid[index], mapSizeX);
			}
			set
			{
				grid[index] = CellIndicesUtility.CellToIndex(value, mapSizeX);
			}
		}

		public IntVec3 this[int x, int z]
		{
			get
			{
				int num = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
				return CellIndicesUtility.IndexToCell(grid[num], mapSizeX);
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
				grid[num] = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
			}
		}

		public int CellsCount => grid.Length;

		public CellGrid()
		{
		}

		public CellGrid(Map map)
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
				Clear();
			}
		}

		public void Clear()
		{
			int num = CellIndicesUtility.CellToIndex(IntVec3.Invalid, mapSizeX);
			for (int i = 0; i < grid.Length; i++)
			{
				grid[i] = num;
			}
		}
	}
}
