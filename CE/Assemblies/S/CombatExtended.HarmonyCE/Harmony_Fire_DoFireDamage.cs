using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "DoFireDamage")]
internal static class Harmony_Fire_DoFireDamage
{
	private static void ApplySizeMult(Pawn pawn, ref float damage)
	{
		damage *= pawn.BodySize;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction code in instructions)
		{
			if (code.opcode == OpCodes.Ldc_R4 && code.operand is float && (float)code.operand == 150f)
			{
				code.operand = 300f;
			}
			if (code.operand == AccessTools.Field(typeof(RulePackDefOf), "DamageEvent_Fire"))
			{
				yield return new CodeInstruction(OpCodes.Ldloca, (object)0);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Fire_DoFireDamage), "ApplySizeMult", (Type[])null, (Type[])null));
				yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
			}
			yield return code;
		}
	}
}
