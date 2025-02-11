using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(HediffComp_TendDuration))]
[HarmonyPatch("CompTended")]
[HarmonyPatch(new Type[]
{
	typeof(float),
	typeof(float),
	typeof(int)
})]
internal static class HediffComp_TendDuration_CompTended
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int countReplace = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (countReplace < 2 && instruction.opcode == OpCodes.Ldc_R4 && (CodeInstructionExtensions.OperandIs(instruction, (object)(-0.25f)) || CodeInstructionExtensions.OperandIs(instruction, (object)0.25f)))
			{
				instruction.operand = ((countReplace == 0) ? (-0.15f) : 0.15f);
				countReplace++;
			}
			yield return instruction;
		}
	}
}
