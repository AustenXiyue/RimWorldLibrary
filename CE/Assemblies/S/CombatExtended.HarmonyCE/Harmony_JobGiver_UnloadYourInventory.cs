using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(JobGiver_UnloadYourInventory), "TryGiveJob", new Type[] { typeof(Pawn) })]
internal static class Harmony_JobGiver_UnloadYourInventory
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
	{
		ParameterInfo[] args = source.GetParameters();
		int argIndex = -1;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].ParameterType.Equals(typeof(Pawn)))
			{
				argIndex = i + 1;
				break;
			}
		}
		Label branchFalse = il.DefineLabel();
		int patchPhase = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (patchPhase == 1 && instruction.opcode.Equals(OpCodes.Ldnull))
			{
				instruction.labels.Add(branchFalse);
				patchPhase = 2;
			}
			if (patchPhase == 0 && instruction.opcode.Equals(OpCodes.Brtrue_S))
			{
				yield return new CodeInstruction(OpCodes.Brfalse, (object)branchFalse);
				yield return new CodeInstruction(OpCodes.Ldarg, (object)argIndex);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Utility_HoldTracker), "HasAnythingForDrop", (Type[])null, (Type[])null));
				patchPhase = 1;
			}
			yield return instruction;
		}
		if (patchPhase < 2)
		{
			Log.Warning("CombatExtended :: Harmony-JobGiver_UnloadYourInventory patch failed to complete all its steps");
		}
	}
}
