using Verse;

namespace CombatExtended;

public class HediffCompProperties_InfecterCE : HediffCompProperties
{
	public float infectionChancePerHourUntended = 0.01f;

	public HediffCompProperties_InfecterCE()
	{
		compClass = typeof(HediffComp_InfecterCE);
	}
}
