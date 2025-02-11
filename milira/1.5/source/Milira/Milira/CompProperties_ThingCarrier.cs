using System.Collections.Generic;
using Verse;

namespace Milira;

public class CompProperties_ThingCarrier : CompProperties
{
	public ThingDef fixedIngredient;

	public int maxIngredientCount;

	public int startingIngredientCount;

	public CompProperties_ThingCarrier()
	{
		compClass = typeof(CompThingCarrier);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
	}
}
