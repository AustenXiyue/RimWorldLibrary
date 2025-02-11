using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(TradeUtility), "GetPricePlayerBuy")]
internal static class Harmony_TradeUtility
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_R4 && (instruction.operand?.Equals(0.5f) ?? false))
			{
				instruction.operand = 0.01f;
			}
			yield return instruction;
		}
	}
}
