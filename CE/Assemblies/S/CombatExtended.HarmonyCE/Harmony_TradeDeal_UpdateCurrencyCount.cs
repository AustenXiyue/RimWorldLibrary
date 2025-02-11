using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(TradeDeal), "UpdateCurrencyCount")]
internal static class Harmony_TradeDeal_UpdateCurrencyCount
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodBase method = typeof(Mathf).GetMethod("RoundToInt", AccessTools.all);
		MethodBase method2 = typeof(Mathf).GetMethod("CeilToInt", AccessTools.all);
		return Transpilers.MethodReplacer(instructions, method, method2);
	}
}
