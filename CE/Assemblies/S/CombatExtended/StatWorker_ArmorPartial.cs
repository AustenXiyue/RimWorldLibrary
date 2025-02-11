using System;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_ArmorPartial : StatWorker
{
	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		if (Controller.settings.PartialStat)
		{
			if (req.Thing != null)
			{
				if (req.Thing.def.HasModExtension<PartialArmorExt>())
				{
					string text = "";
					if ((stat?.parts ?? null) != null)
					{
						foreach (StatPart part in stat.parts)
						{
							text = text + part.ExplanationPart(req) + "\n";
						}
					}
					text += "\n" + "CE_StatWorker_ArmorGeneral".Translate() + finalVal.ToString() + " \n \n" + "CE_StatWorker_ArmorSpecific".Translate();
					PartialArmorExt modExtension = req.Thing.def.GetModExtension<PartialArmorExt>();
					foreach (ApparelPartialStat stat in modExtension.stats)
					{
						if (stat.stat != base.stat)
						{
							continue;
						}
						float num = 0f;
						if (req.Thing is Apparel)
						{
							num = (float)Math.Round(((Apparel)req.Thing).PartialStat(base.stat, stat.parts.First()), 2);
						}
						else if (req.Thing is Pawn pawn)
						{
							num = (float)Math.Round(pawn.PartialStat(stat.stat, stat.parts.First()), 2);
						}
						text += "\n";
						text = text + stat.stat.formatString.Replace("{0}", num.ToString()) + " for: ";
						foreach (BodyPartDef part2 in stat.parts)
						{
							text += "\n - ";
							text += part2.label;
						}
					}
					return text;
				}
			}
			else if (req.Def.HasModExtension<PartialArmorExt>())
			{
				string text2 = "";
				if ((base.stat?.parts ?? null) != null)
				{
					foreach (StatPart part3 in base.stat.parts)
					{
						string text3 = part3.ExplanationPart(req);
						if (text3 != null && text3.Any() && text3.Count() > 0)
						{
							text2 = text2 + "\n" + text3 + "\n";
						}
					}
				}
				text2 += "CE_StatWorker_ArmorGeneral".Translate() + finalVal.ToString() + " \n \n" + "CE_StatWorker_ArmorSpecific".Translate();
				PartialArmorExt modExtension2 = req.Def.GetModExtension<PartialArmorExt>();
				foreach (ApparelPartialStat stat2 in modExtension2.stats)
				{
					if (stat2.stat != base.stat)
					{
						continue;
					}
					text2 = (stat2.useStatic ? ((string)(text2 + ("\n" + "CE_SetValPartial".Translate() + " " + stat2.staticValue.ToStringPercent()))) : ((string)(text2 + ("\n" + "CE_Multiplier".Translate() + " " + stat2.mult.ToStringPercent()))));
					foreach (BodyPartDef part4 in stat2.parts)
					{
						text2 = text2 + "\n -" + part4.label;
					}
				}
				return text2;
			}
		}
		return base.GetExplanationFinalizePart(req, numberSense, finalVal);
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		if (Controller.settings.PartialStat && (base.stat == StatDefOf.ArmorRating_Blunt || base.stat == StatDefOf.ArmorRating_Sharp))
		{
			bool flag = true;
			if (optionalReq.Thing is Apparel apparel)
			{
				float num = value;
				float num2 = value;
				if (apparel.def.HasModExtension<PartialArmorExt>())
				{
					foreach (ApparelPartialStat stat2 in apparel.def.GetModExtension<PartialArmorExt>().stats)
					{
						float num3 = value;
						num3 = ((!stat2.useStatic) ? (num3 * stat2.mult) : stat2.staticValue);
						if (num3 < num)
						{
							num = num3;
						}
						else if (num3 > num2)
						{
							num2 = num3;
						}
					}
					string text = num.ToString("0.00");
					string text2 = num2.ToString("0.00");
					return string.Format(stat.formatString, text + " ~ " + text2);
				}
			}
			else
			{
				Def def = optionalReq.Def;
				if (def != null && def.HasModExtension<PartialArmorExt>())
				{
					float num4 = value;
					float num5 = value;
					PartialArmorExt modExtension = optionalReq.Def.GetModExtension<PartialArmorExt>();
					foreach (ApparelPartialStat stat3 in modExtension.stats)
					{
						float num6 = value;
						if (stat3.stat == stat)
						{
							num6 = ((!stat3.useStatic) ? (num6 * stat3.mult) : stat3.staticValue);
						}
						if (num6 < num4)
						{
							num4 = num6;
						}
						else if (num6 > num5)
						{
							num5 = num6;
						}
						if (num4 != value || num5 != value)
						{
							string text3 = num4.ToString("0.00");
							string text4 = num5.ToString("0.00");
							return string.Format(stat.formatString, text3 + " ~ " + text4);
						}
					}
				}
			}
		}
		return base.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, finalized);
	}
}
