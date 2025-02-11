using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Explosion), "GetArmorPenetrationAt")]
public class Harmony_ExplosionCE_GetArmorPenetrationAt
{
	internal static bool Prefix(Explosion __instance, ref float __result, IntVec3 c)
	{
		if (__instance is ExplosionCE explosionCE)
		{
			__result = explosionCE.GetArmorPenetrationAtCE(c);
			return false;
		}
		return true;
	}
}
