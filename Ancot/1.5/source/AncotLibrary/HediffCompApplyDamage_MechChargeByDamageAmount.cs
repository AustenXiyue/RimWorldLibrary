using Verse;

namespace AncotLibrary;

public class HediffCompApplyDamage_MechChargeByDamageAmount : HediffComp
{
	public bool available = true;

	public int ticks = 0;

	private HediffCompProperties_ApplyDamage_MechChargeByDamageAmount Props => (HediffCompProperties_ApplyDamage_MechChargeByDamageAmount)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (!available)
		{
			ticks++;
			if (ticks > Props.cooldownTick)
			{
				available = true;
				ticks = 0;
			}
		}
	}

	public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if ((Props.armorCategory.Contains(dinfo.Def.armorCategory) || Props.damageDefs.Contains(dinfo.Def)) && base.Pawn.needs.energy != null && available)
		{
			base.Pawn.needs.energy.CurLevel += Props.energyPerDMGTaken * dinfo.Amount;
			available = false;
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref available, "available", defaultValue: false);
		Scribe_Values.Look(ref ticks, "ticks", 0);
	}
}
