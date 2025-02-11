using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

internal static class PawnColumnWorkers_Resize
{
	private static readonly float orgMinWidth = 194f;

	private static readonly float orgOptimalWidth = 251f;

	public static void Patch()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		Type[] array = new Type[3]
		{
			typeof(PawnColumnWorker_Outfit),
			typeof(PawnColumnWorker_DrugPolicy),
			typeof(PawnColumnWorker_FoodRestriction)
		};
		string[] array2 = new string[2] { "GetMinWidth", "GetOptimalWidth" };
		HarmonyMethod[] array3 = (HarmonyMethod[])(object)new HarmonyMethod[2]
		{
			new HarmonyMethod(typeof(PawnColumnWorkers_Resize), "MinWidth", (Type[])null),
			new HarmonyMethod(typeof(PawnColumnWorkers_Resize), "OptWidth", (Type[])null)
		};
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				MethodBase method = array[i].GetMethod(array2[j], AccessTools.all);
				HarmonyBase.instance.Patch(method, (HarmonyMethod)null, (HarmonyMethod)null, array3[j], (HarmonyMethod)null);
			}
		}
	}

	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> MinWidth(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_R4 && (instruction.operand as float?).HasValue && (instruction.operand as float?).Value.Equals(orgMinWidth))
			{
				instruction.operand = 158f;
			}
			yield return instruction;
		}
	}

	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> OptWidth(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_R4 && (instruction.operand as float?).HasValue && (instruction.operand as float?).Value.Equals(orgOptimalWidth))
			{
				instruction.operand = 188f;
			}
			yield return instruction;
		}
	}
}
