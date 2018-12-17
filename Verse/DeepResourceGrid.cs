using UnityEngine;

namespace Verse
{
	public sealed class DeepResourceGrid : ICellBoolGiver, IExposable
	{
		private Map map;

		private CellBoolDrawer drawer;

		private ushort[] defGrid;

		private ushort[] countGrid;

		public Color Color => Color.white;

		public DeepResourceGrid(Map map)
		{
			this.map = map;
			defGrid = new ushort[map.cellIndices.NumGridCells];
			countGrid = new ushort[map.cellIndices.NumGridCells];
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			drawer = new CellBoolDrawer(this, x, size2.z, 1f);
		}

		public void ExposeData()
		{
			MapExposeUtility.ExposeUshort(map, (IntVec3 c) => defGrid[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, ushort val)
			{
				defGrid[map.cellIndices.CellToIndex(c)] = val;
			}, "defGrid");
			MapExposeUtility.ExposeUshort(map, (IntVec3 c) => countGrid[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, ushort val)
			{
				countGrid[map.cellIndices.CellToIndex(c)] = val;
			}, "countGrid");
		}

		public ThingDef ThingDefAt(IntVec3 c)
		{
			return DefDatabase<ThingDef>.GetByShortHash(defGrid[map.cellIndices.CellToIndex(c)]);
		}

		public int CountAt(IntVec3 c)
		{
			return countGrid[map.cellIndices.CellToIndex(c)];
		}

		public void SetAt(IntVec3 c, ThingDef def, int count)
		{
			if (count == 0)
			{
				def = null;
			}
			ushort num = def?.shortHash ?? 0;
			ushort num2 = (ushort)count;
			if (count > 65535)
			{
				Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
				num2 = ushort.MaxValue;
			}
			if (count < 0)
			{
				Log.Error("Cannot store count " + count + " in DeepResourceGrid: out of ushort range.");
				num2 = 0;
			}
			int num3 = map.cellIndices.CellToIndex(c);
			if (defGrid[num3] != num || countGrid[num3] != num2)
			{
				defGrid[num3] = num;
				countGrid[num3] = num2;
				drawer.SetDirty();
			}
		}

		public void DeepResourceGridUpdate()
		{
			drawer.CellBoolDrawerUpdate();
			if (DebugViewSettings.drawDeepResources)
			{
				MarkForDraw();
			}
		}

		public void MarkForDraw()
		{
			if (map == Find.CurrentMap)
			{
				drawer.MarkForDraw();
			}
		}

		public bool GetCellBool(int index)
		{
			return CountAt(map.cellIndices.IndexToCell(index)) > 0;
		}

		public Color GetCellExtraColor(int index)
		{
			IntVec3 c = map.cellIndices.IndexToCell(index);
			int num = CountAt(c);
			ThingDef thingDef = ThingDefAt(c);
			float num2 = (float)num / (float)thingDef.deepCountPerCell / 2f;
			int num3 = Mathf.RoundToInt(num2 * 100f);
			num3 %= 100;
			return DebugMatsSpectrum.Mat(num3, transparent: true).color;
		}
	}
}
