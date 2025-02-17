using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StatPart_BedStat : StatPart
{
	private StatDef stat;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			val *= BedMultiplier(pawn);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn { ageTracker: not null } pawn)
		{
			return "StatsReport_InBed".Translate() + ": x" + BedMultiplier(pawn).ToStringPercent();
		}
		return null;
	}

	private float BedMultiplier(Pawn pawn)
	{
		if (pawn.InBed())
		{
			return pawn.CurrentBed().GetStatValue(stat);
		}
		if (pawn.InCaravanBed())
		{
			return pawn.CurrentCaravanBed().GetStatValue(stat);
		}
		return 1f;
	}
}
