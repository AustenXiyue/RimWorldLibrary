using RimWorld;

namespace CombatExtended;

public class CompProperties_ShearableRenameable : CompProperties_Shearable
{
	public string growthLabel = "";

	public CompProperties_ShearableRenameable()
	{
		compClass = typeof(CompShearableRenameable);
	}
}
