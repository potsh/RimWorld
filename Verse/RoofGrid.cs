using RimWorld;
using UnityEngine;

namespace Verse
{
	public sealed class RoofGrid : IExposable, ICellBoolGiver
	{
		private Map map;

		private RoofDef[] roofGrid;

		private CellBoolDrawer drawerInt;

		public CellBoolDrawer Drawer
		{
			get
			{
				if (drawerInt == null)
				{
					IntVec3 size = map.Size;
					int x = size.x;
					IntVec3 size2 = map.Size;
					drawerInt = new CellBoolDrawer(this, x, size2.z);
				}
				return drawerInt;
			}
		}

		public Color Color => new Color(0.3f, 1f, 0.4f);

		public RoofGrid(Map map)
		{
			this.map = map;
			roofGrid = new RoofDef[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			MapExposeUtility.ExposeUshort(map, (IntVec3 c) => (ushort)((roofGrid[map.cellIndices.CellToIndex(c)] != null) ? roofGrid[map.cellIndices.CellToIndex(c)].shortHash : 0), delegate(IntVec3 c, ushort val)
			{
				SetRoof(c, DefDatabase<RoofDef>.GetByShortHash(val));
			}, "roofs");
		}

		public bool GetCellBool(int index)
		{
			return roofGrid[index] != null && !map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			if (RoofDefOf.RoofRockThick != null && roofGrid[index] == RoofDefOf.RoofRockThick)
			{
				return Color.gray;
			}
			return Color.white;
		}

		public bool Roofed(int index)
		{
			return roofGrid[index] != null;
		}

		public bool Roofed(int x, int z)
		{
			return roofGrid[map.cellIndices.CellToIndex(x, z)] != null;
		}

		public bool Roofed(IntVec3 c)
		{
			return roofGrid[map.cellIndices.CellToIndex(c)] != null;
		}

		public RoofDef RoofAt(int index)
		{
			return roofGrid[index];
		}

		public RoofDef RoofAt(IntVec3 c)
		{
			return roofGrid[map.cellIndices.CellToIndex(c)];
		}

		public RoofDef RoofAt(int x, int z)
		{
			return roofGrid[map.cellIndices.CellToIndex(x, z)];
		}

		public void SetRoof(IntVec3 c, RoofDef def)
		{
			if (roofGrid[map.cellIndices.CellToIndex(c)] != def)
			{
				roofGrid[map.cellIndices.CellToIndex(c)] = def;
				map.glowGrid.MarkGlowGridDirty(c);
				map.regionGrid.GetValidRegionAt_NoRebuild(c)?.Room.Notify_RoofChanged();
				if (drawerInt != null)
				{
					drawerInt.SetDirty();
				}
				map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Roofs);
			}
		}

		public void RoofGridUpdate()
		{
			if (Find.PlaySettings.showRoofOverlay)
			{
				Drawer.MarkForDraw();
			}
			Drawer.CellBoolDrawerUpdate();
		}
	}
}
