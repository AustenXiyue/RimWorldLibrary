using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(LordToil_Siege), "Init")]
internal static class Harmony_LordToil_Siege_Init
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		CodeMatcher val = new CodeMatcher(instructions, generator);
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.LoadsConstant(250.0) }).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_Init couldn't find market price cap").RemoveInstruction()
			.Insert((CodeInstruction[])(object)new CodeInstruction[1]
			{
				new CodeInstruction(OpCodes.Ldc_R4, (object)(-1f))
			});
		return val.Instructions();
	}
}
