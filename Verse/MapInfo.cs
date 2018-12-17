using RimWorld.Planet;

namespace Verse
{
	public sealed class MapInfo : IExposable
	{
		private IntVec3 sizeInt;

		public MapParent parent;

		public int Tile => parent.Tile;

		public int NumCells
		{
			get
			{
				IntVec3 size = Size;
				int x = size.x;
				IntVec3 size2 = Size;
				int num = x * size2.y;
				IntVec3 size3 = Size;
				return num * size3.z;
			}
		}

		public IntVec3 Size
		{
			get
			{
				return sizeInt;
			}
			set
			{
				sizeInt = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref sizeInt, "size");
			Scribe_References.Look(ref parent, "parent");
		}
	}
}
