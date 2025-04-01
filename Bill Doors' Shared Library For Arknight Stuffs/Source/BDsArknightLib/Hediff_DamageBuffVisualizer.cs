using Verse;

namespace BDsArknightLib
{
    public class Hediff_DamageBuffVisualizer : HediffWithComps
    {
        public bool ImproperlyRemoved = true;

        public override string Label => GameComponent_DamageBuffTracker.Tracker?.CheckForDamageBuff(pawn).Description ?? "";

        public override string Description => base.Description;

        public override void PostRemoved()
        {
            if (ImproperlyRemoved) RemoveBuff();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            RemoveBuff();
        }

        public override void Notify_Downed()
        {
            RemoveBuff();
        }

        void RemoveBuff()
        {
            GameComponent_DamageBuffTracker.Tracker.DeregisterDamageBuff(pawn);
        }
    }
}
