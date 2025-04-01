using UnityEngine;
using Verse;
using System.IO;
using System.Reflection;
using System;
using CombatExtended;
using zhuzi.AdvancedEnergy.Shields;
using System.Collections.Generic;
using CombatExtended.Compatibility;
using Verse.Noise;
using zhuzi.AdvancedEnergy.Shields.Shields;
using System.Linq;
using RimWorld;
using HarmonyLib;

namespace cn.zhuzijun.EnergyShieldCECompat
{
    public class ZModSettings : ModSettings
    {
        public static bool forceBlock = false;
        public static bool debugMode = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref forceBlock, "forceBlock",  false,true);
            Scribe_Values.Look(ref debugMode, "debugMode", false, true);

        }

        //string buffer1;
        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);
            list.CheckboxLabeled("EnergyShieldCECompat_forceBlock".Translate(), ref forceBlock, "EnergyShieldCECompat_forceBlock_tooltip".Translate());
            list.CheckboxLabeled("EnergyShieldCECompat_debugMode".Translate(), ref debugMode);
            list.End();
        }

    }
    public class ZMod : Mod
    {
        public static ZModSettings settings = new ZModSettings();

        public ModContentPack Pack { get; }

        public override string SettingsCategory() => Pack.Name;

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
        Harmony harmony;
        public ZMod(ModContentPack content) : base(content)
        {
            Pack = content;
            settings = GetSettings<ZModSettings>();
            DoCompat();
        }

        //see : CombatExtended.Compatibility.VanillaExpandedFramework
        private void DoCompat()
        {
            if (!ModLister.HasActiveModWithName("Combat Extended"))
                return;
            if (!ModLister.HasActiveModWithName("EnergyShield 能量护盾"))
                return;

            harmony = new Harmony("cn.zhuzijun.EnergyShieldCECompat");
            harmony.PatchAll();
            BlockerRegistry.RegisterCheckForCollisionCallback(new Func<ProjectileCE, IntVec3, Thing, bool>(CheckIntercept));
            BlockerRegistry.RegisterShieldZonesCallback(new Func<Thing, IEnumerable<IEnumerable<IntVec3>>>(ShieldZonesCallback));
            BlockerRegistry.RegisterImpactSomethingCallback(new Func<ProjectileCE, Thing, bool>(ImpactSomethingCallback));
        }

        /// <summary>
        /// 把护盾范围加入CE火力压制的掩护列表
        /// </summary>
        /// <param name="pawnToSuppress"></param>
        /// <returns></returns>
        private IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing pawnToSuppress)
        {
            IEnumerable<Building_Shield> _ShieldBuildings = pawnToSuppress.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            List<IEnumerable<IntVec3>> list = new List<IEnumerable<IntVec3>>();

            foreach (Building_Shield shield in _ShieldBuildings)
            {
                var shield_comp = shield.TryGetComp<Comp_ShieldGenerator>();
                if (shield_comp != null && shield_comp.ShieldOnline && shield_comp.ShieldDefenceBulletActive)
                    list.Add(GenRadial.RadialCellsAround(shield.Position, shield_comp.ShieldRadius_Current, true));
            }
            return list;

        }

        // Token: 0x06000AD1 RID: 2769 RVA: 0x0005E644 File Offset: 0x0005C844
        public static bool ImpactSomethingCallback(ProjectileCE projectile, Thing launcher)
        {
            bool flyOverhead = projectile.def.projectile.flyOverhead;
            Map map = projectile.Map;
            IEnumerable<Building_Shield> _ShieldBuildings = projectile.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            foreach (Building_Shield shield in _ShieldBuildings)
            {
                var shield_comp = shield.TryGetComp<Comp_ShieldGenerator>();
                ThingDef def = projectile.def;
                Vector3 p = projectile.LastPos.Yto0();
                Vector3 p2 = projectile.ExactPosition.Yto0();
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
                        projectile.ExactPosition = array.OrderBy(((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude)).First<Vector3>();
                        projectile.landed = true;
                        projectile.InterceptProjectile(shield, projectile.ExactPosition, true);
                        //扣除护盾
                        //compShieldField.AbsorbDamage(projectile.DamageAmount, projectile.def.projectile.damageDef, launcher);

                        float dmg = Math.Min(projectile.DamageAmount, 666);
                        if (projectile.def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            dmg *= shield_comp.ShieldHurtRate_EMP;
                        }
                        else if (projectile.def.projectile.damageDef == DamageDefOf.Flame)
                            dmg *= shield_comp.ShieldHurtRate_Flame;

                        dmg = Mathf.Max(0, dmg * shield_comp.ShieldHurtRate);
                        float explosionRadius = 0;
                        //判定为爆炸弹丸
                        if (projectile.def.projectile.explosionRadius != 0)
                        {
                            //受到伤害取决于弹丸威力*伤害半径^0.66
                            dmg *= (float)Math.Pow(Math.Min(projectile.def.projectile.explosionRadius, 10), 0.66f) * shield_comp.ShieldHurtRate_AOE;
                            explosionRadius = projectile.def.projectile.explosionRadius * 0.66f;
                        }

                        if (projectile.def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            if (projectile.def.projectile.explosionRadius != 0)
                                dmg += shield_comp.ShieldEnergyCurrent * shield_comp.ShieldHurtRateExtra_EMP;
                        }
                        shield_comp.costShield(dmg, projectile.ExactPosition.ToIntVec3(), explosionRadius);
                        FleckMakerCE.ThrowLightningGlow(projectile.ExactPosition, map, 0.5f);

                        return true;
                    }
                }
            }
            return false;
        }
        private static bool CheckIntercept(ProjectileCE projectile, IntVec3 cell, Thing launcher)
        {
            bool flyOverhead = projectile.def.projectile.flyOverhead;
            

            IEnumerable<Building_Shield> _ShieldBuildings = projectile.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            foreach (Building_Shield shield in _ShieldBuildings)
            {
                var shield_comp = shield.TryGetComp<Comp_ShieldGenerator>();
                ThingDef def = projectile.def;
                Vector3 p = projectile.LastPos.Yto0();
                Vector3 p2 = projectile.ExactPosition.Yto0();
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

                    if (flyOverhead) {
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
                        projectile.ExactPosition = array.OrderBy(((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude)).First<Vector3>();
                        projectile.landed = true;
                        projectile.InterceptProjectile(shield, projectile.ExactPosition, true);
                        //扣除护盾
                        //compShieldField.AbsorbDamage(projectile.DamageAmount, projectile.def.projectile.damageDef, launcher);

                        float dmg = Math.Min(projectile.DamageAmount, 666);
                        if (projectile.def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            dmg *= shield_comp.ShieldHurtRate_EMP;
                        }
                        else if (projectile.def.projectile.damageDef == DamageDefOf.Flame)
                            dmg *= shield_comp.ShieldHurtRate_Flame;

                        dmg = Mathf.Max(0, dmg * shield_comp.ShieldHurtRate);
                        float explosionRadius = 0;
                        //判定为爆炸弹丸
                        if (projectile.def.projectile.explosionRadius != 0)
                        {
                            //受到伤害取决于弹丸威力*伤害半径^0.66
                            dmg *= (float)Math.Pow(Math.Min(projectile.def.projectile.explosionRadius, 10), 0.66f) * shield_comp.ShieldHurtRate_AOE;
                            explosionRadius = projectile.def.projectile.explosionRadius * 0.66f;
                        }

                        if (projectile.def.projectile.damageDef == DamageDefOf.EMP)
                        {
                            if (projectile.def.projectile.explosionRadius != 0)
                                dmg += shield_comp.ShieldEnergyCurrent * shield_comp.ShieldHurtRateExtra_EMP;
                        }
                        shield_comp.costShield(dmg, projectile.ExactPosition.ToIntVec3(), explosionRadius);

                        return true;
                    }

                }
            }
            return false;
        }




    }
}
