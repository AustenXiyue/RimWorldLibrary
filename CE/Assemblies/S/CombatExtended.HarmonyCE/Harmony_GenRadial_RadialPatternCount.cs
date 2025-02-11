using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

internal static class Harmony_GenRadial_RadialPatternCount
{
	private static readonly string logPrefix = "Combat Extended :: " + typeof(Harmony_GenRadial_RadialPatternCount).Name + " :: ";

	private const sbyte newRadialRange = sbyte.MaxValue;

	private const double keepPatternRange = 0.6944444179534912;

	private static readonly int newRadialPatternCount = Convert.ToInt32(Math.Pow(254.0, 2.0) * 0.6944444179534912);

	private static int defaultValue;

	internal static void Patch()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		IEnumerable<MethodInfo> enumerable = from m in typeof(GenRadial).GetMethods(AccessTools.all)
			where m.DeclaringType == typeof(GenRadial)
			select m;
		ConstructorInfo constructorInfo = typeof(GenRadial).GetConstructors(AccessTools.all).FirstOrDefault();
		MethodInfo methodInfo = null;
		defaultValue = 10000;
		HarmonyBase.instance.Patch((MethodBase)constructorInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(Harmony_GenRadial_RadialPatternCount), "Transpiler_RadialPatternCount", (Type[])null), (HarmonyMethod)null);
		foreach (MethodInfo item in enumerable)
		{
			HarmonyBase.instance.Patch((MethodBase)item, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(Harmony_GenRadial_RadialPatternCount), "Transpiler_RadialPatternCount", (Type[])null), (HarmonyMethod)null);
			if (item.Name == "SetupRadialPattern")
			{
				methodInfo = item;
				HarmonyBase.instance.Patch((MethodBase)item, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(Harmony_GenRadial_RadialPatternCount), "Transpiler_Range", (Type[])null), (HarmonyMethod)null);
			}
		}
		Traverse.Create(typeof(GenRadial)).Field("RadialPattern").SetValue((object)new IntVec3[newRadialPatternCount]);
		Traverse.Create(typeof(GenRadial)).Field("RadialPatternRadii").SetValue((object)new float[newRadialPatternCount]);
		methodInfo.Invoke(null, null);
		Log.Message(logPrefix + "Info: Post GenRadial patch maximum radius: " + GenRadial.MaxRadialPatternRadius);
	}

	private static IEnumerable<CodeInstruction> Transpiler_RadialPatternCount(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == defaultValue)
			{
				instruction.operand = newRadialPatternCount;
			}
		}
		return instructions;
	}

	private static IEnumerable<CodeInstruction> Transpiler_Range(IEnumerable<CodeInstruction> instructions)
	{
		int num = 0;
		int num2 = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (num2 < 2)
			{
				if (num < 2 && instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == -60)
				{
					instruction.operand = (sbyte)(-127);
					num++;
				}
				if (num >= 2 && num2 < 2 && instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 60)
				{
					instruction.operand = sbyte.MaxValue;
					num2++;
				}
			}
		}
		return instructions;
	}
}
