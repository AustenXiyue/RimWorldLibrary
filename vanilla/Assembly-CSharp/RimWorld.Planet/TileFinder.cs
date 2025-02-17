using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

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
				float num = tile.biome.settlementSelectionWeight;
				if (faction?.def.minSettlementTemperatureChanceCurve != null)
				{
					num *= faction.def.minSettlementTemperatureChanceCurve.Evaluate(GenTemperature.MinTemperatureAtTile(x));
				}
				return num;
			}, out var result) && IsValidTileForNewSettlement(result))
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
		Settlement settlement = Find.WorldObjects.SettlementBaseAt(tile);
		if (settlement != null)
		{
			if (reason != null)
			{
				if (settlement.Faction == null)
				{
					reason.Append("TileOccupied".Translate());
				}
				else if (settlement.Faction == Faction.OfPlayer)
				{
					reason.Append("YourBaseAlreadyThere".Translate());
				}
				else
				{
					reason.Append("BaseAlreadyThere".Translate(settlement.Faction.Name));
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

	public static bool TryFindPassableTileWithTraversalDistance(int rootTile, int minDist, int maxDist, out int result, Predicate<int> validator = null, bool ignoreFirstTilePassability = false, TileFinderMode tileFinderMode = TileFinderMode.Random, bool canTraverseImpassable = false, bool exitOnFirstTileFound = false)
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
				if (exitOnFirstTileFound)
				{
					return true;
				}
			}
			return false;
		});
		if (exitOnFirstTileFound)
		{
			if (tmpTiles.Count > 0)
			{
				result = tmpTiles[0].First;
				return true;
			}
			result = -1;
			return false;
		}
		Pair<int, int> result2;
		switch (tileFinderMode)
		{
		case TileFinderMode.Near:
			if (tmpTiles.TryRandomElementByWeight((Pair<int, int> x) => 1f - (float)(x.Second - minDist) / ((float)(maxDist - minDist) + 0.01f), out result2))
			{
				result = result2.First;
				return true;
			}
			result = -1;
			return false;
		case TileFinderMode.Furthest:
			if (tmpTiles.Count > 0)
			{
				int maxDistanceWithOffset = Mathf.Clamp(tmpTiles.MaxBy((Pair<int, int> t) => t.Second).Second - 2, minDist, maxDist);
				if (tmpTiles.Where((Pair<int, int> t) => t.Second >= maxDistanceWithOffset - 1).TryRandomElement(out var result3))
				{
					result = result3.First;
					return true;
				}
			}
			result = -1;
			return false;
		case TileFinderMode.Random:
			if (tmpTiles.TryRandomElement(out result2))
			{
				result = result2.First;
				return true;
			}
			result = -1;
			return false;
		default:
			Log.Error($"Unknown tile distance preference {tileFinderMode}.");
			result = -1;
			return false;
		}
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
		if (Find.Maps.Where((Map x) => x.IsPlayerHome && (validator == null || validator(x.Tile))).TryRandomElement(out var result))
		{
			tile = result.Tile;
			return true;
		}
		if (Find.Maps.Where((Map x) => x.mapPawns.FreeColonistsSpawnedCount != 0 && (validator == null || validator(x.Tile))).TryRandomElement(out var result2))
		{
			tile = result2.Tile;
			return true;
		}
		if (!allowCaravans && Find.WorldObjects.Caravans.Where((Caravan x) => x.IsPlayerControlled && (validator == null || validator(x.Tile))).TryRandomElement(out var result3))
		{
			tile = result3.Tile;
			return true;
		}
		tile = -1;
		return false;
	}

	public static bool TryFindNewSiteTile(out int tile, int minDist = 7, int maxDist = 27, bool allowCaravans = false, TileFinderMode tileFinderMode = TileFinderMode.Near, int nearThisTile = -1, bool exitOnFirstTileFound = false)
	{
		Func<int, int> findTile = delegate(int root)
		{
			if (TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out var result, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && IsValidTileForNewSettlement(x), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound))
			{
				return result;
			}
			return TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out result, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && IsValidTileForNewSettlement(x) && (!Find.World.Impassable(x) || Find.WorldGrid[x].WaterCovered), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: true, exitOnFirstTileFound) ? result : (-1);
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
