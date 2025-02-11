using RimWorld;
using Verse;

namespace AncotLibrary;

public class CompProperties_WeaponAbility : CompProperties
{
	public AbilityDef abilityDef;

	public CompProperties_WeaponAbility()
	{
		compClass = typeof(CompWeaponAbility);
	}
}
