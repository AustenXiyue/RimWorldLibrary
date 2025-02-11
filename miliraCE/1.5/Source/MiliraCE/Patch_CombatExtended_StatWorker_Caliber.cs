using AncotLibrary;
using CombatExtended;
using CombatExtended.Compatibility;
using HarmonyLib;
using RimWorld;
using System.Text;
using Verse;

namespace MiliraCE
{
    [HarmonyPatch(typeof(StatWorker_Caliber))]
    [HarmonyPatch(nameof(StatWorker_Caliber.GetExplanationUnfinalized))]
    public static class Patch_CombatExtended_StatWorker_Caliber_GetExplanationUnfinalized
    {
        // Copied from CombatExtended.StatWorker_Caliber.GunDef
        private static ThingDef GunDef(StatRequest req)
        {
            ThingDef thingDef = req.Def as ThingDef;
            if ((thingDef?.building?.IsTurret).GetValueOrDefault())
            {
                thingDef = thingDef.building.turretGunDef;
            }
            return thingDef;
        }

        // Copied from CombatExtended.StatWorker_Caliber.Gun
        private static Thing Gun(StatRequest req)
        {
            return (req.Thing as Building_Turret)?.GetGun() ?? req.Thing;
        }

        // Copied from CombatExtended.StatWorker_Caliber.ShouldDisplayAmmoSet
        private static bool ShouldDisplayAmmoSet(AmmoSetDef ammoSet)
        {
            return ammoSet != null && AmmoUtility.IsAmmoSystemActive(ammoSet);
        }

        internal static void Postfix(ref string __result, StatRequest req)
        {
            ThingDef gunDef = GunDef(req);
            AmmoSetDef ammoSetDef = gunDef?.GetCompProperties<CompProperties_AmmoUserChargable>()?.chargedAmmoSet;
            if (ShouldDisplayAmmoSet(ammoSetDef))
            {
                StringBuilder stringBuilder = new StringBuilder("\n\n");
                stringBuilder.AppendLine("CE_MiliraAmmoWhenCharged".Translate());
                stringBuilder.AppendLine();

                Thing gun = Gun(req);
                foreach (AmmoLink ammoType in ammoSetDef.ammoTypes)
                {
                    string text = (string.IsNullOrEmpty(ammoType.ammo.ammoClass.LabelCapShort) ? ((string)ammoType.ammo.ammoClass.LabelCap) : ammoType.ammo.ammoClass.LabelCapShort);
                    stringBuilder.AppendLine(text + ":\n" + ammoType.projectile.GetProjectileReadout(gun));
                }
                __result += stringBuilder.ToString().TrimEndNewlines();
            }
            else
            {
                CompProperties_WeaponCharge propWeaponCharge = gunDef?.GetCompProperties<CompProperties_WeaponCharge>();
                if (propWeaponCharge != null)
                {
                    Thing gun = Gun(req);
                    ThingDef projectile = propWeaponCharge.projectileCharged;
                    StringBuilder stringBuilder = new StringBuilder("\n\n");
                    stringBuilder.AppendLine("CE_MiliraAmmoWhenCharged".Translate());
                    stringBuilder.AppendLine(projectile.LabelCap + ":\n" + projectile.GetProjectileReadout(gun));
                    __result += stringBuilder.ToString().TrimEndNewlines();
                }
            }
        }
    }
}
