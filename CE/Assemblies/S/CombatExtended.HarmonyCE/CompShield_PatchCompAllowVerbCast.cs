using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompShield), "CompAllowVerbCast")]
internal static class CompShield_PatchCompAllowVerbCast
{
	internal static bool Prefix(ref bool __result, Verb verb, CompShield __instance)
	{
		if (__instance.Props.blocksRangedWeapons)
		{
			__result = __instance.ShieldState != 0 || verb is Verb_MarkForArtillery || (!(verb is Verb_LaunchProjectileCE) && !(verb is Verb_LaunchProjectile));
		}
		else
		{
			__result = true;
		}
		return false;
	}
}
