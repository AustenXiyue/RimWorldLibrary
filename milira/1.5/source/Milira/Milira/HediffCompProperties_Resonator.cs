using Verse;

namespace Milira;

public class HediffCompProperties_Resonator : HediffCompProperties
{
	public int maxMechBandwidth = 10;

	public HediffCompProperties_Resonator()
	{
		compClass = typeof(HediffComp_HomeTerminal);
	}
}
