using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(DamageWorker_AddInjury), "ShouldReduceDamageToPreservePart")]
internal static class Patch_ShouldReduceDamageToPreservePart
{
	[HarmonyPrefix]
	private static bool Prefix(ref bool __result, BodyPartRecord bodyPart)
	{
		__result = false;
		return false;
	}
}
