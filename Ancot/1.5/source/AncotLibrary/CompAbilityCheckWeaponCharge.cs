using RimWorld;
using Verse;

namespace AncotLibrary;

public class CompAbilityCheckWeaponCharge : CompAbilityEffect
{
	public new CompProperties_AbilityCheckWeaponCharge Props => (CompProperties_AbilityCheckWeaponCharge)props;

	public override bool GizmoDisabled(out string reason)
	{
		Pawn pawn = parent.pawn;
		CompMeleeWeaponCharge_Ability compMeleeWeaponCharge_Ability = pawn.equipment.Primary.TryGetComp<CompMeleeWeaponCharge_Ability>();
		if (compMeleeWeaponCharge_Ability == null || !compMeleeWeaponCharge_Ability.CanBeUsed)
		{
			reason = "Ancot.NoWeaponAbilityCharge".Translate();
			return true;
		}
		reason = "";
		return false;
	}
}
