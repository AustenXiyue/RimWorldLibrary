using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class WorkGiver_HunterHuntCE : WorkGiver_HunterHunt
{
	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return base.ShouldSkip(pawn, forced) || HasMeleeShieldAndTwoHandedWeapon(pawn);
	}

	public static bool HasMeleeShieldAndTwoHandedWeapon(Pawn p)
	{
		if (p.equipment.Primary != null && !(p.equipment.Primary.def.weaponTags?.Contains("CE_OneHandedWeapon") ?? false))
		{
			List<Apparel> wornApparel = p.apparel.WornApparel;
			foreach (Apparel item in wornApparel)
			{
				if (item is Apparel_Shield)
				{
					return true;
				}
			}
		}
		return false;
	}
}
