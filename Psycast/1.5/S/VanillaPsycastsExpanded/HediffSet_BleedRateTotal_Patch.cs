using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public static class HediffSet_BleedRateTotal_Patch
{
	public static void Postfix(ref float __result, HediffSet __instance)
	{
		if (__result > 0f && __instance?.GetFirstHediffOfDef(VPE_DefOf.VPE_BlockBleeding) != null)
		{
			__result = 0f;
		}
	}
}
