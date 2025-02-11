using RimWorld;

namespace AncotLibrary;

public class CompProperties_AbilityInstantRepair : CompProperties_AbilityEffect
{
	public int repairPoint = 10;

	public CompProperties_AbilityInstantRepair()
	{
		compClass = typeof(CompAbilityInstantRepair);
	}
}
