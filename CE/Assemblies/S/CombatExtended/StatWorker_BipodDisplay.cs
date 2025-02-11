using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class StatWorker_BipodDisplay : StatWorker
{
	public CompProperties_BipodComp bipodComp(StatRequest req)
	{
		CompProperties_BipodComp compProperties_BipodComp = req.Thing?.TryGetComp<BipodComp>().Props;
		if (compProperties_BipodComp == null)
		{
			req.Thing.def.statBases.RemoveAll((StatModifier x) => x.stat == stat);
		}
		return compProperties_BipodComp;
	}

	public CompProperties_BipodComp bipodCompDef(StatRequest req)
	{
		if (!(req.Def is ThingDef))
		{
			return null;
		}
		CompProperties compProperties = ((ThingDef)req.Def).comps.Find((CompProperties x) => x is CompProperties_BipodComp);
		if (compProperties == null)
		{
			((ThingDef)req.Def).statBases.RemoveAll((StatModifier x) => x.stat == stat);
		}
		return (CompProperties_BipodComp)compProperties;
	}

	public VerbPropertiesCE verbPropsCE(StatRequest req)
	{
		VerbPropertiesCE verbPropertiesCE = (VerbPropertiesCE)(req.Thing?.def.verbs.Find((VerbProperties x) => x is VerbPropertiesCE));
		if (verbPropertiesCE == null)
		{
			req.Thing.def.statBases.RemoveAll((StatModifier x) => x.stat == stat);
		}
		return verbPropertiesCE;
	}

	public VerbPropertiesCE verbPropsDef(StatRequest req)
	{
		if (!(req.Def is ThingDef))
		{
			return null;
		}
		VerbProperties verbProperties = ((ThingDef)req.Def).verbs.Find((VerbProperties x) => x is VerbPropertiesCE);
		if (verbProperties == null)
		{
			((ThingDef)req.Def).statBases.RemoveAll((StatModifier x) => x.stat == stat);
		}
		return (VerbPropertiesCE)verbProperties;
	}

	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		if (req.HasThing && req.Thing != null)
		{
			CompProperties_BipodComp compProperties_BipodComp = bipodComp(req);
			VerbPropertiesCE verbPropertiesCE = verbPropsCE(req);
			string text = string.Concat("CE_BipodSetupTime".Translate(), compProperties_BipodComp.ticksToSetUp.ToString(), " ticks (", (compProperties_BipodComp.ticksToSetUp / 60).ToString(), "s)");
			if (Controller.settings.AutoSetUp)
			{
				text += "\n" + "CE_BipodAutoSetupMode".Translate() + "\n";
				if (compProperties_BipodComp.catDef.useAutoSetMode)
				{
					text = text + "- " + compProperties_BipodComp.catDef.autosetMode.ToString() + "\n";
				}
				else
				{
					text = text + "- " + AimMode.AimedShot.ToString() + "\n";
					text = text + "- " + AimMode.SuppressFire.ToString() + "\n";
				}
			}
			text = text + "\n" + "CE_BipodStatWhenSetUp".Translate().Colorize(Color.green) + "\n";
			text = text + CE_StatDefOf.Recoil.label + ": " + Math.Round(verbPropertiesCE.recoilAmount * compProperties_BipodComp.recoilMulton, 2);
			text += "\n";
			text = text + CE_StatDefOf.SwayFactor.label + ": " + Math.Round(req.Thing.def.statBases.Find((StatModifier x) => x.stat == CE_StatDefOf.SwayFactor).value * compProperties_BipodComp.swayMult, 2);
			text += "\n";
			text = string.Concat(text, "CE_BipodStatRange".Translate() + ": ", ((float)compProperties_BipodComp.additionalrange + verbPropertiesCE.range).ToString());
			text += "\n";
			text = string.Concat(text, "CE_BipodStatWarmUp".Translate() + ": ", (compProperties_BipodComp.warmupMult * verbPropertiesCE.warmupTime).ToString());
			text += "\n\n";
			text = text + "CE_BipodStatWhenNotSetUp".Translate().Colorize(Color.red) + "\n";
			text = text + CE_StatDefOf.Recoil.label + ": " + Math.Round(verbPropertiesCE.recoilAmount * compProperties_BipodComp.recoilMultoff, 2);
			text += "\n";
			text = text + CE_StatDefOf.SwayFactor.label + ": " + Math.Round(req.Thing.def.statBases.Find((StatModifier x) => x.stat == CE_StatDefOf.SwayFactor).value * compProperties_BipodComp.swayPenalty, 2);
			text += "\n";
			text = string.Concat(text, "CE_BipodStatRange".Translate() + ": ", verbPropertiesCE.range.ToString());
			text += "\n";
			return string.Concat(text, "CE_BipodStatWarmUp".Translate() + ": ", (compProperties_BipodComp.warmupPenalty * verbPropertiesCE.warmupTime).ToString());
		}
		if (req.Def != null)
		{
			CompProperties_BipodComp compProperties_BipodComp2 = bipodCompDef(req);
			VerbPropertiesCE verbPropertiesCE2 = verbPropsDef(req);
			if (verbPropertiesCE2 == null || compProperties_BipodComp2 == null)
			{
				return base.GetExplanationFinalizePart(req, numberSense, finalVal);
			}
			string text2 = string.Concat("CE_BipodSetupTime".Translate(), compProperties_BipodComp2.ticksToSetUp.ToString(), " ticks (", (compProperties_BipodComp2.ticksToSetUp / 60).ToString(), "s)\n", "Stats when set up: ".Colorize(Color.green), "\n");
			text2 = text2 + CE_StatDefOf.Recoil.label + ": " + verbPropertiesCE2.recoilAmount * compProperties_BipodComp2.recoilMulton;
			text2 += "\n";
			text2 = text2 + CE_StatDefOf.SwayFactor.label + ": " + ((ThingDef)req.Def).statBases.Find((StatModifier x) => x.stat == CE_StatDefOf.SwayFactor).value * compProperties_BipodComp2.swayPenalty;
			text2 += "\n";
			text2 = string.Concat(text2, "CE_BipodStatRange".Translate() + ": ", ((float)compProperties_BipodComp2.additionalrange + verbPropertiesCE2.range).ToString());
			text2 += "\n";
			text2 = string.Concat(text2, "CE_BipodStatWarmUp".Translate() + ": ", (compProperties_BipodComp2.warmupMult * verbPropertiesCE2.warmupTime).ToString());
			text2 += "\n\n";
			text2 = text2 + "CE_BipodStatWhenNotSetUp".Translate().Colorize(Color.red) + "\n";
			text2 = text2 + CE_StatDefOf.Recoil.label + ": " + verbPropertiesCE2.recoilAmount * compProperties_BipodComp2.recoilMultoff;
			text2 += "\n";
			text2 = text2 + CE_StatDefOf.SwayFactor.label + ": " + ((ThingDef)req.Def).statBases.Find((StatModifier x) => x.stat == CE_StatDefOf.SwayFactor).value * compProperties_BipodComp2.swayPenalty;
			text2 += "\n";
			text2 = string.Concat(text2, "CE_BipodStatRange".Translate() + ": ", verbPropertiesCE2.range.ToString());
			text2 += "\n";
			return string.Concat(text2, "CE_BipodStatWarmUp".Translate() + ": ", (compProperties_BipodComp2.warmupPenalty * verbPropertiesCE2.warmupTime).ToString());
		}
		return base.GetExplanationFinalizePart(req, numberSense, finalVal);
	}

	public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
	{
		if (finalized)
		{
			return "CE_BipodHoverOverStat".Translate();
		}
		return null;
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		return req.Thing?.def.comps.Any((CompProperties x) => x is CompProperties_BipodComp) ?? (req.Def is ThingDef && ((ThingDef)req.Def).comps.Any((CompProperties x) => x is CompProperties_BipodComp));
	}
}
