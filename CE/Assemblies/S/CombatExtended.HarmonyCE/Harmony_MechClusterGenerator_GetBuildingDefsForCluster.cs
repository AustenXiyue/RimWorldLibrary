using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(MechClusterGenerator))]
[HarmonyPatch("GetBuildingDefsForCluster")]
[HarmonyPatch(new Type[]
{
	typeof(float),
	typeof(IntVec2),
	typeof(bool),
	typeof(float?),
	typeof(bool)
})]
public static class Harmony_MechClusterGenerator_GetBuildingDefsForCluster
{
	[HarmonyPostfix]
	public static void PostFix(float points, ref List<ThingDef> __result)
	{
		if (Controller.settings.EnableAmmoSystem && __result.Any((ThingDef x) => x.building.IsTurret && !x.building.IsMortar && x.building.turretGunDef != null && x.building.turretGunDef.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet != null))
		{
			if (points > 3000f)
			{
				__result.Add(CE_ThingDefOf.CombatExtended_MechAmmoBeacon);
			}
			if (points > 7000f)
			{
				__result.Add(CE_ThingDefOf.CombatExtended_MechAmmoBeacon);
			}
			__result.Add(CE_ThingDefOf.CombatExtended_MechAmmoBeacon);
		}
	}
}
