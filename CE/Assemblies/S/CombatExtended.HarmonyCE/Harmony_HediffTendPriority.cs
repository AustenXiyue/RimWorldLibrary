using HarmonyLib;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal class Harmony_HediffTendPriority
{
	internal static void Postfix(Hediff __instance, ref float __result)
	{
		HediffComp_Stabilize hediffComp_Stabilize = (__instance as HediffWithComps)?.TryGetComp<HediffComp_Stabilize>() ?? null;
		if (hediffComp_Stabilize != null)
		{
			__result = Mathf.Max(__result, __instance.BleedRate * 1.5f + hediffComp_Stabilize.StabilizedBleed * 0.3f);
		}
	}
}
