using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(FireUtility), "ChanceToStartFireIn")]
internal static class Harmony_FireUtility_ChanceToStartFireIn
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		bool write = false;
		foreach (CodeInstruction code in instructions)
		{
			if (write)
			{
				if (code.opcode == OpCodes.Ldc_I4_1)
				{
					code.opcode = OpCodes.Ldc_I4_M1;
				}
				write = false;
			}
			else if (code.opcode == OpCodes.Ldfld && code.operand == AccessTools.Field(typeof(ThingDef), "category"))
			{
				write = true;
			}
			yield return code;
		}
	}
}
