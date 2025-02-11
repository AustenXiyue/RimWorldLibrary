using AncotLibrary;
using CombatExtended;
using System.Linq;
using Verse;

namespace MiliraCE
{
    public class CompProperties_AmmoUserChargable : CompProperties_AmmoUser
    {
        public AmmoSetDef chargedAmmoSet = null;

        public CompProperties_AmmoUserChargable()
        {
            compClass = typeof(CompAmmoUserChargable);
        }
    }

    public class CompAmmoUserChargable : CompAmmoUser
    {
        public new CompProperties_AmmoUserChargable Props => (CompProperties_AmmoUserChargable)props;
        private CompWeaponCharge CompWeaponCharge => parent.GetComp<CompWeaponCharge>();

        public ThingDef CurChargableAmmoProjectile
        {
            get
            {
                AmmoSetDef ammoSetDef;
                if (parent != null && CompWeaponCharge != null && CompWeaponCharge.charge > 0)
                {
                    ammoSetDef = Props.chargedAmmoSet;
                }
                else
                {
                    ammoSetDef = Props.ammoSet;
                }
                return ammoSetDef?.ammoTypes?.FirstOrDefault((AmmoLink x) => x.ammo == CurrentAmmo)?.projectile ?? parent.def.Verbs.FirstOrDefault().defaultProjectile;
            }
        }
    }
}
