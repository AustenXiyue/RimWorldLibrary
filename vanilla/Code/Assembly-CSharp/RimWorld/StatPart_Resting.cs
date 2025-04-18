using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StatPart_Resting : StatPart
{
	public float factor = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			val *= RestingMultiplier(pawn);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			return "StatsReport_Resting".Translate() + ": x" + RestingMultiplier(pawn).ToStringPercent();
		}
		return null;
	}

	private float RestingMultiplier(Pawn pawn)
	{
		if (pawn.InBed() || (pawn.GetPosture() != 0 && !pawn.Downed) || (pawn.IsCaravanMember() && !pawn.GetCaravan().pather.MovingNow) || pawn.InCaravanBed() || pawn.CarriedByCaravan())
		{
			return factor;
		}
		return 1f;
	}
}
