using RimWorld;
using Verse;

namespace CombatExtended;

public class StatPart_Bulk : StatPart
{
	public bool ValidReq(StatRequest req)
	{
		return req.HasThing && inv(req) != null;
	}

	public CompInventory inv(StatRequest req)
	{
		return req.Thing.TryGetComp<CompInventory>();
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (ValidReq(req))
		{
			return string.Concat("CE_BulkEffect".Translate() + " x", (MassBulkUtility.HitChanceBulkFactor(inv(req).currentBulk, inv(req).capacityBulk) * 100f).ToString(), "%");
		}
		return null;
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ValidReq(req))
		{
			val *= MassBulkUtility.HitChanceBulkFactor(inv(req).currentBulk, inv(req).capacityBulk);
		}
	}
}
