using Verse;

namespace Milira;

public class CompProperties_Resonator : CompProperties
{
	public HediffDef hediff;

	public BodyPartDef part;

	public string resonatorTag;

	public string appendString;

	public bool onlyTargetMechs;

	public int checkInterval = 10;

	public CompProperties_Resonator()
	{
		compClass = typeof(CompResonator);
	}
}
