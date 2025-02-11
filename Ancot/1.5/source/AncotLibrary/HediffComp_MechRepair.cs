using RimWorld;
using Verse;

namespace AncotLibrary;

public class HediffComp_MechRepair : HediffComp
{
	private HediffCompProperties_MechRepair Props => (HediffCompProperties_MechRepair)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (base.Pawn.IsHashIntervalTick(Props.intervalTicks) && MechRepairUtility.CanRepair(base.Pawn) && base.Pawn.needs.energy.CurLevelPercentage > Props.energyThreshold)
		{
			MechRepairUtility.RepairTick(base.Pawn);
			if (base.Pawn.needs.energy != null)
			{
				base.Pawn.needs.energy.CurLevel -= Props.energyPctPerRepair;
			}
		}
	}
}
