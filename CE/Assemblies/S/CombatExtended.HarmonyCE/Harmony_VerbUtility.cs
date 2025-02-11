using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(VerbUtility), "GetProjectile")]
internal static class Harmony_VerbUtility
{
	internal static bool Prefix(Verb verb, ref ThingDef __result)
	{
		if (verb is Verb_LaunchProjectileCE verb_LaunchProjectileCE)
		{
			__result = verb_LaunchProjectileCE.Projectile;
			return false;
		}
		return true;
	}
}
