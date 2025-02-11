using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Fire), "TrySpread")]
internal static class Harmony_Fire_TrySpread
{
	private static SimpleCurve _angleCurveWide;

	private static SimpleCurve _angleCurveNarrow;

	private static float GetWindMult(Fire fire)
	{
		WeatherTracker component = fire.Map.GetComponent<WeatherTracker>();
		float b = Mathf.Sqrt(component.GetWindStrengthAt(fire.Position)) * FireSpread.values.windSpeedMultiplier;
		return FireSpread.values.spreadFarBaseChance * Mathf.Max(1f, b);
	}

	private static IntVec3 GetRandWindShift(Fire fire, bool spreadFar)
	{
		if (_angleCurveWide == null)
		{
			_angleCurveWide = new SimpleCurve
			{
				{ 0f, 360f },
				{ 3f, 210f },
				{ 6f, 90f },
				{ 9f, 30f },
				{ 999f, 1f }
			};
		}
		if (_angleCurveNarrow == null)
		{
			_angleCurveNarrow = new SimpleCurve
			{
				{ 0f, 360f },
				{ 3f, 120f },
				{ 6f, 30f },
				{ 9f, 10f },
				{ 999f, 1f }
			};
		}
		WeatherTracker component = fire.Map.GetComponent<WeatherTracker>();
		float num = (spreadFar ? _angleCurveNarrow.Evaluate(component.GetWindStrengthAt(fire.Position) * FireSpread.values.windSpeedMultiplier) : _angleCurveWide.Evaluate(component.GetWindStrengthAt(fire.Position) * FireSpread.values.windSpeedMultiplier));
		num *= 0.5f;
		float angle = Rand.Range(0f - num, num);
		Vector3 vect = component.WindDirection.RotatedBy(angle);
		if (spreadFar)
		{
			vect *= Rand.Range(1f, Mathf.Max(2f, component.GetWindStrengthAt(fire.Position) * FireSpread.values.windSpeedMultiplier));
		}
		return vect.ToIntVec3();
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		bool flag = false;
		bool flag2 = false;
		List<CodeInstruction> list = new List<CodeInstruction>();
		foreach (CodeInstruction instruction in instructions)
		{
			if (flag)
			{
				flag = instruction.opcode != OpCodes.Ldelem;
			}
			else if (instruction.operand == AccessTools.Field(typeof(GenRadial), "ManualRadialPattern"))
			{
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(flag2 ? new CodeInstruction(OpCodes.Ldc_I4_1, (object)null) : new CodeInstruction(OpCodes.Ldc_I4_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(typeof(Harmony_Fire_TrySpread), "GetRandWindShift", (Type[])null, (Type[])null)));
				flag2 = true;
				flag = true;
			}
			else if (instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float num && num == 0.8f)
			{
				instruction.operand = 1f;
				list.Add(instruction);
				list.Add(new CodeInstruction(OpCodes.Ldarg_0, (object)null));
				list.Add(new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_Fire_TrySpread), "GetWindMult", (Type[])null, (Type[])null)));
				list.Add(new CodeInstruction(OpCodes.Sub, (object)null));
			}
			else
			{
				list.Add(instruction);
			}
		}
		return list;
	}
}
