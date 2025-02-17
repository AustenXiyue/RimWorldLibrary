using RimWorld;

namespace Verse;

public static class WildManUtility
{
	public static bool IsWildMan(this Pawn p)
	{
		if (p.kindDef == PawnKindDefOf.WildMan)
		{
			return !p.IsMutant;
		}
		return false;
	}

	public static bool AnimalOrWildMan(this Pawn p)
	{
		if (!p.IsNonMutantAnimal)
		{
			return p.IsWildMan();
		}
		return true;
	}

	public static bool NonHumanlikeOrWildMan(this Pawn p)
	{
		if (p.RaceProps.Humanlike)
		{
			return p.IsWildMan();
		}
		return true;
	}

	public static bool WildManShouldReachOutsideNow(Pawn p)
	{
		if (p.IsWildMan() && !p.mindState.WildManEverReachedOutside)
		{
			if (p.IsPrisoner)
			{
				return p.guest.Released;
			}
			return true;
		}
		return false;
	}
}
