using Verse;

namespace BDsArknightLib
{
    public abstract class DamageBuffWorker : IExposable
    {
        public DamageBuffWorker()
        {
            if (Duration > 0)
            {
                SetDuration(Duration);
            }
        }

        public void SetDuration(int durationTick)
        {
            validUntil = durationTick + Find.TickManager.TicksGame;
        }

        public int Duration = -1;

        public int validUntil = -1;

        public int charges = -1;

        string labelcache = "";

        public abstract float OverrideHeirachy { get; }

        public virtual string Description
        {
            get
            {
                labelcache = "";
                if (validUntil > 0) labelcache += (validUntil - Find.TickManager.TicksGame).TicksToSeconds().ToString("f0") + "sec";
                if (validUntil > 0 && charges > 0) labelcache += "/";
                if (charges > 0) labelcache += "x" + charges.ToString();
                return labelcache;
            }
        }

        public abstract void Apply(ref DamageInfo dinfo);

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref validUntil, "validUntil");
            Scribe_Values.Look(ref charges, "charges");
        }
    }
}
