using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class MinHeatPatches
{
	[HarmonyTargetMethods]
	public static IEnumerable<MethodInfo> TargetMethods()
	{
		Type type = typeof(Pawn_PsychicEntropyTracker);
		yield return AccessTools.Method(type, "TryAddEntropy", (Type[])null, (Type[])null);
		yield return AccessTools.Method(type, "PsychicEntropyTrackerTick", (Type[])null, (Type[])null);
		yield return AccessTools.Method(type, "RemoveAllEntropy", (Type[])null, (Type[])null);
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		bool found = false;
		foreach (CodeInstruction instruction in instructions)
		{
			int num;
			if (!found && instruction.opcode == OpCodes.Ldc_R4)
			{
				object operand = instruction.operand;
				num = ((operand is float && (float)operand == 0f) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			if (num != 0)
			{
				found = true;
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(Pawn_PsychicEntropyTracker), "pawn"));
				yield return new CodeInstruction(OpCodes.Ldsfld, (object)AccessTools.Field(typeof(VPE_DefOf), "VPE_PsychicEntropyMinimum"));
				yield return new CodeInstruction(OpCodes.Ldc_I4_1, (object)null);
				yield return new CodeInstruction(OpCodes.Ldc_I4_1, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(StatExtension), "GetStatValue", (Type[])null, (Type[])null));
			}
			else
			{
				yield return instruction;
			}
		}
	}
}
