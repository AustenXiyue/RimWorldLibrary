using Verse;

namespace MoreBetterDeepDrill.Types;

public class DrillableOre : IExposable
{
	private ThingDef thingDef;

	public int amountPerPortion;

	public ThingDef OreDef => thingDef;

	public DrillableOre()
	{
	}

	public DrillableOre(ThingDef thingDef, int amountPerPortion)
	{
		this.thingDef = thingDef;
		this.amountPerPortion = amountPerPortion;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Values.Look(ref amountPerPortion, "amountPerPortion", 0);
	}
}
