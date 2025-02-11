using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Explosion), "GetDamageAmountAt")]
public static class Harmony_ExplosionCE_GetDamageAmountAt
{
	internal static bool Prefix(Explosion __instance, ref int __result, IntVec3 c)
	{
		if (__instance is ExplosionCE explosionCE)
		{
			__result = explosionCE.GetDamageAmountAtCE(c);
			return false;
		}
		return true;
	}
}
