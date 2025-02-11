using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Verb), "IsStillUsableBy")]
internal static class Harmony_Verb_IsStillUsableBy
{
	internal static void Postfix(Verb __instance, ref bool __result, Pawn pawn)
	{
		if (__result && __instance.tool is ToolCE toolCE)
		{
			__result = toolCE.restrictedGender == Gender.None || toolCE.restrictedGender == pawn.gender;
		}
	}
}
