using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
internal static class Harmony_Pawn_HealthTracker_CheckForStateChange
{
	private static MethodBase mGet_IsMechanoid = AccessTools.Method(typeof(RaceProperties), "get_IsMechanoid", (Type[])null, (Type[])null);

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		Label label = generator.DefineLabel();
		bool flag = false;
		List<CodeInstruction> list = instructions.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(list[i], (MemberInfo)mGet_IsMechanoid))
			{
				flag = true;
			}
			else if (flag && list[i].opcode == OpCodes.Br_S)
			{
				list[i] = new CodeInstruction(OpCodes.Br_S, (object)label);
				flag = false;
			}
			else if (list[i].opcode == OpCodes.Ldsfld && list[i].operand == AccessTools.Field(typeof(DebugViewSettings), "logCauseOfDeath"))
			{
				list[i].labels = new List<Label> { label };
			}
		}
		return Transpilers.MethodReplacer((IEnumerable<CodeInstruction>)list, (MethodBase)AccessTools.Method(typeof(Rand), "Chance", (Type[])null, (Type[])null), (MethodBase)AccessTools.Method(typeof(Harmony_Pawn_HealthTracker_CheckForStateChange), "NoChance", (Type[])null, (Type[])null));
	}

	private static bool NoChance(float unusedChance)
	{
		return false;
	}
}
