using Verse;
using Verse.AI;

namespace AncotLibrary;

public class ThinkNode_ConditionalWeaponEnergy : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return IsPawnEquippedWithMeleeWeapon(pawn);
	}

	private bool IsPawnEquippedWithMeleeWeapon(Pawn pawn)
	{
		if (pawn.equipment == null || pawn.equipment.Primary == null)
		{
			return false;
		}
		ThingWithComps primary = pawn.equipment.Primary;
		CompMeleeWeaponCharge_Ability compMeleeWeaponCharge_Ability = primary.TryGetComp<CompMeleeWeaponCharge_Ability>();
		return compMeleeWeaponCharge_Ability.CanBeUsed;
	}
}
