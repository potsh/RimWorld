using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class GenPlanetMorphology
	{
		private static HashSet<int> tmpOutput = new HashSet<int>();

		private static HashSet<int> tilesSet = new HashSet<int>();

		private static List<int> tmpNeighbors = new List<int>();

		private static List<int> tmpEdgeTiles = new List<int>();

		public static void Erode(List<int> tiles, int count, Predicate<int> extraPredicate = null)
		{
			if (count > 0)
			{
				WorldGrid worldGrid = Find.WorldGrid;
				tilesSet.Clear();
				tilesSet.AddRange(tiles);
				tmpEdgeTiles.Clear();
				for (int i = 0; i < tiles.Count; i++)
				{
					worldGrid.GetTileNeighbors(tiles[i], tmpNeighbors);
					for (int j = 0; j < tmpNeighbors.Count; j++)
					{
						if (!tilesSet.Contains(tmpNeighbors[j]))
						{
							tmpEdgeTiles.Add(tiles[i]);
							break;
						}
					}
				}
				if (tmpEdgeTiles.Any())
				{
					tmpOutput.Clear();
					Predicate<int> predicate = (extraPredicate == null) ? ((Predicate<int>)((int x) => tilesSet.Contains(x))) : ((Predicate<int>)((int x) => tilesSet.Contains(x) && extraPredicate(x)));
					WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
					int rootTile = -1;
					Predicate<int> passCheck = predicate;
					Func<int, int, bool> processor = delegate(int tile, int traversalDist)
					{
						if (traversalDist >= count)
						{
							tmpOutput.Add(tile);
						}
						return false;
					};
					List<int> extraRootTiles = tmpEdgeTiles;
					worldFloodFiller.FloodFill(rootTile, passCheck, processor, 2147483647, extraRootTiles);
					tiles.Clear();
					tiles.AddRange(tmpOutput);
				}
			}
		}

		public static void Dilate(List<int> tiles, int count, Predicate<int> extraPredicate = null)
		{
			if (count > 0)
			{
				WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
				int rootTile = -1;
				Predicate<int> passCheck = extraPredicate ?? ((Predicate<int>)((int x) => true));
				Func<int, int, bool> processor = delegate(int tile, int traversalDist)
				{
					if (traversalDist > count)
					{
						return true;
					}
					if (traversalDist != 0)
					{
						tiles.Add(tile);
					}
					return false;
				};
				List<int> extraRootTiles = tiles;
				worldFloodFiller.FloodFill(rootTile, passCheck, processor, 2147483647, extraRootTiles);
			}
		}

		public static void Open(List<int> tiles, int count)
		{
			Erode(tiles, count);
			Dilate(tiles, count);
		}

		public static void Close(List<int> tiles, int count)
		{
			Dilate(tiles, count);
			Erode(tiles, count);
		}
	}
}
