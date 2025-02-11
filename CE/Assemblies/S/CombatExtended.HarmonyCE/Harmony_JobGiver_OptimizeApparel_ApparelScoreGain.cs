using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
internal static class Harmony_JobGiver_OptimizeApparel_ApparelScoreGain
{
	internal static bool Prefix(Pawn pawn, Apparel ap, ref float __result)
	{
		bool valueOrDefault = pawn?.equipment?.Primary?.def?.weaponTags?.Contains("CE_OneHandedWeapon") == true;
		if (ap is Apparel_Shield && pawn?.equipment?.Primary != null && !valueOrDefault)
		{
			__result = -1000f;
			return false;
		}
		return true;
	}
}
