using RimWorld;
using Verse;

namespace CombatExtended.Lasers;

public static class ThingExtensions
{
	public static bool IsShielded(this Thing thing)
	{
		return (thing as Pawn)?.IsShielded() ?? false;
	}

	public static bool IsShielded(this Pawn pawn)
	{
		if (pawn == null || pawn.apparel == null)
		{
			return false;
		}
		DamageInfo dinfo = new DamageInfo(DamageDefOf.Bomb, 0f);
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (item.CheckPreAbsorbDamage(dinfo))
			{
				return true;
			}
		}
		return false;
	}
}
