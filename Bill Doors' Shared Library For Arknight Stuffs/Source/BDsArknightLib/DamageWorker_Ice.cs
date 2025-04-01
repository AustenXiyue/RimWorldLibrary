using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BDsArknightLib
{
    public class ModExtension_DamageIFF : DefModExtension
    {
        public bool playerUsable = true;
    };

    public class ModExtension_DamageHediffs : DefModExtension
    {
        public HediffDef hediffDef;

        public HediffDef secondHediffDef;

        public float duration = 10;
        public float secondDuration = 10;

        public StatDef chanceStat;

        public StatDef resistStat;

        public StatDef durationStat;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var v in base.ConfigErrors())
            {
                yield return v;
            }
            if (hediffDef == null || secondHediffDef == null)
            {
                yield return "ModExtension_DamageHediffs with hediff left empty";
            }
        }
    }


    public class DamageWorker_ExplosionIFF : DamageWorker_AddInjury
    {
        ModExtension_DamageIFF IFFext => def.GetModExtension<ModExtension_DamageIFF>();
        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            bool IFF = IFFext != null;
            if (IFF && !IFFext.playerUsable && explosion.instigator.Faction.IsPlayer) IFF = false;
            if (IFF && t.Faction != null && !((explosion.instigator is Pawn && !explosion.instigator.Destroyed) ? explosion.instigator.HostileTo(t) : explosion.instigator.Faction.HostileTo(t.Faction)))
            {
                return;
            }
            base.ExplosionDamageThing(explosion, t, damagedThings, ignoredThings, cell);
        }
    }

    //Move damage bonus to harmony patch
    public class DamageWorker_Ice : DamageWorker_ExplosionIFF
    {
        ModExtension_DamageHediffs ext => def.GetModExtension<ModExtension_DamageHediffs>();

        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * cellsToAffect.Count);
            ExplosionVisualEffectCenter(explosion);
        }

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
            float num2 = 1f - lengthHorizontal / explosion.radius;
            if (num2 > 0f)
            {
                explosion.Map.snowGrid.AddDepth(c, num2 * def.explosionSnowMeltAmount);
            }
        }

        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            if (!(t is Pawn || t is Plant || t is Fire || t is Building))
            {
                return;
            }
            base.ExplosionDamageThing(explosion, t, damagedThings, ignoredThings, cell);
        }

        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult result = new DamageResult();

            //Plant takes damage normally, instantly damage fire
            if (victim is Plant)
            {
                return base.Apply(dinfo, victim);
            }
            if (victim is Fire)
            {
                victim.Destroy();
                return result;
            }

            if (victim is Pawn p)
            {
                //Temperature multiplier
                float temperature = dinfo.ArmorPenetrationInt * -100;
                float multiplier = (temperature - p.ComfortableTemperatureRange().min) / temperature;
                if (multiplier < 0f) return result;
                if (multiplier > 1f) multiplier = 1;
                dinfo.SetAmount(dinfo.Amount * multiplier);
                if (ext.durationStat != null) multiplier *= IceDamageUtil.EffectiveWeapon(dinfo.Instigator).GetStatValue(ext.durationStat);
                if (!IceDamageUtil.TryApply(dinfo.Instigator, ext) || IceDamageUtil.TryResist(victim, ext)) return base.Apply(dinfo, victim);

                if (ext != null && ext.hediffDef != null && ext.secondHediffDef != null)
                {
                    //If slowed, remove slowed and add frozen
                    if (p.health.hediffSet.TryGetHediff(ext.hediffDef, out Hediff existing))
                    {
                        p.health.RemoveHediff(existing);
                        p.stances?.stunner?.StunFor((ext.secondDuration * multiplier).SecondsToTicks(), dinfo.Instigator, disableRotation: true);
                        var hediff = p.health.AddHediff(ext.secondHediffDef, dinfo: dinfo);
                        HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                        if (hediffComp_Disappears != null)
                        {
                            hediffComp_Disappears.ticksToDisappear = (ext.secondDuration * multiplier).SecondsToTicks();
                        }
                    }
                    //If not frozen, apply slowed. Otherwise one'll be both slowed and frozen
                    else if (!p.health.hediffSet.TryGetHediff(ext.secondHediffDef, out _))
                    {
                        var hediff = p.health.AddHediff(ext.hediffDef, dinfo: dinfo);
                        HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                        if (hediffComp_Disappears != null)
                        {
                            hediffComp_Disappears.ticksToDisappear = (ext.duration * multiplier).SecondsToTicks();
                        }
                    }
                }
                return base.Apply(dinfo, victim);
            }
            //Other things takes damage relative to flammability. Flammable buildings takes 1.5x damage, non-flammable takes 0.5x
            dinfo.SetAmount(dinfo.Amount * (victim.GetStatValue(StatDefOf.Flammability) + 0.5f));
            return base.Apply(dinfo, victim);
        }
    }

    public static class IceDamageUtil
    {
        public static float ApplyChanceOverride = -1;
        //Assume the weapon held by the instigator pawn is the weapon caused this damage, since dinfo only passes weapon as a def, and I might make BDFNE_IceBreakingBonus scale with quality
        //If no weapon is held, use instigator as weapon
        public static Thing EffectiveWeapon(Thing Instigator)
        {
            if (Instigator is Pawn p)
            {
                return p.equipment.Primary ?? p;
            }
            return Instigator;
        }

        public static bool TryApply(Thing instigator, ModExtension_DamageHediffs ext)
        {
            if (ext == null || ext.chanceStat == null || instigator == null) return true;
            if (ApplyChanceOverride > 0)
            {
                bool b = Rand.Chance(ApplyChanceOverride);
                ApplyChanceOverride = -1;
                return b;
            }

            return Rand.Chance(EffectiveWeapon(instigator).GetStatValue(ext.chanceStat));
        }

        public static bool TryResist(Thing victim, ModExtension_DamageHediffs ext)
        {
            if (ext == null || ext.resistStat == null || victim == null) return false;
            return Rand.Chance(victim.GetStatValue(ext.resistStat));
        }
    }
}
