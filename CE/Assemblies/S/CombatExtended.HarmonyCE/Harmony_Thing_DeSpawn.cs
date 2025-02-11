using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CombatExtended.Utilities;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
public class Harmony_Thing_DeSpawn
{
	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AccessTools.Method(typeof(Thing), "DeSpawn", (Type[])null, (Type[])null);
		yield return AccessTools.Method(typeof(Thing), "ForceSetStateToUnspawned", (Type[])null, (Type[])null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[HarmonyPriority(800)]
	public static void Prefix(Thing __instance)
	{
		ThingsTracker.GetTracker(__instance.Map)?.Notify_DeSpawned(__instance);
	}
}
