using Verse;

namespace AncotLibrary;

public class HediffComp_SeverityFollowHediff : HediffComp
{
	private HediffCompProperties_SeverityFollowHediff Props => (HediffCompProperties_SeverityFollowHediff)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (base.Pawn.IsHashIntervalTick(Props.intervalTicks))
		{
			Hediff firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
			if (firstHediffOfDef != null)
			{
				parent.Severity = base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff).Severity;
			}
			else
			{
				parent.Severity = Props.defaultSeverity;
			}
		}
	}
}
