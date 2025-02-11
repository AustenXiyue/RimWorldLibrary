using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class StatWorker_MeleeArmorPenetration : StatWorker_MeleeStats
{
	public const float skillFactorPerLevel = 1f / 76f;

	public const float powerForOtherFactors = 0.75f;

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		return GetFinalDisplayValue(optionalReq);
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		List<Tool> list = (req.Def as ThingDef)?.tools;
		if (list.NullOrEmpty())
		{
			return base.GetExplanationUnfinalized(req, numberSense);
		}
		StringBuilder stringBuilder = new StringBuilder();
		float penetrationFactor = GetPenetrationFactor(req);
		float skillFactor = GetSkillFactor(req);
		stringBuilder.AppendLine("CE_WeaponPenetrationFactor".Translate() + ": " + penetrationFactor.ToStringByStyle(ToStringStyle.PercentZero));
		if (Mathf.Abs(skillFactor - 1f) > 0.001f)
		{
			stringBuilder.AppendLine("CE_WeaponPenetrationSkillFactor".Translate() + ": " + skillFactor.ToStringByStyle(ToStringStyle.PercentZero));
		}
		stringBuilder.AppendLine();
		foreach (ToolCE tool in list)
		{
			IEnumerable<ManeuverDef> enumerable = DefDatabase<ManeuverDef>.AllDefsListForReading.Where((ManeuverDef d) => tool.capacities.Contains(d.requiredCapacity));
			string text = "(";
			foreach (ManeuverDef item in enumerable)
			{
				text = text + item.ToString() + "/";
			}
			text = text.TrimmedToLength(text.Length - 1) + ")";
			stringBuilder.AppendLine("  " + "Tool".Translate() + ": " + tool.ToString() + " " + text);
			float num = GetOtherFactors(req).Aggregate(1f, (float x, float y) => x * y);
			if (Mathf.Abs(num - 1f) > 0.001f)
			{
				stringBuilder.AppendLine("   " + "CE_WeaponPenetrationOtherFactors".Translate() + ": " + num.ToStringByStyle(ToStringStyle.PercentZero));
			}
			stringBuilder.Append(string.Format("    {0}: {1} x {2}", "CE_DescSharpPenetration".Translate(), tool.armorPenetrationSharp.ToStringByStyle(ToStringStyle.FloatMaxTwo), penetrationFactor.ToStringByStyle(ToStringStyle.FloatMaxThree)));
			if (Mathf.Abs(skillFactor - 1f) > 0.001f)
			{
				stringBuilder.Append($" x {skillFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo)}");
			}
			if (Mathf.Abs(num - 1f) > 0.001f)
			{
				stringBuilder.Append($" x {num.ToStringByStyle(ToStringStyle.FloatMaxTwo)}");
			}
			stringBuilder.AppendLine(string.Format(" = {0} {1}", (tool.armorPenetrationSharp * penetrationFactor * skillFactor * num).ToStringByStyle(ToStringStyle.FloatMaxTwo), "CE_mmRHA".Translate()));
			stringBuilder.Append(string.Format("    {0}: {1} x {2}", "CE_DescBluntPenetration".Translate(), tool.armorPenetrationBlunt.ToStringByStyle(ToStringStyle.FloatMaxTwo), penetrationFactor.ToStringByStyle(ToStringStyle.FloatMaxThree)));
			if (Mathf.Abs(skillFactor - 1f) > 0.001f)
			{
				stringBuilder.Append($" x {skillFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo)}");
			}
			if (Mathf.Abs(num - 1f) > 0.001f)
			{
				stringBuilder.Append($" x {num.ToStringByStyle(ToStringStyle.FloatMaxTwo)}");
			}
			stringBuilder.AppendLine(string.Format(" = {0} {1}", (tool.armorPenetrationBlunt * penetrationFactor * skillFactor * num).ToStringByStyle(ToStringStyle.FloatMaxTwo), "CE_MPa".Translate()));
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		return "StatsReport_FinalValue".Translate() + ": " + GetFinalDisplayValue(req);
	}

	private string GetFinalDisplayValue(StatRequest optionalReq)
	{
		List<Tool> list = (optionalReq.Def as ThingDef)?.tools;
		if (list.NullOrEmpty())
		{
			return "";
		}
		if (list.Any((Tool x) => !(x is ToolCE)))
		{
			Log.Error("Trying to get stat MeleeArmorPenetration from " + optionalReq.Def.defName + " which has no support for Combat Extended.");
			return "";
		}
		float num = 0f;
		foreach (Tool item in list)
		{
			num += item.chanceFactor;
		}
		float num2 = 0f;
		float num3 = 0f;
		foreach (ToolCE item2 in list)
		{
			float num4 = item2.chanceFactor / num;
			float num5 = GetOtherFactors(optionalReq).Aggregate(1f, (float x, float y) => x * y);
			num2 += num4 * item2.armorPenetrationSharp * num5;
			num3 += num4 * item2.armorPenetrationBlunt * num5;
		}
		float penetrationFactor = GetPenetrationFactor(optionalReq);
		float skillFactor = GetSkillFactor(optionalReq);
		return (num2 * penetrationFactor * skillFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo) + " " + "CE_mmRHA".Translate() + ", " + (num3 * penetrationFactor * skillFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo) + " " + "CE_MPa".Translate();
	}

	private float GetPenetrationFactor(StatRequest req)
	{
		float num = 1f;
		if (req.Thing != null)
		{
			return req.Thing.GetStatValue(CE_StatDefOf.MeleePenetrationFactor);
		}
		num *= req.StuffDef?.stuffProps?.statFactors?.FirstOrDefault((StatModifier t) => t.stat == CE_StatDefOf.MeleePenetrationFactor)?.value ?? 1f;
		return num + (req.StuffDef?.stuffProps?.statOffsets?.FirstOrDefault((StatModifier t) => t.stat == CE_StatDefOf.MeleePenetrationFactor)?.value).GetValueOrDefault();
	}

	private float GetSkillFactor(StatRequest req)
	{
		float num = 1f;
		if (req.Thing is Pawn { skills: not null } pawn)
		{
			num += 1f / 76f * (float)(pawn.skills.GetSkill(SkillDefOf.Melee).Level - 1);
		}
		else
		{
			Pawn pawn2 = (req.Thing?.ParentHolder as Pawn_EquipmentTracker)?.pawn;
			if (pawn2 != null && pawn2.skills != null)
			{
				num += 1f / 76f * (float)(pawn2.skills.GetSkill(SkillDefOf.Melee).Level - 1);
			}
		}
		return num;
	}

	private IEnumerable<float> GetOtherFactors(StatRequest req)
	{
		Pawn pawn = (req.Thing as Pawn) ?? (req.Thing?.ParentHolder as Pawn_EquipmentTracker)?.pawn;
		if (pawn != null)
		{
			yield return Mathf.Pow(pawn.ageTracker.CurLifeStage.meleeDamageFactor, 0.75f);
			yield return Mathf.Pow(pawn.GetStatValue(StatDefOf.MeleeDamageFactor), 0.75f);
		}
	}
}
