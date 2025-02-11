using Verse;

namespace Milira;

public class CompProperties_Generator_SunLightFuel : CompProperties
{
	public int productPerGen = 1;

	public ThingDef product;

	public CompProperties_Generator_SunLightFuel()
	{
		compClass = typeof(CompGenerator_SunLightFuel);
	}
}
