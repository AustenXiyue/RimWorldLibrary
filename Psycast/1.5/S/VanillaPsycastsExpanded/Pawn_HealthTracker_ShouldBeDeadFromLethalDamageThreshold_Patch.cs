using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
public static class Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold_Patch
{
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> codes = instructions.ToList();
		Label label = generator.DefineLabel();
		for (int i = 0; i < codes.Count; i++)
		{
			yield return codes[i];
			if (codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].opcode == OpCodes.Isinst && CodeInstructionExtensions.OperandIs(codes[i - 1], (MemberInfo)typeof(Hediff_Injury)))
			{
				codes[i + 1].labels.Add(label);
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(Pawn_HealthTracker), "hediffSet"));
				yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(HediffSet), "hediffs"));
				yield return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(typeof(List<Hediff>), "get_Item", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold_Patch), "IsNotRegeneratingHediff", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
			}
		}
	}

	public static bool IsNotRegeneratingHediff(Hediff hediff)
	{
		return hediff.def != VPE_DefOf.VPE_Regenerating;
	}
}
