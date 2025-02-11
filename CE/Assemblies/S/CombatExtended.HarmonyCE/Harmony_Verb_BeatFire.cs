using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

internal class Harmony_Verb_BeatFire
{
	[HarmonyPatch(typeof(Verb_BeatFire), "TryCastShot")]
	private class EditFirePunchingStrength
	{
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float amount && amount == 32f)
				{
					instruction.operand = 48f;
				}
				yield return instruction;
			}
		}
	}
}
