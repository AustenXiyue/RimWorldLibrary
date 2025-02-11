using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo")]
internal static class Verb_LaunchProjectileCE_RerouteTryFindShootLineFromTo
{
	internal static bool Prefix(Verb __instance, ref bool __result, IntVec3 root, LocalTargetInfo targ, ref ShootLine resultingLine)
	{
		if (__instance is Verb_LaunchProjectileCE verb_LaunchProjectileCE)
		{
			__result = verb_LaunchProjectileCE.TryFindCEShootLineFromTo(root, targ, out resultingLine);
			return false;
		}
		return true;
	}
}
