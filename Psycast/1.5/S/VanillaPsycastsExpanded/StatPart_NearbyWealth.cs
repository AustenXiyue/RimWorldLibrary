using System.Linq;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public class StatPart_NearbyWealth : StatPart_Focus
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ApplyOn(req) && req.Thing.Map != null)
		{
			float wealthTotal = req.Thing.Map.wealthWatcher.WealthTotal;
			float num = GenRadialCached.RadialDistinctThingsAround(req.Thing.Position, req.Thing.Map, 6f, useCenter: true).Sum((Thing t) => t.MarketValue * (float)t.stackCount);
			val += num / wealthTotal;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (!ApplyOn(req) || req.Thing.Map == null)
		{
			return "";
		}
		float wealthTotal = req.Thing.Map.wealthWatcher.WealthTotal;
		float num = GenRadialCached.RadialDistinctThingsAround(req.Thing.Position, req.Thing.Map, 6f, useCenter: true).Sum((Thing t) => t.MarketValue * (float)t.stackCount);
		return "VPE.WealthNearby".Translate(num.ToStringMoney(), wealthTotal.ToStringMoney()) + ": " + parentStat.Worker.ValueToString(num / wealthTotal, finalized: true, ToStringNumberSense.Offset);
	}
}
