using System;
using HarmonyLib;
using RimWorld.QuestGen;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(QuestNodeEqualUtility), "Equal")]
public static class Harmony_QuestNodeEqualUtility_PreventThingDefToAmmoDefCast
{
	public static bool Prefix(object value1, object value2, Type compareAs, ref bool __result)
	{
		if (value1 is AmmoDef && value2 is string defName)
		{
			AmmoDef named = DefDatabase<AmmoDef>.GetNamed(defName, errorOnFail: false);
			if (named == null)
			{
				__result = false;
				return false;
			}
		}
		return true;
	}
}
