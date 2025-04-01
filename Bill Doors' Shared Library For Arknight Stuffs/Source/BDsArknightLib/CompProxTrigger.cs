using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BDsArknightLib
{
    public abstract class CompProxTrigger : ThingComp
    {
        CompProperties_ProxTrigger Props => props as CompProperties_ProxTrigger;
        public override void CompTick()
        {
            Interval();
        }

        public override void CompTickRare()
        {
            Interval();
        }

        public override void CompTickLong()
        {
            Interval();
        }

        public virtual void Interval()
        {
            if (Check())
            {
                DoAction();
                parent.Destroy();
            }
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            GenDraw.DrawRadiusRing(Props.centerCell, Props.radius);
        }

        public virtual bool Check()
        {
            var r = Props.radius * Props.radius;
            var ps = parent.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            foreach (var p in ps)
            {
                if (p.Position.DistanceToSquared(Props.centerCell) < r) return true;
            }
            return false;
        }

        public abstract void DoAction();
    }
    public class CompProperties_ProxTrigger : CompProperties
    {
        public CompProperties_ProxTrigger()
        {
            compClass = typeof(CompProxTrigger);
        }

        public IntVec3 centerCell;

        public float radius = 35;
    }
}
