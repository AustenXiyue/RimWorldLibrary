using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BDsArknightLib
{
    public class ModExtension_IceBreaker : DefModExtension
    {
        //This modext is still needed to define the hediffdef
        public HediffDef hediffDef;

        //If no BDFNE_IceBreakingBonus is defined, use modext value instead
        protected float damageMultiplier = 2;

        protected StatDef statDef;

        float DamageMultiplierForThing(Thing weapon)
        {
            if (weapon == null || statDef == null) return damageMultiplier;
            var stat = weapon.GetStatValue(statDef);
            return stat > 0 ? stat : damageMultiplier;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var v in base.ConfigErrors())
            {
                yield return v;
            }
            if (hediffDef == null)
            {
                yield return "ModExtension_DamageHediffs with hediff left empty";
            }
        }



        public void ApplyBonus(ref DamageInfo dinfo, Thing victim)
        {
            if ((victim is ThingWithComps twp && twp.GetComp<CompStunnable>() is CompStunnable compstun && compstun.StunHandler.Stunned)
             || victim is Pawn p && (p.health.hediffSet.TryGetHediff(hediffDef, out _) || (p.stances?.stunner?.Stunned ?? false)))
            {
                dinfo = ApplyBonusFinalize(dinfo, IceDamageUtil.EffectiveWeapon(dinfo.Instigator));
            }
        }

        DamageInfo ApplyBonusFinalize(DamageInfo dinfo, Thing weapon)
        {
            return new DamageInfo(
                dinfo.Def, dinfo.Amount * DamageMultiplierForThing(weapon), dinfo.ArmorPenetrationInt * DamageMultiplierForThing(weapon), dinfo.Angle,
                dinfo.Instigator, dinfo.HitPart, dinfo.Weapon, dinfo.Category, dinfo.IntendedTarget, dinfo.InstigatorGuilty, dinfo.SpawnFilth, dinfo.WeaponQuality, true
                );
        }
    }
}
