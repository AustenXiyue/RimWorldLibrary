using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
public static class GlobalControls_TemperatureString_Patch
{
	[HarmonyPriority(int.MinValue)]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
	{
		List<CodeInstruction> codes = codeInstructions.ToList();
		for (int i = 0; i < codes.Count; i++)
		{
			CodeInstruction code = codes[i];
			yield return code;
			int num;
			if (code.opcode == OpCodes.Stloc_S)
			{
				object operand = code.operand;
				if (operand is LocalBuilder lb)
				{
					num = ((lb.LocalIndex == 4) ? 1 : 0);
					goto IL_00e2;
				}
			}
			num = 0;
			goto IL_00e2;
			IL_00e2:
			if (num != 0)
			{
				yield return new CodeInstruction(OpCodes.Ldloca_S, (object)4);
				yield return new CodeInstruction(OpCodes.Ldloc_1, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.PropertyGetter(typeof(Find), "CurrentMap"));
				yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GlobalControls_TemperatureString_Patch), "ModifyTemperatureIfNeeded", (Type[])null, (Type[])null));
			}
		}
	}

	public static void ModifyTemperatureIfNeeded(ref float result, IntVec3 cell, Map map)
	{
		if (GenTemperature_GetTemperatureForCell_Patch.cachedComp?.map != map)
		{
			GenTemperature_GetTemperatureForCell_Patch.cachedComp = map.GetComponent<MapComponent_PsycastsManager>();
		}
		if (GenTemperature_GetTemperatureForCell_Patch.cachedComp.TryGetOverridenTemperatureFor(cell, out var result2))
		{
			result = result2;
		}
	}
}
