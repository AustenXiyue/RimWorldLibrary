using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CombatExtended.HarmonyCE.Compatibility;

public static class Harmony_AlphaGenes
{
	[HarmonyPatch]
	public static class Harmony_CanCommandTo_Patch
	{
		public static bool Prepare()
		{
			return TypeOfCanCommandTo_Patch_HarmonyPatches != null;
		}

		public static MethodBase TargetMethod()
		{
			return AccessTools.Method("AlphaGenes.AlphaGenes_Pawn_MechanitorTracker_CanCommandTo_Patch:ModifyRange", (Type[])null, (Type[])null);
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 1225f)
				{
					yield return new CodeInstruction(OpCodes.Ldc_R4, (object)3469.21f);
				}
				else if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 225f)
				{
					yield return new CodeInstruction(OpCodes.Ldc_R4, (object)835.21f);
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}

	[HarmonyPatch]
	public static class Harmony_DrawCommandRadius_Patch
	{
		public static bool Prepare()
		{
			return TypeOfDrawCommandRadius_Patch_HarmonyPatches != null;
		}

		public static MethodBase TargetMethod()
		{
			return AccessTools.Method("AlphaGenes.AlphaGenes_Pawn_MechanitorTracker_DrawCommandRadius_Patch:DrawExtraCommandRadius", (Type[])null, (Type[])null);
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 35f)
				{
					yield return new CodeInstruction(OpCodes.Ldc_R4, (object)58.9f);
				}
				else if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 15f)
				{
					yield return new CodeInstruction(OpCodes.Ldc_R4, (object)28.9f);
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}

	private static Type TypeOfCanCommandTo_Patch_HarmonyPatches => AccessTools.TypeByName("AlphaGenes.AlphaGenes_Pawn_MechanitorTracker_CanCommandTo_Patch");

	private static Type TypeOfDrawCommandRadius_Patch_HarmonyPatches => AccessTools.TypeByName("AlphaGenes.AlphaGenes_Pawn_MechanitorTracker_DrawCommandRadius_Patch");
}
