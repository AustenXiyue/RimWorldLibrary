using RimWorld;
using Verse;

namespace BDsArknightLib
{
    public class HediffComp_Stun : HediffComp
    {
        StunHandler stunner => Pawn?.stances?.stunner;

        HediffComp_Disappears hediffComp_Disappears => parent.TryGetComp<HediffComp_Disappears>();

        Thing instigator;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            instigator = dinfo?.Instigator;
            if (stunner != null && hediffComp_Disappears != null)
            {
                stunner.StunFor(hediffComp_Disappears.ticksToDisappear, instigator);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (stunner != null && !stunner.Stunned && hediffComp_Disappears != null)
            {
                stunner.StunFor(hediffComp_Disappears.ticksToDisappear, instigator);
            }
        }

        public override void CompPostPostRemoved()
        {
            if (stunner != null && stunner.Stunned && !stunner.StunFromEMP)
            {
                stunner.StopStun();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref instigator, "instigator", true);
        }
    }
}
