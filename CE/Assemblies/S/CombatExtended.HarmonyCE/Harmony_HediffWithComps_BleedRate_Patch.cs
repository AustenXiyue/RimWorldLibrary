using Verse;

namespace CombatExtended.HarmonyCE;

internal static class Harmony_HediffWithComps_BleedRate_Patch
{
	public static void Postfix(Hediff __instance, ref float __result)
	{
		if (__result > 0f)
		{
			HediffComp_Stabilize hediffComp_Stabilize = (__instance as HediffWithComps)?.TryGetComp<HediffComp_Stabilize>() ?? null;
			if (hediffComp_Stabilize != null)
			{
				__result *= hediffComp_Stabilize.BleedModifier;
			}
		}
	}
}
