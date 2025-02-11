using RimWorld;
using Verse;

namespace AncotLibrary;

public class HediffComp_RestoreProjectileInterceptor : HediffComp
{
	private HediffCompProperties_RestoreProjectileInterceptor Props => (HediffCompProperties_RestoreProjectileInterceptor)props;

	private CompProjectileInterceptor comp => base.Pawn.TryGetComp<CompProjectileInterceptor>();

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (base.Pawn.IsHashIntervalTick(20) && !(parent.Severity < Props.severityThreshold) && comp.Active && (float)comp.currentHitPoints / (float)comp.HitPointsMax < Props.restoreThreshold)
		{
			comp.currentHitPoints += (int)((float)comp.HitPointsMax * Props.restorePct);
			parent.Severity -= Props.severityCost;
			if (Props.effecter != null)
			{
				Effecter effecter = Props.effecter.Spawn();
				effecter.Trigger(new TargetInfo(base.Pawn.Position, base.Pawn.Map), new TargetInfo(base.Pawn.Position, base.Pawn.Map));
				effecter.Cleanup();
			}
		}
	}
}
