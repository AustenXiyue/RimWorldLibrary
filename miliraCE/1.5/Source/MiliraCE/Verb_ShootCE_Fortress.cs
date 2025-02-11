using AncotLibrary;
using CombatExtended;
using Milira;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    // TODO: Make a CompAmmoUser-like class to provide fortress with ammo
    // TODO: Make something to get the mech auto-load itself with fortress ammo

    // Copied from Milira.Verb_Shoot_Fortress, but with a different base class
    public class Verb_ShootCE_Fortress : Verb_ShootCE
    {
        private CompThingContainer_Milian CompThingContainer_Milian => caster.TryGetComp<CompThingContainer_Milian>();

        private CompThingCarrier Carrier => CompThingContainer_Milian.ContainedThing.TryGetComp<CompThingCarrier>();

        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }
            return Carrier != null && Carrier.IngredientCount > 20;
        }

        public override bool TryCastShot()
        {
            bool success = base.TryCastShot();
            Vector3 directionNorm = (currentTarget.CenterVector3 - caster.Position.ToVector3()).normalized;
            Find.CameraDriver.shaker.DoShake(2f);
            Building_TurretGunCEFortress building_TurretGunFortress = caster as Building_TurretGunCEFortress;
            float curRotation = building_TurretGunFortress.top.CurRotation;
            for (int i = 0; i < 20; i++)
            {
                AncotFleckMaker.CustomFleckThrow(building_TurretGunFortress.Map, Milira.FleckDefOf.AirPuff, building_TurretGunFortress.Position.ToVector3Shifted(), new Color(0.92f, 0.91f, 0.76f), directionNorm + new Vector3(Rand.Range(-0.05f, 0.05f), 0f, Rand.Range(-0.05f, 0.05f)), Rand.Range(1f, 4f), 0f, curRotation + 180f + Rand.Range(-30f, 30f), Rand.Range(4f, 12f));
            }
            for (int j = 0; j < 8; j++)
            {
                AncotFleckMaker.CustomFleckThrow(building_TurretGunFortress.Map, RimWorld.FleckDefOf.MicroSparksFast, building_TurretGunFortress.Position.ToVector3Shifted(), new Color(0.92f, 0.91f, 0.76f), directionNorm, Rand.Range(1f, 4f), 0f, curRotation + Rand.Range(-10f, 10f), Rand.Range(1f, 3f));
            }
            if (success && CompThingContainer_Milian != null)
            {
                CompThingContainer_Milian.staySec = 20;
            }
            Carrier.TryRemoveThingInCarrier(20);
            return success;
        }
    }
}
