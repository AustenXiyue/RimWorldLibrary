using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

public static class HarmonyBase
{
	private static Harmony harmony = null;

	private static readonly OpCode[] branchOps = new OpCode[26]
	{
		OpCodes.Br,
		OpCodes.Br_S,
		OpCodes.Brfalse,
		OpCodes.Brfalse_S,
		OpCodes.Brtrue,
		OpCodes.Brtrue_S,
		OpCodes.Bge,
		OpCodes.Bge_S,
		OpCodes.Bge_Un,
		OpCodes.Bge_Un_S,
		OpCodes.Bgt,
		OpCodes.Bgt_S,
		OpCodes.Bgt_Un,
		OpCodes.Bgt_Un_S,
		OpCodes.Ble,
		OpCodes.Ble_S,
		OpCodes.Ble_Un,
		OpCodes.Ble_Un_S,
		OpCodes.Blt,
		OpCodes.Blt_S,
		OpCodes.Blt_Un,
		OpCodes.Blt_Un_S,
		OpCodes.Beq,
		OpCodes.Beq_S,
		OpCodes.Bne_Un,
		OpCodes.Bne_Un_S
	};

	internal static Harmony instance
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (harmony == null)
			{
				harmony = new Harmony("CombatExtended.HarmonyCE");
			}
			return harmony;
		}
	}

	public static void InitPatches()
	{
		instance.PatchAll(Assembly.GetExecutingAssembly());
		PatchThingOwner();
		PatchHediffWithComps(instance);
		Harmony_GenRadial_RadialPatternCount.Patch();
		PawnColumnWorkers_Resize.Patch();
		PawnColumnWorkers_SwapButtons.Patch();
	}

	private static void PatchThingOwner()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		Type typeFromHandle = typeof(ThingOwner);
		MethodInfo method = typeof(Harmony_ThingOwner_NotifyAdded_Patch).GetMethod("Postfix");
		instance.Patch((MethodBase)typeFromHandle.GetMethod("NotifyAdded", BindingFlags.Instance | BindingFlags.NonPublic), (HarmonyMethod)null, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null);
		MethodInfo method2 = typeof(Harmony_ThingOwner_NotifyAddedAndMergedWith_Patch).GetMethod("Postfix");
		instance.Patch((MethodBase)typeFromHandle.GetMethod("NotifyAddedAndMergedWith", BindingFlags.Instance | BindingFlags.NonPublic), (HarmonyMethod)null, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null);
		MethodInfo method3 = typeof(Harmony_ThingOwner_Take_Patch).GetMethod("Postfix");
		instance.Patch((MethodBase)typeFromHandle.GetMethod("Take", new Type[2]
		{
			typeof(Thing),
			typeof(int)
		}), (HarmonyMethod)null, new HarmonyMethod(method3), (HarmonyMethod)null, (HarmonyMethod)null);
		MethodInfo method4 = typeof(Harmony_ThingOwner_NotifyRemoved_Patch).GetMethod("Postfix");
		instance.Patch((MethodBase)typeFromHandle.GetMethod("NotifyRemoved", BindingFlags.Instance | BindingFlags.NonPublic), (HarmonyMethod)null, new HarmonyMethod(method4), (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static void PatchHediffWithComps(Harmony harmonyInstance)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		MethodInfo method = typeof(Harmony_HediffWithComps_BleedRate_Patch).GetMethod("Postfix");
		Type typeFromHandle = typeof(HediffWithComps);
		List<Type> list = typeFromHandle.AllSubclassesNonAbstract();
		foreach (Type item in list)
		{
			MethodInfo getMethod = item.GetProperty("BleedRate").GetGetMethod();
			if (getMethod.IsVirtual && getMethod.DeclaringType.Equals(item))
			{
				harmonyInstance.Patch((MethodBase)getMethod, (HarmonyMethod)null, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	internal static LocalBuilder[] GetLocals(ILGenerator il)
	{
		return Traverse.Create((object)il).Field("locals").GetValue<LocalBuilder[]>();
	}

	internal static bool isBranch(CodeInstruction instruction)
	{
		if (branchOps.Contains(instruction.opcode))
		{
			return true;
		}
		return false;
	}

	internal static bool doCast(bool? input)
	{
		if (!input.HasValue)
		{
			return false;
		}
		return input.Value;
	}

	internal static CodeInstruction MakeLocalLoadInstruction(int index, LocalVariableInfo info = null)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		if (index < 0 || index > 65535)
		{
			throw new ArgumentException("Index must be greater than 0 and less than " + uint.MaxValue + ".");
		}
		switch (index)
		{
		case 0:
			return new CodeInstruction(OpCodes.Ldloc_0, (object)null);
		case 1:
			return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
		case 2:
			return new CodeInstruction(OpCodes.Ldloc_2, (object)null);
		case 3:
			return new CodeInstruction(OpCodes.Ldloc_3, (object)null);
		default:
		{
			object obj = ((info == null || info.LocalIndex != index) ? ((object)index) : info);
			if (index > 255)
			{
				return new CodeInstruction(OpCodes.Ldloc, obj);
			}
			return new CodeInstruction(OpCodes.Ldloc_S, obj);
		}
		}
	}

	internal static int OpcodeStoreIndex(CodeInstruction instruction)
	{
		if (instruction.opcode == OpCodes.Stloc_0)
		{
			return 0;
		}
		if (instruction.opcode == OpCodes.Stloc_1)
		{
			return 1;
		}
		if (instruction.opcode == OpCodes.Stloc_2)
		{
			return 2;
		}
		if (instruction.opcode == OpCodes.Stloc_3)
		{
			return 3;
		}
		if (instruction.opcode == OpCodes.Stloc_S)
		{
			return (instruction.operand as LocalVariableInfo).LocalIndex;
		}
		if (instruction.opcode == OpCodes.Stloc)
		{
			return (instruction.operand as LocalVariableInfo).LocalIndex;
		}
		return -1;
	}

	internal static int OpcodeLoadIndex(CodeInstruction instruction)
	{
		if (instruction.opcode == OpCodes.Ldloc_0)
		{
			return 0;
		}
		if (instruction.opcode == OpCodes.Ldloc_1)
		{
			return 1;
		}
		if (instruction.opcode == OpCodes.Ldloc_2)
		{
			return 2;
		}
		if (instruction.opcode == OpCodes.Ldloc_3)
		{
			return 3;
		}
		if (instruction.opcode == OpCodes.Ldloc_S)
		{
			return (instruction.operand as LocalVariableInfo).LocalIndex;
		}
		if (instruction.opcode == OpCodes.Ldloc)
		{
			return (instruction.operand as LocalVariableInfo).LocalIndex;
		}
		return -1;
	}
}
