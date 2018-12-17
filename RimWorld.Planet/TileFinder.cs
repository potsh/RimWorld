using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class TileFinder
	{
		private static List<Pair<int, int>> tmpTiles = new List<Pair<int, int>>();

		private static List<int> tmpPlayerTiles = new List<int>();

		public static int RandomStartingTile()
		{
			return RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: true);
		}

		public static int RandomSettlementTileFor(Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
		{
			for (int i = 0; i < 500; i++)
			{
				if ((from _ in Enumerable.Range(0, 100)
				select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
				{
					Tile tile = Find.WorldGrid[x];
					if (!tile.biome.canBuildBase || !tile.biome.implemented || tile.hilliness == Hilliness.Impassable)
					{
						return 0f;
					}
					if (mustBeAutoChoosable && !tile.biome.canAutoChoose)
					{
						return 0f;
					}
					if (extraValidator != null && !extraValidator(x))
					{
						return 0f;
					}
					return tile.biome.settlementSelectionWeight;
				}, out int result) && IsValidTileForNewSettlement(result))
				{
					return result;
				}
			}
			Log.Error("Failed to find faction base tile for " + faction);
			return 0;
		}

		public static bool IsValidTileForNewSettlement(int tile, StringBuilder reason = null)
		{
			Tile tile2 = Find.WorldGrid[tile];
			if (!tile2.biome.canBuildBase)
			{
				reason?.Append("CannotLandBiome".Translate(tile2.biome.LabelCap));
				return false;
			}
			if (!tile2.biome.implemented)
			{
				reason?.Append("BiomeNotImplemented".Translate() + ": " + tile2.biome.LabelCap);
				return false;
			}
			if (tile2.hilliness == Hilliness.Impassable)
			{
				reason?.Append("CannotLandImpassableMountains".Translate());
				return false;
			}
			SettlementBase settlementBase = Find.WorldObjects.SettlementBaseAt(tile);
			if (settlementBase != null)
			{
				if (reason != null)
				{
					if (settlementBase.Faction == null)
					{
						reason.Append("TileOccupied".Translate());
					}
					else if (settlementBase.Faction == Faction.OfPlayer)
					{
						reason.Append("YourBaseAlreadyThere".Translate());
					}
					else
					{
						reason.Append("BaseAlreadyThere".Translate(settlementBase.Faction.Name));
					}
				}
				return false;
			}
			if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
			{
				reason?.Append("FactionBaseAdjacent".Translate());
				return false;
			}
			if (Find.WorldObjects.AnyMapParentAt(tile) || Current.Game.FindMap(tile) != null || Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
			{
				reason?.Append("TileOccupied".Translate());
				return false;
			}
			return true;
		}

		public static bool TryFindPassableTileWithTraversalDistance(int rootTile, int minDist, int maxDist, out int result, Predicate<int> validator = null, bool ignoreFirstTilePassability = false, bool preferCloserTiles = false, bool canTraverseImpassable = false)
		{
			tmpTiles.Clear();
			Find.WorldFloodFiller.FloodFill(rootTile, (int x) => canTraverseImpassable || !Find.World.Impassable(x) || (x == rootTile && ignoreFirstTilePassability), delegate(int tile, int traversalDistance)
			{
				if (traversalDistance > maxDist)
				{
					return true;
				}
				if (traversalDistance >= minDist && !Find.World.Impassable(tile) && (validator == null || validator(tile)))
				{
					tmpTiles.Add(new Pair<int, int>(tile, traversalDistance));
				}
				return false;
			});
			Pair<int, int> result2;
			if (preferCloserTiles)
			{
				if (tmpTiles.TryRandomElementByWeight((Pair<int, int> x) => 1f - (float)(x.Second - minDist) / ((float)(maxDist - minDist) + 0.01f), out result2))
				{
					result = result2.First;
					return true;
				}
				result = -1;
				return false;
			}
			if (tmpTiles.TryRandomElement(out result2))
			{
				result = result2.First;
				return true;
			}
			result = -1;
			return false;
		}

		public static bool TryFindRandomPlayerTile(out int tile, bool allowCaravans, Predicate<int> validator = null)
		{
			tmpPlayerTiles.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && maps[i].mapPawns.FreeColonistsSpawnedCount != 0 && (validator == null || validator(maps[i].Tile)))
				{
					tmpPlayerTiles.Add(maps[i].Tile);
				}
			}
			if (allowCaravans)
			{
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled && (validator == null || validator(caravans[j].Tile)))
					{
						tmpPlayerTiles.Add(caravans[j].Tile);
					}
				}
			}
			if (tmpPlayerTiles.TryRandomElement(out tile))
			{
				return true;
			}
			if ((from x in Find.Maps
			where x.IsPlayerHome && (validator == null || validator(x.Tile))
			select x).TryRandomElement(out Map result))
			{
				tile = result.Tile;
				return true;
			}
			if ((from x in Find.Maps
			where x.mapPawns.FreeColonistsSpawnedCount != 0 && (validator == null || validator(x.Tile))
			select x).TryRandomElement(out Map result2))
			{
				tile = result2.Tile;
				return true;
			}
			if (!allowCaravans && (from x in Find.WorldObjects.Caravans
			where x.IsPlayerControlled && (validator == null || validator(x.Tile))
			select x).TryRandomElement(out Caravan result3))
			{
				tile = result3.Tile;
				return true;
			}
			tile = -1;
			return false;
		}

		public static bool TryFindNewSiteTile(out int tile, int minDist = 7, int maxDist = 27, bool allowCaravans = false, bool preferCloserTiles = true, int nearThisTile = -1)
		{
			Func<int, int> findTile = delegate(int root)
			{
				int minDist2 = minDist;
				int maxDist2 = maxDist;
				Predicate<int> validator = (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && IsValidTileForNewSettlement(x);
				bool preferCloserTiles2 = preferCloserTiles;
				int result = default(int);
				if (TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator, ignoreFirstTilePassability: false, preferCloserTiles2))
				{
					return result;
				}
				return -1;
			};
			int tile2;
			if (nearThisTile != -1)
			{
				tile2 = nearThisTile;
			}
			else if (!TryFindRandomPlayerTile(out tile2, allowCaravans, (int x) => findTile(x) != -1))
			{
				tile = -1;
				return false;
			}
			tile = findTile(tile2);
			return tile != -1;
		}
	}
}
