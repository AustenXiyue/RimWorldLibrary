using Verse;

namespace AncotLibrary;

public class Verb_ChargeShoot : Verb_Shoot
{
	private CompWeaponCharge comp => base.EquipmentSource.GetComp<CompWeaponCharge>();

	public override ThingDef Projectile
	{
		get
		{
			if (base.EquipmentSource != null && comp != null && comp.charge > 0)
			{
				return comp.projectileCharged;
			}
			return verbProps.defaultProjectile;
		}
	}

	protected override bool TryCastShot()
	{
		bool flag = base.TryCastShot();
		if (flag && comp != null && comp.charge > 0)
		{
			comp.ChargeFireEffect(caster, base.CurrentTarget.ToTargetInfo(caster.Map));
			comp.UsedOnce();
		}
		return flag;
	}
}
