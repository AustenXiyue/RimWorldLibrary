using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cn.zhuzijun.EnergyShieldCECompat;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using zhuzi.AdvancedEnergy.Shields.Shields;

namespace EnergyShieldCECompat
{

    [HarmonyPatch(typeof(ProjectileCE))]
    internal class PatchProjectileCE
    {
        [HarmonyPostfix]
        [HarmonyPatch("Tick")]
        public static void TickPostfix(ProjectileCE __instance)
        {
            if (!ZModSettings.forceBlock) return;
            Thing launcher = __instance.launcher;

            ThingDef def = __instance.def;
            bool flyOverhead = def.projectile.flyOverhead;
            Map map = __instance.Map;
            if (map == null || map.listerBuildings == null) return;
            IEnumerable<Building_Shield> _ShieldBuildings = map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            Vector3 p = __instance.LastPos;
            if (p == null)
                p = __instance.Position.ToVector3();
            p = p.Yto0();
            Vector3 p2 = __instance.ExactPosition;
            if (p2 == null)
                p2 = __instance.Position.ToVector3();
            foreach (Building_Shield shield in _ShieldBuildings)
            {
                var shield_comp = shield.TryGetComp<Comp_ShieldGenerator>();

                p2 =p2.Yto0();
                if (shield_comp != null && shield_comp.ShieldOnline)
                {
                    //敌我识别
                    if (shield_comp.ShieldDefenceIFFActive)
                    {
                        //不抵挡无来源的
                        if (launcher == null || launcher.Faction == null)
                            continue;
                        //不抵挡中立和友军
                        if (launcher.Faction.AllyOrNeutralTo(shield.Faction))
                            continue;
                    }
                    if (flyOverhead)
                    {
                        if (!shield_comp.ShieldDefenceProjectileActive)
                            continue;
                    }
                    else
                    {
                        if (!shield_comp.ShieldDefenceBulletActive)
                            continue;
                    }
                    //需要抵挡,判断距离
                    Vector3 center = shield.Position.ToVector3Shifted().Yto0();
                    float shieldRadius = shield_comp.ShieldRadius_Current;
                    Vector3[] array;
                    bool flag3 = !CE_Utility.IntersectionPoint(p, p2, center, shieldRadius, out array, false, false, null);
                    if (!flag3)
                    {
                        __instance.ExactPosition = array.OrderBy(((Vector3 x) => (__instance.OriginIV3.ToVector3() - x).sqrMagnitude)).First<Vector3>();
                        __instance.landed = true;
                        __instance.InterceptProjectile(shield, __instance.ExactPosition, true);
                        //扣除护盾
                        //compShieldField.AbsorbDamage(projectile.DamageAmount, projectile.def.projectile.damageDef, launcher);

                        float dmg = Math.Min(__instance.DamageAmount, 666);
                        if (def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            dmg *= shield_comp.ShieldHurtRate_EMP;
                        }
                        else if (def.projectile.damageDef == DamageDefOf.Flame)
                            dmg *= shield_comp.ShieldHurtRate_Flame;

                        dmg = Mathf.Max(0, dmg * shield_comp.ShieldHurtRate);
                        float explosionRadius = 0;
                        //判定为爆炸弹丸
                        if (def.projectile.explosionRadius != 0)
                        {
                            //受到伤害取决于弹丸威力*伤害半径^0.66
                            dmg *= (float)Math.Pow(Math.Min(def.projectile.explosionRadius, 10), 0.66f) * shield_comp.ShieldHurtRate_AOE;
                            explosionRadius = def.projectile.explosionRadius * 0.66f;
                        }

                        if (def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            if (def.projectile.explosionRadius != 0)
                                dmg += shield_comp.ShieldEnergyCurrent * shield_comp.ShieldHurtRateExtra_EMP;
                        }
                        shield_comp.costShield(dmg, __instance.ExactPosition.ToIntVec3(), explosionRadius);
                        FleckMakerCE.ThrowLightningGlow(__instance.ExactPosition, map, 0.5f);

                        return;
                    }
                }
            }
            return;
        }


    }
}
