using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "SpawnSmokeParticles")]
internal static class Harmony_Fire_SpawnSmokeParticles
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction code in instructions)
		{
			if (code.opcode == OpCodes.Ldc_I4_S && code.operand is sbyte && (sbyte)code.operand == 15)
			{
				yield return new CodeInstruction(OpCodes.Ldc_I4, (object)1500);
			}
			else
			{
				yield return code;
			}
		}
	}

	internal static void Postfix(Fire __instance)
	{
	}
}
