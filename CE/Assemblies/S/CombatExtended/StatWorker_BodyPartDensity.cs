using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public abstract class StatWorker_BodyPartDensity : StatWorker
{
	public abstract string UnitString { get; }

	public new abstract float GetBaseValueFor(StatRequest req);

	public override bool ShouldShowFor(StatRequest req)
	{
		return base.ShouldShowFor(req) && req.Thing is Pawn;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		float baseValueFor = GetBaseValueFor(req);
		Pawn pawn = (Pawn)req.Thing;
		return baseValueFor * pawn.HealthScale;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("{0}: {1} {2}", "CE_StatsReport_BaseValue".Translate(), GetBaseValueFor(req), UnitString));
		stringBuilder.AppendLine();
		Pawn pawn = (Pawn)req.Thing;
		stringBuilder.AppendLine(string.Format("{0}: x{1}", "StatsReport_HealthMultiplier".Translate(pawn.HealthScale), pawn.HealthScale.ToStringPercent()));
		stringBuilder.AppendLine();
		return stringBuilder.ToString().Trim();
	}
}
