using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CompProperties_UnderBarrel : CompProperties
{
	public CompProperties_AmmoUser propsUnderBarrel;

	public VerbPropertiesCE verbPropsUnderBarrel;

	public CompProperties_FireModes propsFireModesUnderBarrel;

	public List<string> targetTags;

	public bool targetHighVal;

	public bool oneAmmoHolder = false;

	[MustTranslate]
	public string underBarrelLabel;

	[MustTranslate]
	public string standardLabel;

	public string underBarrelIconTexPath;

	public string standardIconTexPath;

	public CompProperties_UnderBarrel()
	{
		compClass = typeof(CompUnderBarrel);
	}
}
