using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(MassUtility), "Capacity")]
internal static class Harmony_MassUtility_Capacity
{
	private static void Postfix(ref float __result, Pawn p)
	{
		if (__result != 0f)
		{
			__result = p.GetStatValue(CE_StatDefOf.CarryWeight);
		}
	}
}
