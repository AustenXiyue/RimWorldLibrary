using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Thing), "TakeDamage")]
public static class PatchCETakeDamage
{
	private static void Postfix(Thing __instance, DamageWorker.DamageResult __result)
	{
		if (__instance is Pawn thing)
		{
			thing.TryGetComp<CompAmmoExploder>()?.PostDamageResult(__result);
		}
	}
}
