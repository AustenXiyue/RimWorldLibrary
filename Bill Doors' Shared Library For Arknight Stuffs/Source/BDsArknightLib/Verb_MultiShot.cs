using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BDsArknightLib
{
    public class Verb_MultiShot : Verb_Shoot
    {
        List<LocalTargetInfo> targets = new List<LocalTargetInfo>();

        float extraShotCount => EquipmentSource.GetStatValue(BDAKN_DefOf.BDAKN_MultiShotCount);

        LocalTargetInfo originalTarget = LocalTargetInfo.Invalid;
        protected IAttackTargetSearcher searcher => caster as IAttackTargetSearcher;

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Retarget();
        }

        protected override bool TryCastShot()
        {
            bool b = base.TryCastShot();
            originalTarget = currentTarget;
            if (burstShotsLeft > 0 && !EnsureAllTargValid())
            {
                Retarget();
            }
            for (int i = 0; i < Math.Min(targets.Count, extraShotCount); i++)
            {
                currentTarget = targets[i];
                base.TryCastShot();
            }
            currentTarget = originalTarget;
            return b;
        }

        void Retarget()
        {
            targets.Clear();
            var tgts = caster.Map.attackTargetsCache.GetPotentialTargetsFor(searcher).Where(t => CanHitTarget(t.Thing) && !t.ThreatDisabled(searcher)).Select(t => t.Thing).ToList();
            if (originalTarget.HasThing)
            {
                tgts.Remove(originalTarget.Thing);
            }
            float f = extraShotCount;
            Thing thing = null;
            while (tgts.Any() && f >= 1)
            {
                f--;
                thing = tgts.RandomElementByWeight(ts => 1 / Weight(ts));
                tgts.Remove(thing);
                targets.Add(thing);
            }
            if (tgts.Any() && f > 0 && Rand.Chance(f))
            {
                thing = tgts.RandomElementByWeight(ts => 1 / Weight(ts));
                tgts.Remove(thing);
                targets.Add(thing);
            }
        }

        bool EnsureAllTargValid()
        {
            if (targets.NullOrEmpty()) return false;
            foreach (var t in targets)
            {
                if (!CanHitTarget(t)) return false;
                if (t.Thing is IAttackTarget ia && ia.ThreatDisabled(searcher)) return false;
            }
            return true;
        }

        protected virtual float Weight(Thing thing)
        {
            return thing.Position.DistanceToSquared(originalTarget.Cell);
        }
    }
}
