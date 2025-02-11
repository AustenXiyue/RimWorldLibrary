using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompProjectileInterceptor), "PostPreApplyDamage")]
internal static class Harmony_CompProjectileInterceptor_PostPreApplyDamage
{
	public static bool Prefix(CompProjectileInterceptor __instance)
	{
		return __instance.Active;
	}
}
