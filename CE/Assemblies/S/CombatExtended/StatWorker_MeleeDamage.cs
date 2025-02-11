using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_MeleeDamage : StatWorker_MeleeDamageBase
{
	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		return GetFinalDisplayValue(optionalReq);
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		float num = 0.5f;
		float num2 = 1.5f;
		int num3 = -1;
		if (req.Thing?.ParentHolder is Pawn_EquipmentTracker { pawn: not null, pawn: var pawn })
		{
			num = StatWorker_MeleeDamageBase.GetDamageVariationMin(pawn);
			num2 = StatWorker_MeleeDamageBase.GetDamageVariationMax(pawn);
			if (pawn.skills != null)
			{
				num3 = pawn.skills.GetSkill(SkillDefOf.Melee).Level;
			}
		}
		List<Tool> list = (req.Def as ThingDef)?.tools;
		if (list.NullOrEmpty())
		{
			return base.GetExplanationUnfinalized(req, numberSense);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (num3 >= 0)
		{
			stringBuilder.AppendLine(string.Concat("CE_WielderSkillLevel".Translate() + ": ", num3.ToString()));
		}
		stringBuilder.AppendLine(string.Format("CE_DamageVariation".Translate() + ": {0}% - {1}%", (100f * num).ToStringByStyle(ToStringStyle.FloatMaxTwo), (100f * num2).ToStringByStyle(ToStringStyle.FloatMaxTwo)));
		stringBuilder.AppendLine("");
		foreach (Tool item in list)
		{
			ToolCE toolCE = item as ToolCE;
			if (toolCE != null)
			{
				float adjustedDamage = StatWorker_MeleeDamageBase.GetAdjustedDamage(toolCE, req.Thing);
				IEnumerable<ManeuverDef> enumerable = DefDatabase<ManeuverDef>.AllDefsListForReading.Where((ManeuverDef d) => toolCE.capacities.Contains(d.requiredCapacity));
				string text = "(";
				foreach (ManeuverDef item2 in enumerable)
				{
					text = text + item2.ToString() + "/";
				}
				text = text.TrimmedToLength(text.Length - 1) + ")";
				stringBuilder.AppendLine("  " + "Tool".Translate() + ": " + toolCE.ToString() + " " + text);
				stringBuilder.AppendLine("    " + "CE_DescBaseDamage".Translate() + ": " + toolCE.power.ToStringByStyle(ToStringStyle.FloatMaxTwo));
				stringBuilder.AppendLine("    " + "CE_AdjustedForWeapon".Translate() + ": " + adjustedDamage.ToStringByStyle(ToStringStyle.FloatMaxTwo));
				stringBuilder.AppendLine(string.Format("    " + "StatsReport_FinalValue".Translate() + ": {0} - {1}", (adjustedDamage * num).ToStringByStyle(ToStringStyle.FloatMaxTwo), (adjustedDamage * num2).ToStringByStyle(ToStringStyle.FloatMaxTwo)));
				stringBuilder.AppendLine();
			}
			else
			{
				if (DebugSettings.godMode)
				{
					Log.Error("Trying to get stat MeleeDamage from " + req.Def.defName + " which has no support for Combat Extended.");
				}
				stringBuilder.AppendLine("CE_UnpatchedWeapon".Translate());
			}
		}
		return stringBuilder.ToString();
	}

	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		return "StatsReport_FinalValue".Translate() + ": " + GetFinalDisplayValue(req);
	}

	private string GetFinalDisplayValue(StatRequest optionalReq)
	{
		float num = 0.5f;
		float num2 = 1.5f;
		if (optionalReq.Thing?.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker)
		{
			num = StatWorker_MeleeDamageBase.GetDamageVariationMin(pawn_EquipmentTracker.pawn);
			num2 = StatWorker_MeleeDamageBase.GetDamageVariationMax(pawn_EquipmentTracker.pawn);
		}
		List<Tool> list = (optionalReq.Def as ThingDef)?.tools;
		if (list.NullOrEmpty())
		{
			return "";
		}
		if (list.Any((Tool x) => !(x is ToolCE)))
		{
			if (DebugSettings.godMode)
			{
				Log.Error("Trying to get stat MeleeDamage from " + optionalReq.Def.defName + " which has no support for Combat Extended.");
			}
			return "CE_UnpatchedWeaponShort".Translate();
		}
		float num3 = 2.1474836E+09f;
		float num4 = 0f;
		foreach (ToolCE item in list)
		{
			float adjustedDamage = StatWorker_MeleeDamageBase.GetAdjustedDamage(item, optionalReq.Thing);
			if (adjustedDamage > num4)
			{
				num4 = adjustedDamage;
			}
			if (adjustedDamage < num3)
			{
				num3 = adjustedDamage;
			}
		}
		return (num3 * num).ToStringByStyle(ToStringStyle.FloatMaxTwo) + " - " + (num4 * num2).ToStringByStyle(ToStringStyle.FloatMaxTwo);
	}
}
