using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Patch
{
	public static Func<string, bool, ThingDef> BaseNeurotrainer = AccessTools.Method(typeof(ThingDefGenerator_Neurotrainer), "BaseNeurotrainer", (Type[])null, (Type[])null).CreateDelegate<Func<string, bool, ThingDef>>();

	public static void Postfix(ref IEnumerable<ThingDef> __result, bool hotReload)
	{
		__result = __result.Where((ThingDef def) => !def.defName.StartsWith(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix)).Concat(ImpliedThingDefs(hotReload));
	}

	public static IEnumerable<ThingDef> ImpliedThingDefs(bool hotReload)
	{
		foreach (AbilityDef abilityDef in DefDatabase<AbilityDef>.AllDefs)
		{
			AbilityExtension_Psycast psycastExt = abilityDef.Psycast();
			if (psycastExt != null)
			{
				ThingDef thingDef = BaseNeurotrainer(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_" + ((Def)(object)abilityDef).defName, hotReload);
				thingDef.label = "PsycastNeurotrainerLabel".Translate(((Def)(object)abilityDef).label);
				thingDef.description = "PsycastNeurotrainerDescription".Translate(abilityDef.Named("PSYCAST"), $"[{psycastExt.path.LabelCap}]\n{((Def)(object)abilityDef).description}".Named("PSYCASTDESCRIPTION"));
				thingDef.comps.Add(new CompProperties_Usable
				{
					compClass = typeof(CompUsable),
					useJob = JobDefOf.UseNeurotrainer,
					useLabel = "PsycastNeurotrainerUseLabel".Translate(((Def)(object)abilityDef).label)
				});
				thingDef.comps.Add((CompProperties)(object)new CompProperties_UseEffect_Psytrainer
				{
					ability = abilityDef
				});
				thingDef.statBases.Add(new StatModifier
				{
					stat = StatDefOf.MarketValue,
					value = Mathf.Round(500f + 300f * (float)psycastExt.level)
				});
				thingDef.thingCategories = new List<ThingCategoryDef> { ThingCategoryDefOf.NeurotrainersPsycast };
				thingDef.thingSetMakerTags = new List<string> { "RewardStandardLowFreq" };
				thingDef.modContentPack = ((Def)(object)abilityDef).modContentPack;
				thingDef.descriptionHyperlinks = new List<DefHyperlink>
				{
					new DefHyperlink((Def)(object)abilityDef)
				};
				thingDef.stackLimit = 1;
				yield return thingDef;
			}
		}
	}
}
