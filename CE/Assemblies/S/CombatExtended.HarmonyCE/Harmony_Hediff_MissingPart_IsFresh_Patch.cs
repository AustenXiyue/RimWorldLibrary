using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Hediff_MissingPart))]
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class Harmony_Hediff_MissingPart_IsFresh_Patch
{
	public static bool Prefix(Hediff_MissingPart __instance, ref bool __result)
	{
		__result = Current.ProgramState != 0 && __instance.IsFresh && !__instance.Part.def.IsSolid(__instance.Part, __instance.pawn.health.hediffSet.hediffs) && !__instance.ParentIsMissing && __instance.lastInjury != HediffDefOf.SurgicalCut;
		return false;
	}
}
