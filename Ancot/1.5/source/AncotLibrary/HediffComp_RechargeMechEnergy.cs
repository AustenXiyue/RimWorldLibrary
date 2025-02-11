using Verse;

namespace AncotLibrary;

public class HediffComp_RechargeMechEnergy : HediffComp
{
	private HediffCompProperties_RechargeMechEnergy Props => (HediffCompProperties_RechargeMechEnergy)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (base.Pawn.IsHashIntervalTick(Props.intervalTicks) && base.Pawn.needs.energy != null && Available())
		{
			base.Pawn.needs.energy.CurLevel += Props.energyPerCharge;
		}
	}

	public bool Available()
	{
		if (Props.onlyDormant)
		{
			return base.Pawn.CurJob.forceSleep;
		}
		return true;
	}
}
