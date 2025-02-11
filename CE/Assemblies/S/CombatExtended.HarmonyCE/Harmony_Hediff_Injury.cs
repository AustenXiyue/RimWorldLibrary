using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

internal static class Harmony_Hediff_Injury
{
	[HarmonyPatch(typeof(Hediff_Injury), "PostAdd")]
	private static class Patch_PostAdd
	{
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> SilenceLogSpam(IEnumerable<CodeInstruction> instructions)
		{
			int patchPhase = 0;
			bool emitOriginal = true;
			foreach (CodeInstruction instruction in instructions)
			{
				switch (patchPhase)
				{
				case 0:
					if (instruction.opcode == OpCodes.Ldfld && instruction.operand == AccessTools.Field(typeof(BodyPartRecord), "coverageAbs"))
					{
						patchPhase = 1;
					}
					break;
				case 1:
					if (instruction.opcode == OpCodes.Bgt_Un)
					{
						emitOriginal = false;
						patchPhase = 2;
						yield return instruction;
						yield return new CodeInstruction(OpCodes.Nop, (object)null);
					}
					break;
				case 2:
					if (instruction.opcode == OpCodes.Ret)
					{
						emitOriginal = true;
						patchPhase = -1;
					}
					break;
				}
				if (emitOriginal)
				{
					yield return instruction;
				}
			}
		}
	}
}
