using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompShield), "CompTick")]
internal static class CompShield_DisableOnOperateTurret
{
	private const int SHORT_SHIELD_RECHARGE_TIME = 120;

	internal static void Postfix(CompShield __instance, ref int ___ticksToReset)
	{
		if (Controller.settings.TurretsBreakShields && __instance.PawnOwner?.CurJobDef == JobDefOf.ManTurret && __instance.PawnOwner?.jobs?.curDriver?.OnLastToil == true)
		{
			if (__instance.ShieldState == ShieldState.Active)
			{
				__instance.Break();
				___ticksToReset = 120;
			}
			if (___ticksToReset < 120)
			{
				___ticksToReset = 120;
			}
		}
	}
}
