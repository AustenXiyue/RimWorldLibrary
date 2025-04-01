using UnityEngine;
using Verse;

namespace BDsArknightLib
{
    public class DamageBuffWorker_Pct : DamageBuffWorker
    {
        public float pct;

        public override float OverrideHeirachy => pct;

        public override string Description
        {
            get
            {
                var labelcache = base.Description;
                if (pct > 0) labelcache += "+";
                labelcache += $"{pct * 100:f0}% ";
                return labelcache;
            }
        }

        public override void Apply(ref DamageInfo dinfo)
        {
            dinfo.SetAmount(dinfo.Amount * (1 + pct));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref pct, "pct");
        }
    }
}
