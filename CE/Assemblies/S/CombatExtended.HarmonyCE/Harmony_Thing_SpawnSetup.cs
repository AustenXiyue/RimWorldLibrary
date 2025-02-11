using System.Runtime.CompilerServices;
using CombatExtended.Utilities;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Thing), "SpawnSetup")]
public class Harmony_Thing_SpawnSetup
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[HarmonyPriority(800)]
	public static void Postfix(Thing __instance)
	{
		ThingsTracker.GetTracker(__instance.Map)?.Notify_Spawned(__instance);
	}
}
