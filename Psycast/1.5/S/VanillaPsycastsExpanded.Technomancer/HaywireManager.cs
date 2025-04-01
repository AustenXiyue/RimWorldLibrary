using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaPsycastsExpanded.Technomancer;

[HarmonyPatch]
[StaticConstructorOnStartup]
public class HaywireManager
{
	[HarmonyPatch]
	public static class OverrideBestAttackTargetValidator
	{
		[HarmonyTargetMethod]
		public static MethodInfo TargetMethod()
		{
			return AccessTools.Method(AccessTools.Inner(typeof(AttackTargetFinder), "<>c__DisplayClass5_0"), "<BestAttackTarget>b__1", (Type[])null, (Type[])null);
		}

		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Expected O, but got Unknown
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Expected O, but got Unknown
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Expected O, but got Unknown
			List<CodeInstruction> list = instructions.ToList();
			MethodInfo info = AccessTools.Method(typeof(GenHostility), "HostileTo", new Type[2]
			{
				typeof(Thing),
				typeof(Thing)
			}, (Type[])null);
			int startIndex = list.FindIndex((CodeInstruction ins) => CodeInstructionExtensions.Calls(ins, info));
			int num = list.FindLastIndex(startIndex, (CodeInstruction ins) => ins.opcode == OpCodes.Ldarg_0);
			FieldInfo fieldInfo = (FieldInfo)list[num + 1].operand;
			int index = list.FindIndex(startIndex, (CodeInstruction ins) => ins.opcode == OpCodes.Ldc_I4_0);
			list.RemoveAt(index);
			list.InsertRange(index, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[3]
			{
				new CodeInstruction(OpCodes.Ldarg_0, (object)null),
				new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo),
				new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HaywireManager), "ShouldTargetAllies", (Type[])null, (Type[])null))
			});
			return list;
		}
	}

	public static readonly HashSet<Thing> HaywireThings;

	static HaywireManager()
	{
		HaywireThings = new HashSet<Thing>();
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (typeof(Building_Turret).IsAssignableFrom(allDef.thingClass))
			{
				allDef.comps.Add(new CompProperties(typeof(CompHaywire)));
			}
		}
	}

	public static bool ShouldTargetAllies(Thing t)
	{
		return HaywireThings.Contains(t);
	}

	[HarmonyPatch(typeof(AttackTargetsCache), "GetPotentialTargetsFor")]
	[HarmonyPostfix]
	public static void ChangeTargets(IAttackTargetSearcher th, ref List<IAttackTarget> __result, AttackTargetsCache __instance)
	{
		if (th is Thing item && HaywireThings.Contains(item))
		{
			__result.Clear();
			__result.AddRange(__instance.TargetsHostileToColony);
		}
	}

	[HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		FieldInfo info = AccessTools.Field(typeof(Building_TurretGun), "mannableComp");
		int num = list.FindIndex((CodeInstruction ins) => CodeInstructionExtensions.LoadsField(ins, info, false));
		Label label = (Label)list[num + 1].operand;
		int num2 = list.FindLastIndex(num, (CodeInstruction ins) => ins.opcode == OpCodes.Ldarg_0);
		list.InsertRange(num2 + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[3]
		{
			new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(HaywireManager), "ShouldTargetAllies", (Type[])null, (Type[])null)),
			new CodeInstruction(OpCodes.Brtrue, (object)label),
			new CodeInstruction(OpCodes.Ldarg_0, (object)null)
		});
		return list;
	}
}
