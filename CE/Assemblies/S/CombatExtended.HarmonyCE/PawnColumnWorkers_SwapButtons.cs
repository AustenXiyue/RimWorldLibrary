using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

internal static class PawnColumnWorkers_SwapButtons
{
	private const string apparelString = "CE_Outfits";

	private const string drugString = "CE_Drugs";

	public static void Patch()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		MethodBase[] array = new MethodBase[3]
		{
			typeof(PawnColumnWorker_Outfit).GetMethod("DoCell", AccessTools.all),
			typeof(DrugPolicyUIUtility).GetMethod("DoAssignDrugPolicyButtons", AccessTools.all),
			typeof(PawnColumnWorker_FoodRestriction).GetMethod("DoAssignFoodRestrictionButtons", AccessTools.all)
		};
		HarmonyMethod val = new HarmonyMethod(AccessTools.Method(typeof(PawnColumnWorkers_SwapButtons), "Transpiler", (Type[])null, (Type[])null));
		MethodBase[] array2 = array;
		foreach (MethodBase methodBase in array2)
		{
		}
	}

	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		bool flag = false;
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			CodeInstruction val = list[i];
			if (num2 >= 0 && num3 < 0 && val.opcode == OpCodes.Ldarga_S)
			{
				num3 = i - 1;
			}
			if (num >= 0 && num2 < 0 && val.opcode == OpCodes.Ldarga_S)
			{
				num2 = i;
			}
			if (val.opcode == OpCodes.Call && val.operand.Equals(typeof(Mathf).GetMethod("FloorToInt", AccessTools.all)) && num < 0)
			{
				num = i - 2;
			}
			if (val.opcode == OpCodes.Call && val.operand == typeof(Rect).GetMethod("get_y", AccessTools.all))
			{
				if (flag && i + 1 < list.Count && list[i + 1].opcode == OpCodes.Ldc_R4 && list[i + 1].operand.Equals(2f))
				{
					list2.Add(i);
				}
				else
				{
					flag = true;
				}
			}
			object operand = val.operand;
			if (operand != null && operand.Equals("ClearForcedApparel"))
			{
				list4.Add(i);
			}
			object operand2 = val.operand;
			if (operand2 != null && operand2.Equals("AssignTabEdit"))
			{
				list3.Add(i);
			}
		}
		if (list[num].operand.Equals(0.71428573f))
		{
			list[num].operand = PawnColumnWorker_Loadout.IconSize;
			list[num + 1].opcode = OpCodes.Sub;
		}
		list[num2].opcode = OpCodes.Ldc_R4;
		list[num2].operand = PawnColumnWorker_Loadout.IconSize;
		foreach (CodeInstruction item in list.GetRange(num2 + 1, num3 - num2 - 2))
		{
			item.operand = null;
			item.opcode = OpCodes.Nop;
		}
		foreach (int item2 in list3)
		{
			CodeInstruction val2 = list[item2];
			val2.opcode = OpCodes.Call;
			val2.operand = typeof(PawnColumnWorker_Loadout).GetProperty("EditImage", AccessTools.all).GetGetMethod();
			foreach (CodeInstruction item3 in list.GetRange(item2 + 1, 4))
			{
				item3.operand = null;
				item3.opcode = OpCodes.Nop;
			}
			list[item2 + 6].operand = typeof(Widgets).GetMethod("ButtonImage", new Type[3]
			{
				typeof(Rect),
				typeof(Texture2D),
				typeof(bool)
			});
		}
		foreach (int item4 in list4)
		{
			CodeInstruction val3 = list[item4];
			val3.opcode = OpCodes.Call;
			val3.operand = typeof(PawnColumnWorker_Loadout).GetProperty("ClearImage", AccessTools.all).GetGetMethod();
			foreach (CodeInstruction item5 in list.GetRange(item4 + 1, 4))
			{
				item5.operand = null;
				item5.opcode = OpCodes.Nop;
			}
			list[item4 + 6].operand = typeof(Widgets).GetMethod("ButtonImage", new Type[3]
			{
				typeof(Rect),
				typeof(Texture2D),
				typeof(bool)
			});
		}
		foreach (int item6 in list2)
		{
			CodeInstruction val4 = list[item6];
			val4.opcode = OpCodes.Callvirt;
			val4.operand = typeof(PawnColumnWorkers_SwapButtons).GetMethod("RectHeightHelper", AccessTools.all);
			for (int j = 1; j <= 2; j++)
			{
				CodeInstruction val5 = list[item6 + j];
				val5.operand = null;
				val5.opcode = OpCodes.Nop;
			}
			val4 = list[item6 + 5];
			val4.operand = list[item6 + 3].operand;
			val4.opcode = list[item6 + 3].opcode;
			val4 = list[item6 + 6];
			val4.operand = list[item6 + 4].operand;
			val4.opcode = list[item6 + 4].opcode;
			for (int k = 7; k <= 8; k++)
			{
				CodeInstruction val6 = list[item6 + k];
				val6.operand = null;
				val6.opcode = OpCodes.Nop;
			}
		}
		return list;
	}

	public static float RectHeightHelper(ref Rect rect)
	{
		return rect.y + (rect.height - PawnColumnWorker_Loadout.IconSize) / 2f;
	}

	public static void DoToolTip(Rect area, Pawn pawn, string untranslated)
	{
		TooltipHandler.TipRegion(area, new TipSignal(PawnColumnWorker_Loadout.textGetter(untranslated), pawn.GetHashCode() * 613));
	}
}
