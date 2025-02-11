using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "get_SpreadInterval")]
internal static class Harmony_Fire_SpreadInterval
{
	internal static bool Prefix(Fire __instance, ref float __result)
	{
		__result = (float)FireSpread.values.baseSpreadTicks - __instance.fireSize * FireSpread.values.fireSizeMultiplier;
		float f = __instance.Map.GetComponent<WeatherTracker>().GetWindStrengthAt(__instance.PositionHeld) * FireSpread.values.windSpeedMultiplier;
		__result /= Mathf.Max(1f, Mathf.Sqrt(f));
		if (__result < (float)FireSpread.values.minSpreadTicks)
		{
			__result = FireSpread.values.minSpreadTicks;
		}
		return false;
	}
}
