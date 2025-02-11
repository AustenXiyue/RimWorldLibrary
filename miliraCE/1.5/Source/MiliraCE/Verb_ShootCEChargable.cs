using AncotLibrary;
using CombatExtended;
using Verse;

namespace MiliraCE
{
    public class Verb_ShootCEChargable : Verb_ShootCE
    {
        public CompAmmoUserChargable CompAmmoChargable
        {
            get
            {
                if (!(compAmmo is CompAmmoUserChargable))
                {
                    compAmmo = EquipmentSource?.TryGetComp<CompAmmoUserChargable>();
                }
                return (CompAmmoUserChargable)compAmmo;
            }
        }
        public override CompAmmoUser CompAmmo => CompAmmoChargable;

        private CompWeaponCharge CompWeaponCharge => EquipmentSource?.GetComp<CompWeaponCharge>();

        public override ThingDef Projectile
        {
            get
            {
                if (CompAmmoChargable?.CurrentAmmo != null)
                {
                    return CompAmmoChargable.CurChargableAmmoProjectile;
                }
                if (CompWeaponCharge != null && CompWeaponCharge.charge > 0)
                {
                    return CompWeaponCharge.projectileCharged;
                }
                return base.Projectile;
            }
        }

        public override bool TryCastShot()
        {
            bool success = base.TryCastShot();
            if (success && CompWeaponCharge != null && CompWeaponCharge.charge > 0)
            {
                CompWeaponCharge.ChargeFireEffect(caster, CurrentTarget.ToTargetInfo(caster.Map));
            }
            return success;
        }

        // A somewhat dirty way to consume charge for each pellet fired
        public override void ShiftTarget(ShiftVecReport report, bool calculateMechanicalOnly = false, bool isInstant = false, bool midBurst = false)
        {
            base.ShiftTarget(report, calculateMechanicalOnly, isInstant, midBurst);
            CompWeaponCharge.UsedOnce();
        }
    }
}
