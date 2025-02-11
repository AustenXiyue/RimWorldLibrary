using RimWorld;
using Verse;

namespace AncotLibrary;

public class RoyalUtility
{
	public static bool TitleSenioritySatisfied(Pawn pawn, Faction faction, int seniorityRequired)
	{
		if (pawn == null)
		{
			return false;
		}
		if (pawn.Dead || !pawn.Faction.IsPlayer)
		{
			return false;
		}
		if (pawn.royalty != null && pawn.royalty.HasAnyTitleIn(faction) && pawn.royalty.GetCurrentTitle(faction).seniority >= seniorityRequired)
		{
			return true;
		}
		return false;
	}
}
