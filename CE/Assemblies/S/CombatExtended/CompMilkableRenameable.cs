using RimWorld;
using Verse;

namespace CombatExtended;

public class CompMilkableRenameable : CompMilkable
{
	private string growthLabel = "MilkFullness".Translate();

	private CompProperties_MilkableRenameable properties => props as CompProperties_MilkableRenameable;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		if (properties != null && !properties.growthLabel.NullOrEmpty())
		{
			growthLabel = properties.growthLabel;
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!Active)
		{
			return null;
		}
		return growthLabel + ": " + base.Fullness.ToStringPercent();
	}
}
