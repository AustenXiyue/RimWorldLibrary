using Verse;

namespace AncotLibrary;

public class HediffComp_TakeDamage_DOT : HediffComp
{
	private HediffCompProperties_TakeDamage_DOT Props => (HediffCompProperties_TakeDamage_DOT)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (base.Pawn != null && base.Pawn.IsHashIntervalTick(Props.ticksBetweenDamage) && !base.Pawn.Dead)
		{
			TakeDamage();
		}
	}

	public virtual void TakeDamage()
	{
		if (Props.damageDef != null)
		{
			DamageInfo dinfo = new DamageInfo(Props.damageDef, Props.damageAmountBase, Props.armorPenetrationBase);
			base.Pawn.TakeDamage(dinfo);
			parent.Severity += Props.severityPerTime;
		}
	}
}
