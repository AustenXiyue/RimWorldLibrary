using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

[HarmonyPatch]
[StaticConstructorOnStartup]
public class CompDarkener : CompGlower
{
	private static readonly FieldRef<object, CompGlower> lightGlower;

	private static readonly Dictionary<Map, HashSet<IntVec3>> darkCells;

	protected override bool ShouldBeLitNow => true;

	static CompDarkener()
	{
		darkCells = new Dictionary<Map, HashSet<IntVec3>>();
		Type type = AccessTools.Inner(typeof(GlowGrid), "Light");
		lightGlower = AccessTools.FieldRefAccess<CompGlower>(type, "glower");
	}

	[HarmonyPatch(typeof(GlowGrid), "CombineColors")]
	[HarmonyPrefix]
	public static bool CombineColorsDark(ref Color32 __result, CompGlower toAddGlower)
	{
		if (toAddGlower is CompDarkener)
		{
			__result = new Color32(0, 0, 0, 0);
			return false;
		}
		return true;
	}

	[HarmonyPatch(typeof(GlowGrid), "RegisterGlower")]
	[HarmonyPostfix]
	public static void MoveDarkLast(List<object> ___lights)
	{
		List<object> list = new List<object>();
		int count = ___lights.Count;
		while (count-- > 0)
		{
			object obj = ___lights[count];
			if (lightGlower.Invoke(obj) is CompDarkener)
			{
				list.Add(obj);
				___lights.RemoveAt(count);
			}
		}
		___lights.AddRange(list);
	}

	[HarmonyPatch(typeof(GlowGrid), "GroundGlowAt")]
	[HarmonyPrefix]
	public static void IgnoreSkyDark(IntVec3 c, ref bool ignoreSky, Map ___map)
	{
		if (darkCells.TryGetValue(___map, out var value) && value.Contains(c))
		{
			ignoreSky = true;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		RecacheDarkCells(parent.Map);
	}

	public override void PostDeSpawn(Map map)
	{
		base.PostDeSpawn(map);
		RecacheDarkCells(map);
	}

	private static void RecacheDarkCells(Map map)
	{
		if (!darkCells.TryGetValue(map, out var value))
		{
			value = new HashSet<IntVec3>();
		}
		foreach (Thing allThing in map.listerThings.AllThings)
		{
			if (!(allThing.TryGetComp<CompGlower>() is CompDarkener compDarkener))
			{
				continue;
			}
			foreach (IntVec3 item in GenRadial.RadialCellsAround(allThing.Position, compDarkener.GlowRadius, useCenter: true))
			{
				value.Add(item);
			}
		}
		if (value.Count == 0)
		{
			darkCells.Remove(map);
		}
		else
		{
			darkCells[map] = value;
		}
	}
}
