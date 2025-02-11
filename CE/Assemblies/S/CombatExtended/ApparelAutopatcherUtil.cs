using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public static class ApparelAutopatcherUtil
{
	public static void AddModExtension(this ThingDef def, DefModExtension value)
	{
		if (def.modExtensions == null)
		{
			def.modExtensions = new List<DefModExtension>();
		}
		def.modExtensions.Add(value);
	}

	public static float GetStatValueDef(this ThingDef def, StatDef statDef)
	{
		return (def.statBases?.Find((StatModifier x) => x.stat == statDef)?.value).GetValueOrDefault();
	}

	public static bool Matches(this ThingDef matchee, ApparelPatcherPresetDef matcher)
	{
		bool flag = false;
		return matchee.apparel.layers.All((ApparelLayerDef x) => matcher.neededLayers.Contains(x)) && matchee.apparel.bodyPartGroups.All((BodyPartGroupDef x) => matcher.neededGroups.Contains(x)) && (matcher.vanillaArmorRatingRange.Includes(matchee.GetStatValueDef(StatDefOf.ArmorRating_Sharp)) || matcher.vanillaArmorRatingRange.Includes(matchee.GetStatValueDef(StatDefOf.StuffEffectMultiplierArmor)));
	}
}
