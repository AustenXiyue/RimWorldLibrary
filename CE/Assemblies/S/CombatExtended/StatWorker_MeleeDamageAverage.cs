using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_MeleeDamageAverage : StatWorker_MeleeDamageBase
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess)
	{
		float num = 0.5f;
		float num2 = 1.5f;
		if (req.Thing?.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker)
		{
			num = StatWorker_MeleeDamageBase.GetDamageVariationMin(pawn_EquipmentTracker.pawn);
			num2 = StatWorker_MeleeDamageBase.GetDamageVariationMax(pawn_EquipmentTracker.pawn);
		}
		List<Tool> list = (req.Def as ThingDef)?.tools;
		if (list.NullOrEmpty())
		{
			return 0f;
		}
		if (list.Any((Tool x) => !(x is ToolCE)))
		{
			Log.Error("Trying to get stat MeleeDamageAverage from " + req.Def.defName + " which has no support for Combat Extended.");
			return 0f;
		}
		float num3 = 0f;
		foreach (Tool item in list)
		{
			num3 += item.chanceFactor;
		}
		float num4 = 0f;
		foreach (Tool item2 in list)
		{
			float adjustedDamage = StatWorker_MeleeDamageBase.GetAdjustedDamage((ToolCE)item2, req.Thing);
			float num5 = adjustedDamage / item2.cooldownTime * num;
			float num6 = adjustedDamage / item2.cooldownTime * num2;
			float num7 = item2.chanceFactor / num3;
			num4 += num7 * ((num5 + num6) / 2f);
		}
		return num4;
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
		stringBuilder.AppendLine(string.Format("{0}: {1}% - {2}%", "CE_DamageVariation".Translate(), (100f * num).ToStringByStyle(ToStringStyle.FloatMaxTwo), (100f * num2).ToStringByStyle(ToStringStyle.FloatMaxTwo)));
		stringBuilder.AppendLine("");
		foreach (ToolCE tool in list)
		{
			float adjustedDamage = StatWorker_MeleeDamageBase.GetAdjustedDamage(tool, req.Thing);
			float num4 = adjustedDamage / tool.cooldownTime * num;
			float num5 = adjustedDamage / tool.cooldownTime * num2;
			IEnumerable<ManeuverDef> enumerable = DefDatabase<ManeuverDef>.AllDefsListForReading.Where((ManeuverDef d) => tool.capacities.Contains(d.requiredCapacity));
			string text = "(";
			foreach (ManeuverDef item in enumerable)
			{
				text = text + item.ToString() + "/";
			}
			text = text.TrimmedToLength(text.Length - 1) + ")";
			stringBuilder.AppendLine("  " + "Tool".Translate() + ": " + tool.ToString() + " " + text);
			stringBuilder.AppendLine("    " + "CE_DescBaseDamage".Translate() + ": " + tool.power.ToStringByStyle(ToStringStyle.FloatMaxTwo));
			stringBuilder.AppendLine("    " + "CE_AdjustedForWeapon".Translate() + ": " + adjustedDamage.ToStringByStyle(ToStringStyle.FloatMaxTwo));
			stringBuilder.AppendLine("    " + "CooldownTime".Translate() + ": " + tool.cooldownTime.ToStringByStyle(ToStringStyle.FloatMaxTwo) + " seconds");
			stringBuilder.AppendLine("    " + "CE_DPS".Translate() + ": " + (adjustedDamage / tool.cooldownTime).ToStringByStyle(ToStringStyle.FloatMaxTwo));
			stringBuilder.AppendLine(string.Format("    " + "CE_DamageVariation".Translate() + ": {0} - {1}", num4.ToStringByStyle(ToStringStyle.FloatMaxTwo), num5.ToStringByStyle(ToStringStyle.FloatMaxTwo)));
			stringBuilder.AppendLine("    " + "CE_FinalAverageDamage".Translate() + ": " + ((num4 + num5) / 2f).ToStringByStyle(ToStringStyle.FloatMaxTwo));
			stringBuilder.AppendLine("    " + "CE_ChanceFactor".Translate() + ": " + tool.chanceFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo));
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}
}
