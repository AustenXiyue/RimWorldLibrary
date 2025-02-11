using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class ApparelPatcherPresetDef : Def
{
	public List<ApparelLayerDef> neededLayers;

	public FloatRange vanillaArmorRatingRange;

	public List<BodyPartGroupDef> neededGroups;

	public SimpleCurve ArmorCurveSharp = null;

	public SimpleCurve ArmorCurveBlunt = null;

	public float ArmorStaticSharp;

	public float ArmorStaticBlunt;

	public float Bulk;

	public float Mass;

	public float BulkWorn;

	public List<ApparelPartialStat> partialStats;

	public float FinalRatingSharp(float vanillaRating)
	{
		return ArmorCurveSharp?.Evaluate(vanillaRating) ?? ArmorStaticSharp;
	}

	public float FinalRatingBlunt(float vanillaRating)
	{
		return ArmorCurveBlunt?.Evaluate(vanillaRating) ?? ArmorStaticBlunt;
	}
}
