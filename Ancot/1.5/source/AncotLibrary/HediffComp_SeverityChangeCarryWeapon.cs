using Verse;

namespace AncotLibrary;

public class HediffComp_SeverityChangeCarryWeapon : HediffComp
{
	private HediffCompProperties_SeverityChangeCarryWeapon Props => (HediffCompProperties_SeverityChangeCarryWeapon)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (base.Pawn.IsHashIntervalTick(Props.intervalTicks))
		{
			if (base.Pawn.equipment.Primary != null)
			{
				parent.Severity = Props.severityCarryWeapon;
			}
			else
			{
				parent.Severity = Props.severityDefault;
			}
		}
	}
}
