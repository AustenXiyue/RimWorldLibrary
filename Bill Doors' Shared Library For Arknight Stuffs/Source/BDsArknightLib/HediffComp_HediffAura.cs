using RimWorld;
using System.Collections.Generic;
using Verse;
using static HarmonyLib.Code;

namespace BDsArknightLib
{
    public class HediffComp_HediffAura : HediffComp_Aura
    {
        HediffCompProperties_HediffAura Props => props as HediffCompProperties_HediffAura;

        protected override void ApplyToTarget(Thing target)
        {
            if (target is Pawn pawn)
            {
                base.ApplyToTarget(pawn);
                if (pawn.Dead || pawn.health == null || (!Props.targetingParameters?.CanTarget(pawn) ?? false))
                {
                    return;
                }

                foreach (var h in Props.hediffs)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(h.hediffDef);
                    if (hediff == null)
                    {
                        hediff = MakeHediff(h, pawn);
                        pawn.health.AddHediff(hediff);
                        hediff.Severity = h.severity;
                    }
                    HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears == null)
                    {
                        Log.Error("HediffComp_GiveHediffsInRange has a hediff in props which does not have a HediffComp_Disappears");
                    }
                    else
                    {
                        hediffComp_Disappears.ticksToDisappear = h.durationTicks;
                    }
                }
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
            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link != null)
            {
                hediffComp_Link.drawConnection = true;
                hediffComp_Link.other = parent.pawn;
            }
            return hediff;
        }
    }

    public class HediffCompProperties_HediffAura : HediffCompProperties_Aura
    {
        public HediffCompProperties_HediffAura()
        {
            compClass = typeof(HediffComp_HediffAura);
        }

        public TargetingParameters targetingParameters;

        public List<GiveHediffInfo> hediffs;
    }
}
