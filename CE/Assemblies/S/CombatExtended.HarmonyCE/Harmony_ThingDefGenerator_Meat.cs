using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
public static class Harmony_ThingDefGenerator_Meat
{
	private static MethodBase TargetMethod()
	{
		Type type = AccessTools.Inner(typeof(ThingDefGenerator_Meat), "<ImpliedMeatDefs>d__0");
		return AccessTools.Method(type, "MoveNext", (Type[])null, (Type[])null);
	}

	public static void Postfix(IEnumerator<ThingDef> __instance, bool __result)
	{
		if (__result)
		{
			__instance.Current.SetStatBaseValue(CE_StatDefOf.Bulk, 0.2f);
		}
	}
}
