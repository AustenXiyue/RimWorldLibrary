using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Bombardment), "TryDoExplosion")]
public static class Bombardment_TryDoExplosion_Patch
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_I4_M1)
			{
				yield return new CodeInstruction(OpCodes.Ldc_I4, (object)546);
			}
			else if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == -1f)
			{
				yield return new CodeInstruction(OpCodes.Ldc_R4, (object)180f);
			}
			else
			{
				yield return instruction;
			}
		}
	}
}
