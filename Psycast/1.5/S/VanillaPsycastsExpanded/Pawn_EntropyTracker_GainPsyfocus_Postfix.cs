using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "GainPsyfocus")]
public static class Pawn_EntropyTracker_GainPsyfocus_Postfix
{
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		MethodInfo info = AccessTools.Method(typeof(MeditationUtility), "PsyfocusGainPerTick", (Type[])null, (Type[])null);
		int num = list.FindIndex((CodeInstruction ins) => CodeInstructionExtensions.Calls(ins, info));
		LocalBuilder localBuilder = generator.DeclareLocal(typeof(float));
		list.InsertRange(num + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
		{
			new CodeInstruction(OpCodes.Stloc, (object)localBuilder),
			new CodeInstruction(OpCodes.Ldloc, (object)localBuilder)
		});
		int index = list.FindIndex((CodeInstruction ins) => ins.opcode == OpCodes.Ret);
		List<Label> list2 = CodeInstructionExtensions.ExtractLabels(list[index]);
		list.InsertRange(index, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[3]
		{
			CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Ldarg_0, (object)null), (IEnumerable<Label>)list2),
			new CodeInstruction(OpCodes.Ldloc, (object)localBuilder),
			CodeInstruction.Call(typeof(Pawn_EntropyTracker_GainPsyfocus_Postfix), "GainXpFromPsyfocus", (Type[])null, (Type[])null)
		});
		return list;
	}

	public static void GainXpFromPsyfocus(this Pawn_PsychicEntropyTracker __instance, float gain)
	{
		__instance.Pawn?.Psycasts()?.GainExperience(gain * 100f * PsycastsMod.Settings.XPPerPercent);
	}
}
