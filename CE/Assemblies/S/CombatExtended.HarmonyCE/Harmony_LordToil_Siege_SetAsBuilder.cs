using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(LordToil_Siege), "SetAsBuilder")]
internal static class Harmony_LordToil_Siege_SetAsBuilder
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		CodeMatcher val = new CodeMatcher(instructions, generator);
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[2]
		{
			CodeMatch.LoadsField(AccessTools.Field(typeof(ThingDefOf), "Turret_Mortar"), false),
			CodeMatch.LoadsField(AccessTools.Field(typeof(BuildableDef), "constructionSkillPrerequisite"), false)
		}).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_SetAsBuilder couldn't find required construction skill").RemoveInstructions(2)
			.Insert((CodeInstruction[])(object)new CodeInstruction[1] { CodeInstruction.LoadField(typeof(SiegeUtility), "MinRequiredConstructionSkill", false) });
		return val.Instructions();
	}
}
