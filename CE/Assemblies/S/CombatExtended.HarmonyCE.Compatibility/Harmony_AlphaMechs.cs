using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE.Compatibility;

public static class Harmony_AlphaMechs
{
	[HarmonyPatch]
	public static class Harmony_CompSwapWeapons_Apply
	{
		public static bool Prepare()
		{
			return CompSwapWeapons_Patch_HarmonyPatches != null;
		}

		public static MethodBase TargetMethod()
		{
			return AccessTools.Method("AlphaMechs.CompSwapWeapons:Apply", new Type[2]
			{
				typeof(LocalTargetInfo),
				typeof(LocalTargetInfo)
			}, (Type[])null);
		}

		public static void Prefix(CompAbilityEffect __instance)
		{
			__instance?.parent?.pawn?.equipment.Primary?.TryGetComp<CompAmmoUser>()?.TryUnload();
		}
	}

	private static Type CompSwapWeapons_Patch_HarmonyPatches => AccessTools.TypeByName("AlphaMechs.CompSwapWeapons");
}
