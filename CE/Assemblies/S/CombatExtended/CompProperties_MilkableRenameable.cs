using RimWorld;

namespace CombatExtended;

public class CompProperties_MilkableRenameable : CompProperties_Milkable
{
	public string growthLabel = "";

	public CompProperties_MilkableRenameable()
	{
		compClass = typeof(CompMilkableRenameable);
	}
}
