using Verse;
using BillDoorsFramework;
using System.Collections.Generic;
using BDsArknightLib;
using Verse.AI;
using UnityEngine;

namespace BDsNydiaExp
{
    public class HediffComp_Explosive : HediffComp
    {
        HediffCompProperties_Explosive Props => props as HediffCompProperties_Explosive;

        public Thing instigator;

        public void SetInstigator(Thing instigator)
        {
            this.instigator = instigator;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (instigator == null || dinfo.Instigator == instigator)
            {
                if (dinfo.Def == Props.explosionData.explosiveDamageType) return;

                if (Props.detonateRule != null && Props.detonateLog != null)
                {
                    Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(Pawn, Props.detonateRule, dinfo.Instigator) { def = Props.detonateLog });
                }

                Props.explosionData.Detonate(Pawn.MapHeld, Pawn.PositionHeld, instigator, new List<Thing>() { instigator });
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref instigator, "instigator", true);
        }
    }

    public class HediffCompProperties_Explosive : HediffCompProperties
    {
        public HediffCompProperties_Explosive()
        {
            compClass = typeof(HediffComp_Explosive);
        }

        public ExplosionData explosionData;

        public LogEntryDef detonateLog;
        public RulePackDef detonateRule;
    }

    public class CompAbilityEffect_NydiaDeathSentence : CompAbilityEffect_GiveHediffAOE
    {
        protected override Hediff MakeHediff(GiveHediffInfo hediffInfo, Pawn target)
        {
            var h = base.MakeHediff(hediffInfo, target);
            if (h.TryGetComp<HediffComp_Explosive>() is HediffComp_Explosive exp)
            {
                exp.instigator = Pawn;
            }
            return h;
        }
    }

    public class HediffComp_BlackFeatherAura : HediffComp_HediffAura
    {
        HediffCompProperties_HediffAura Props => props as HediffCompProperties_HediffAura;

        protected override bool Validator(IAttackTarget target)
        {
            return base.Validator(target) && target.Thing is Pawn pawn && CheckHediffs(pawn);
        }

        bool CheckHediffs(Pawn p)
        {
            foreach (var h in Props.hediffs)
            {
                if (!p.health.hediffSet.HasHediff(h.hediffDef)) return true;
            }
            return false;
        }

        protected override Hediff MakeHediff(GiveHediffInfo hediffInfo, Pawn target)
        {
            Hediff hediff = base.MakeHediff(hediffInfo, target);

            if (hediff.TryGetComp<HediffComp_Explosive>() is HediffComp_Explosive exp)
            {
                exp.instigator = Pawn;
            }

            return hediff;
        }
    }
}
