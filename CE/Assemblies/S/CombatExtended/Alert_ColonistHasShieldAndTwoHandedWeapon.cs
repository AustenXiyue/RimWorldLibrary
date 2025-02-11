using RimWorld;
using Verse;

namespace CombatExtended;

public class Alert_ColonistHasShieldAndTwoHandedWeapon : Alert
{
	public Alert_ColonistHasShieldAndTwoHandedWeapon()
	{
		defaultLabel = "CE_ColonistHasShieldAndTwoHandedWeapon".Translate();
		defaultExplanation = "CE_ColonistHasShieldAndTwoHandedWeaponDesc".Translate();
	}

	public override AlertReport GetReport()
	{
		foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
		{
			if (WorkGiver_HunterHuntCE.HasMeleeShieldAndTwoHandedWeapon(item))
			{
				return item;
			}
		}
		return false;
	}
}
