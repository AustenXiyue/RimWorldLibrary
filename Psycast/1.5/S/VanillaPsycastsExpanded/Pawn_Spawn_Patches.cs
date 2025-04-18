using System.Linq;
using HarmonyLib;
using VanillaPsycastsExpanded.Nightstalker;
using VanillaPsycastsExpanded.Technomancer;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class Pawn_Spawn_Patches
{
	[HarmonyPatch(typeof(Pawn), "SpawnSetup")]
	[HarmonyPostfix]
	public static void PawnPostSpawned(Pawn __instance)
	{
		if (__instance.health.hediffSet.GetAllComps().OfType<HediffComp_Haywire>().Any())
		{
			HaywireManager.HaywireThings.Add(__instance);
		}
		if (__instance.health.hediffSet.hediffs.OfType<Hediff_Darkvision>().Any())
		{
			Hediff_Darkvision.DarkvisionPawns.Add(__instance);
		}
	}

	[HarmonyPatch(typeof(Pawn), "DeSpawn")]
	[HarmonyPostfix]
	public static void PawnPostDeSpawned(Pawn __instance)
	{
		if (HaywireManager.HaywireThings.Contains(__instance))
		{
			HaywireManager.HaywireThings.Remove(__instance);
		}
		if (Hediff_Darkvision.DarkvisionPawns.Contains(__instance))
		{
			Hediff_Darkvision.DarkvisionPawns.Remove(__instance);
		}
	}
}
