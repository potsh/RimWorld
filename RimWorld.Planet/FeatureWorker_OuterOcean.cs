using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_OuterOcean : FeatureWorker
	{
		private List<int> group = new List<int>();

		private List<int> edgeTiles = new List<int>();

		public override void GenerateWhereAppropriate()
		{
			WorldGrid worldGrid = Find.WorldGrid;
			int tilesCount = worldGrid.TilesCount;
			edgeTiles.Clear();
			for (int i = 0; i < tilesCount; i++)
			{
				if (IsRoot(i))
				{
					edgeTiles.Add(i);
				}
			}
			if (edgeTiles.Any())
			{
				group.Clear();
				WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
				int rootTile = -1;
				Predicate<int> passCheck = (int x) => CanTraverse(x);
				Func<int, int, bool> processor = delegate(int tile, int traversalDist)
				{
					group.Add(tile);
					return false;
				};
				List<int> extraRootTiles = edgeTiles;
				worldFloodFiller.FloodFill(rootTile, passCheck, processor, 2147483647, extraRootTiles);
				group.RemoveAll((int x) => worldGrid[x].feature != null);
				if (group.Count >= def.minSize && group.Count <= def.maxSize)
				{
					AddFeature(group, group);
				}
			}
		}

		private bool IsRoot(int tile)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			return worldGrid.IsOnEdge(tile) && CanTraverse(tile) && worldGrid[tile].feature == null;
		}

		private bool CanTraverse(int tile)
		{
			BiomeDef biome = Find.WorldGrid[tile].biome;
			return biome == BiomeDefOf.Ocean || biome == BiomeDefOf.Lake;
		}
	}
}
