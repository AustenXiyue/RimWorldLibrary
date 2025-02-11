using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

internal static class Patch_CheckDuplicateDamageToOuterParts
{
	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(DamageWorker_AddInjury), "CheckDuplicateDamageToOuterParts")]
	private static class Patch_DamageWorker_AddInjury
	{
		[HarmonyPrefix]
		private static bool Prefix(DamageWorker_AddInjury __instance, DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result)
		{
			CE_Utility.DamageOutsideSquishy(__instance, dinfo, pawn, totalDamage, result, lastHitPartHealth);
			return true;
		}
	}

	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(DamageWorker_Cut), "ApplySpecialEffectsToPart")]
	private static class Patch_DamageWorker_Cut
	{
		[HarmonyPrefix]
		private static bool Prefix(DamageWorker_Cut __instance, DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result)
		{
			CE_Utility.DamageOutsideSquishy(__instance, dinfo, pawn, totalDamage, result, lastHitPartHealth);
			return true;
		}
	}

	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(DamageWorker_Stab), "ApplySpecialEffectsToPart")]
	private static class Patch_DamageWorker_Stab
	{
		[HarmonyPrefix]
		private static bool Prefix(DamageWorker_Stab __instance, DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result)
		{
			CE_Utility.DamageOutsideSquishy(__instance, dinfo, pawn, totalDamage, result, lastHitPartHealth);
			return true;
		}
	}

	public static float lastHitPartHealth;
}
