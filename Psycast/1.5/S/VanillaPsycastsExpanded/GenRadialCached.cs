using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class GenRadialCached
{
	private struct Key
	{
		public IntVec3 loc;

		public float radius;

		public int mapId;
	}

	private static Dictionary<Key, HashSet<Thing>> cache = new Dictionary<Key, HashSet<Thing>>();

	public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
	{
		Key key = default(Key);
		key.loc = center;
		key.radius = radius;
		key.mapId = map.Index;
		Key key2 = key;
		if (cache == null)
		{
			cache = new Dictionary<Key, HashSet<Thing>>();
		}
		if (cache.TryGetValue(key2, out var value))
		{
			return value;
		}
		value = new HashSet<Thing>();
		int num = GenRadial.NumCellsInRadius(radius);
		for (int i = ((!useCenter) ? 1 : 0); i < num; i++)
		{
			IntVec3 c = GenRadial.RadialPattern[i] + center;
			if (c.InBounds(map))
			{
				value.UnionWith(c.GetThingList(map));
			}
		}
		cache.Add(key2, value);
		return value;
	}

	[HarmonyPatch(typeof(Thing), "SpawnSetup")]
	[HarmonyPostfix]
	public static void SpawnSetup_Postfix(Thing __instance)
	{
		ClearCacheFor(__instance);
	}

	[HarmonyPatch(typeof(Thing), "DeSpawn")]
	[HarmonyPrefix]
	public static void DeSpawn_Prefix(Thing __instance)
	{
		ClearCacheFor(__instance);
	}

	[HarmonyPatch(typeof(MapDeiniter), "Deinit")]
	[HarmonyPostfix]
	public static void Deinit_Postfix(Map map)
	{
		int index = map.Index;
		foreach (var (key2, value) in cache.ToList())
		{
			if (key2.mapId >= index)
			{
				cache.Remove(key2);
			}
			if (key2.mapId > index)
			{
				Key key3 = key2;
				key3.mapId--;
				cache.Add(key3, value);
			}
		}
	}

	private static void ClearCacheFor(Thing thing)
	{
		if (thing.Spawned)
		{
			cache.RemoveAll((KeyValuePair<Key, HashSet<Thing>> pair) => pair.Key.mapId == thing.Map.Index && thing.OccupiedRect().ClosestCellTo(pair.Key.loc).InHorDistOf(pair.Key.loc, pair.Key.radius));
		}
	}
}
