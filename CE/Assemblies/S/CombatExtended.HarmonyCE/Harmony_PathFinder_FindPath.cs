using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(PathFinder), "FindPath", new Type[]
{
	typeof(IntVec3),
	typeof(LocalTargetInfo),
	typeof(TraverseParms),
	typeof(PathEndMode),
	typeof(PathFinderCostTuning)
})]
internal static class Harmony_PathFinder_FindPath
{
	private static Map map;

	private static LightingTracker lightingTracker;

	private static DangerTracker dangerTracker;

	private static bool combaten;

	private static bool crouching;

	internal static bool Prefix(PathFinder __instance, ref PawnPath __result, IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms, PathEndMode peMode)
	{
		map = __instance.map;
		dangerTracker = __instance.map.GetDangerTracker();
		lightingTracker = __instance.map.GetLightingTracker();
		combaten = traverseParms.pawn?.jobs?.curJob?.def.alwaysShowWeapon == true;
		Pawn pawn = traverseParms.pawn;
		CompSuppressable compSuppressable = pawn?.TryGetComp<CompSuppressable>();
		if (compSuppressable == null || !compSuppressable.isSuppressed || compSuppressable.IsCrouchWalking || pawn.CurJob?.def == CE_JobDefOf.RunForCover || (start == dest.Cell && peMode == PathEndMode.OnCell))
		{
			crouching = compSuppressable?.IsCrouchWalking ?? false;
			return true;
		}
		__result = PawnPath.NotFound;
		return false;
	}
}
