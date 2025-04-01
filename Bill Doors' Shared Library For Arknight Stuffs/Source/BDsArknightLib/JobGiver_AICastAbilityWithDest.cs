using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace BDsArknightLib
{
    public abstract class JobGiver_AICastAbilityWithDest : ThinkNode_JobGiver
    {
        protected AbilityDef ability;

        private static List<Thing> tmpHostileSpots = new List<Thing>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            Ability ability = pawn.abilities?.GetAbility(this.ability);
            if (ability == null || !ability.CanCast)
            {
                return null;
            }
            if (TryFindTarget(pawn, out var target, GetRange(ability)) && TryFindDest(pawn, out var dest, GetRange(ability)))
            {
                return ability.GetJob(target, dest);
            }
            return null;
        }

        protected virtual float GetRange(Ability ability)
        {
            return ability.verb.verbProps.range;
        }

        protected abstract bool TryFindTarget(Pawn pawn, out LocalTargetInfo target, float maxDistance);

        protected abstract bool TryFindDest(Pawn pawn, out LocalTargetInfo dest, float maxDistance);

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AICastAbilityWithDest obj = (JobGiver_AICastAbilityWithDest)base.DeepCopy(resolve);
            obj.ability = ability;
            return obj;
        }
    }


    public class JobGiver_AICastOnSelfFlee : JobGiver_AICastAbilityWithDest
    {

        private static List<Thing> tmpHostileSpots = new List<Thing>();

        protected override bool TryFindTarget(Pawn pawn, out LocalTargetInfo target, float maxDistance)
        {
            target = pawn;
            return true;
        }

        protected override bool TryFindDest(Pawn pawn, out LocalTargetInfo dest, float maxDistance)
        {
            Ability ability = pawn.abilities?.GetAbility(this.ability);
            tmpHostileSpots.Clear();
            tmpHostileSpots.AddRange(from a in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
                                     where !a.ThreatDisabled(pawn) && ability.verb.CanHitTarget(a.Thing) && ability.CanApplyOn((LocalTargetInfo)a.Thing)
                                     select a.Thing);
            dest = CellFinderLoose.GetFallbackDest(pawn, tmpHostileSpots, maxDistance, 5f, 5f, 20, (IntVec3 c) => ability.verb.ValidateTarget(c, showMessages: false));
            tmpHostileSpots.Clear();
            return dest.IsValid;
        }
    }


    public class JobGiver_AIYeetTarget : JobGiver_AICastAbilityWithDest
    {
        protected override bool TryFindTarget(Pawn pawn, out LocalTargetInfo target, float maxDistance)
        {
            target = LocalTargetInfo.Invalid;
            Ability ab = pawn.abilities?.GetAbility(ability);
            var adjCells = GenAdj.CellsAdjacent8Way(pawn).Where(a => a.InBounds(pawn.Map));
            foreach (var c in adjCells)
            {
                var ts = pawn.Map.thingGrid.ThingsListAt(c);

                if (ts.Any()) ts = ts.Where(t => t is IAttackTarget tgt && ab.verb.CanHitTarget(t) && ab.CanApplyOn(new LocalTargetInfo(t)) && GenHostility.IsActiveThreatTo(tgt, pawn.Faction)).ToList();

                if (ts.Any()) target = ts.RandomElement();
            }
            if (!target.IsValid && (pawn.mindState.lastAttackedTarget.Thing?.HostileTo(pawn) ?? false) && ab.CanApplyOn(pawn.mindState.lastAttackedTarget))
            {
                target = pawn.mindState.lastAttackedTarget;
            }
            else
            {
                var tar = AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, t => ab.CanApplyOn(new LocalTargetInfo(t)));
                if (tar != null) target = new LocalTargetInfo(tar.Thing);
            }
            return target.IsValid;
        }

        protected override bool TryFindDest(Pawn pawn, out LocalTargetInfo dest, float maxDistance)
        {
            dest = CellFinderLoose.GetFleeDest(pawn, new List<Thing> { pawn }, maxDistance);
            return dest.IsValid;
        }
    }
}
