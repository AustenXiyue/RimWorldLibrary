using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(Building_MechCharger))]
[HarmonyPatch("IsCompatibleWithCharger")]
[HarmonyPatch(new Type[]
{
	typeof(ThingDef),
	typeof(ThingDef)
})]
public static class Milian_MechReCharger_Patch
{
	[HarmonyPostfix]
	public static void Postfix(ThingDef chargerDef, ThingDef mechRace, ref bool __result)
	{
		if (mechRace.race.body.defName == "Milian_Body")
		{
			if (chargerDef == MiliraDefOf.Milian_Recharger)
			{
				__result = true;
			}
			else
			{
				__result = false;
			}
		}
		else if (chargerDef == MiliraDefOf.Milian_Recharger)
		{
			__result = false;
		}
	}
}
