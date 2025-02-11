using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "DoComplexCalcs")]
internal static class Harmony_Fire_DoComplexCalcs
{
	private static float GetWindGrowthAdjust(Fire fire)
	{
		WeatherTracker component = fire.Map.GetComponent<WeatherTracker>();
		return FireSpread.values.baseGrowthPerTick * (1f + Mathf.Sqrt(component.GetWindStrengthAt(fire.Position)) * FireSpread.values.windSpeedMultiplier);
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction code in instructions)
		{
			if (code.opcode == OpCodes.Ldc_R4 && code.operand is float && (float)code.operand == 0.00055f)
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Fire_DoComplexCalcs), "GetWindGrowthAdjust", (Type[])null, (Type[])null));
			}
			else
			{
				yield return code;
			}
		}
	}

	internal static void Postfix(Fire __instance)
	{
		if (!__instance.Spawned || !(__instance.parent is Pawn pawn))
		{
			return;
		}
		__instance.fireSize -= pawn.GetStatValue(StatDefOf.Flammability) * 150f * 0.00055f;
		float num = 0f;
		List<Apparel> list = pawn.apparel?.WornApparel ?? null;
		if (list == null)
		{
			return;
		}
		IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside);
		float num2 = 0f;
		foreach (BodyPartRecord item in notMissingParts)
		{
			num2 += item.coverageAbs;
			float num3 = 1f;
			foreach (Apparel item2 in list)
			{
				if (item2.def.apparel.CoversBodyPart(item))
				{
					num3 = (item2.GetStatValue(StatDefOf.Flammability) - 0.1f) / 0.5f;
					break;
				}
			}
			num += item.coverageAbs * num3;
		}
		num /= num2;
		__instance.fireSize += 0.00055f * num * 1.5f;
		if (__instance.fireSize < 0.1f)
		{
			__instance.Destroy();
		}
	}
}
