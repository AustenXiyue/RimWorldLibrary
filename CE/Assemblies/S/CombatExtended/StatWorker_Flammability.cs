using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class StatWorker_Flammability : StatWorker
{
	private static readonly SimpleCurve HumidityCurve;

	static StatWorker_Flammability()
	{
		HumidityCurve = new SimpleCurve();
		HumidityCurve.Add(0f, FireSpread.values.flammabilityHumidityMin);
		HumidityCurve.Add(0.5f, FireSpread.values.flammabilityHumidityHalf);
		HumidityCurve.Add(1f, FireSpread.values.flammabilityHumidityMax);
	}

	private static float GetPrecipitationFactorFor(Thing plant)
	{
		WeatherTracker component = plant.MapHeld.GetComponent<WeatherTracker>();
		return HumidityCurve.Evaluate(component.HumidityPercent);
	}

	private static void GetApparelAdjustFor(Pawn pawn, out float apparelFlammability, out float apparelCoverage)
	{
		apparelFlammability = 0f;
		apparelCoverage = 0f;
		foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
		{
			List<Apparel> list = pawn.apparel?.WornApparel.FindAll((Apparel a) => a.def.apparel.CoversBodyPart(part));
			if (list != null && list.Any())
			{
				list.SortBy((Apparel a) => a.GetStatValue(StatDefOf.Flammability));
				Apparel thing = list.First();
				apparelFlammability += thing.GetStatValue(StatDefOf.Flammability) * part.coverageAbs;
				apparelCoverage += part.coverageAbs;
			}
		}
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		float valueUnfinalized = base.GetValueUnfinalized(req, applyPostProcess);
		if (req.Thing is Plant plant)
		{
			return valueUnfinalized * GetPrecipitationFactorFor(plant);
		}
		if (req.Thing is Pawn pawn)
		{
			Pawn_ApparelTracker apparel = pawn.apparel;
			if (apparel != null && apparel.WornApparelCount > 0)
			{
				GetApparelAdjustFor(pawn, out var apparelFlammability, out var apparelCoverage);
				apparelFlammability += valueUnfinalized * (1f - apparelCoverage);
				return valueUnfinalized - (1f - apparelFlammability);
			}
		}
		return valueUnfinalized;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		string explanationUnfinalized = base.GetExplanationUnfinalized(req, numberSense);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(explanationUnfinalized);
		if (req.Thing is Plant plant)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Format("{0}: {1}", "CE_StatsReport_FlammabilityPrecipitation".Translate().Trim(), GetPrecipitationFactorFor(plant).ToStringByStyle(ToStringStyle.PercentZero)));
		}
		if (req.Thing is Pawn pawn)
		{
			Pawn_ApparelTracker apparel = pawn.apparel;
			if (apparel != null && apparel.WornApparelCount > 0)
			{
				GetApparelAdjustFor(pawn, out var apparelFlammability, out var apparelCoverage);
				apparelFlammability /= apparelCoverage;
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(string.Format("{0}: {1}", "CE_StatsReport_FlammabilityApparelFactor".Translate().Trim(), apparelFlammability.ToStringByStyle(ToStringStyle.PercentZero)));
				stringBuilder.AppendLine(string.Format("{0}: {1}", "CE_StatsReport_FlammabilityApparelCoverage".Translate().Trim(), apparelCoverage.ToStringByStyle(ToStringStyle.PercentZero)));
			}
		}
		return stringBuilder.ToString();
	}
}
