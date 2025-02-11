using Verse;

namespace AncotLibrary;

public class HediffCompProperties_MechRepair : HediffCompProperties
{
	public float energyPctPerRepair = 0.01f;

	public float energyThreshold = 0.1f;

	public int intervalTicks = 60;

	public HediffCompProperties_MechRepair()
	{
		compClass = typeof(HediffComp_MechRepair);
	}
}
