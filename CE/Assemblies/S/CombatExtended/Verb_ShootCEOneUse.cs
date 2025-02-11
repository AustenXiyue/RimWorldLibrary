using Verse;

namespace CombatExtended;

public class Verb_ShootCEOneUse : Verb_ShootCE
{
	public override bool TryCastShot()
	{
		if (base.TryCastShot())
		{
			if (burstShotsLeft <= 1)
			{
				SelfConsume();
			}
			return true;
		}
		if (CompAmmo != null && CompAmmo.HasMagazine && CompAmmo.CurMagCount <= 0)
		{
			SelfConsume();
		}
		else if (burstShotsLeft < verbProps.burstShotCount)
		{
			SelfConsume();
		}
		return false;
	}

	public override void Notify_EquipmentLost()
	{
		if (state == VerbState.Bursting && burstShotsLeft < verbProps.burstShotCount)
		{
			SelfConsume();
		}
	}

	private void SelfConsume()
	{
		CompInventory compInventory = base.ShooterPawn?.TryGetComp<CompInventory>();
		ThingWithComps equipmentSource = base.EquipmentSource;
		if (equipmentSource != null && !equipmentSource.Destroyed)
		{
			base.EquipmentSource.Destroy();
		}
		if (compInventory != null && base.ShooterPawn?.jobs.curJob?.def != CE_JobDefOf.OpportunisticAttack)
		{
			ThingWithComps thingWithComps = compInventory.rangedWeaponList?.FirstOrDefault((ThingWithComps t) => t.def == base.EquipmentSource?.def);
			if (thingWithComps != null)
			{
				compInventory.TrySwitchToWeapon(thingWithComps);
			}
			else
			{
				compInventory.SwitchToNextViableWeapon();
			}
		}
	}
}
