using RimWorld;
using System.Collections.Generic;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BDsArknightLib
{
    public class CompAbilityEffect_GiveHediffAOE : CompAbilityEffect_AreaOfEffect
    {
        public new CompProperties_AbilityGiveHediffAOE Props => (CompProperties_AbilityGiveHediffAOE)props;

        protected override bool ExtraValidator(Thing thing)
        {
            if (thing is Pawn p)
            {
                if (p.Faction == targetedFaction) return true;

                if (p.HostileTo(Pawn))
                {
                    if (Props.toHostile && !p.ThreatDisabled(Pawn)) return true;
                }
                else if (Props.toFriendly)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ApplyPerThing(Thing thing)
        {
            if (thing is Pawn p)
            {
                base.ApplyPerThing(thing);
                ApplyInner(p);
            }
        }

        protected virtual void ApplyInner(Pawn target)
        {
            if (target == null)
            {
                return;
            }
            if (TryResist(target))
            {
                MoteMaker.ThrowText(target.DrawPos, target.Map, "Resisted".Translate());
                return;
            }

            foreach (var hediffInfo in Props.hediffs)
            {
                Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(hediffInfo.hediffDef);
                if (firstHediffOfDef != null)
                {
                    if (Props.replaceExisting)
                    {
                        target.health.RemoveHediff(firstHediffOfDef);
                    }
                    else
                    {
                        if (Props.severityAccumulative)
                        {
                            firstHediffOfDef.Severity += hediffInfo.severity;
                        }
                        if (Props.durationAccumulative)
                        {
                            var h = firstHediffOfDef.TryGetComp<HediffComp_Disappears>();
                            if (h != null) h.ticksToDisappear += hediffInfo.durationTicks;
                        }
                        continue;
                    }
                }
                target.health.AddHediff(MakeHediff(hediffInfo, target));
                Props.thingEffecter?.Spawn(target, Map);
            }
        }

        protected virtual Hediff MakeHediff(GiveHediffInfo hediffInfo, Pawn target)
        {
            Hediff hediff = HediffMaker.MakeHediff(hediffInfo.hediffDef, target, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.ticksToDisappear = hediffInfo.durationTicks;
            }
            hediff.Severity = hediffInfo.severity;
            return hediff;
        }

        protected virtual bool TryResist(Pawn pawn)
        {
            return Props.resistanceStat != null && Rand.Chance(pawn.GetStatValue(Props.resistanceStat));
        }
    }
    public class CompProperties_AbilityGiveHediffAOE : CompProperties_AbilityAreaOfEffect
    {
        public CompProperties_AbilityGiveHediffAOE()
        {
            compClass = typeof(CompAbilityEffect_GiveHediffAOE);
        }
        public List<GiveHediffInfo> hediffs = new List<GiveHediffInfo>();

        public bool durationAccumulative = false;
        public bool severityAccumulative = false;

        public bool replaceExisting = true;

        public bool toHostile = true;

        public bool toFriendly = false;

        public StatDef resistanceStat;
    }

    public class GiveHediffInfo
    {
        public HediffDef hediffDef;

        public int durationTicks = 180;

        public float severity = 0.1f;
    }
}
