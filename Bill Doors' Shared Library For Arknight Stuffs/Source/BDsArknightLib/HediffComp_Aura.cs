using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.Scripting.GarbageCollector;

namespace BDsArknightLib
{
    public abstract class HediffComp_Aura : HediffComp
    {
        HediffCompProperties_Aura Props => (HediffCompProperties_Aura)props;

        protected Mote mote;

        protected List<IAttackTarget> TargetCache = new List<IAttackTarget>();

        protected virtual IEnumerable<IAttackTarget> PotentialTargets
        {
            get
            {
                if (Pawn.InAggroMentalState) return Pawn.Map.mapPawns.AllPawnsSpawned.Select(p => p as IAttackTarget);
                if (Props.toSameFaction) return parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction).Where(Validator);
                if (Props.toHostile && !Props.toFriendly)
                {
                    return Pawn.Map.attackTargetsCache.GetPotentialTargetsFor(Pawn).Where(t => !t.ThreatDisabled(Pawn) && Validator(t));
                }
                return Pawn.Map.mapPawns.AllPawnsSpawned.Select(p => p as IAttackTarget).Where(Validator);
            }
        }

        protected virtual int MaxTargets => Props.maxTargets;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.Awake() || Pawn.Downed || !Pawn.Spawned || (Props.RequireDrafted && Pawn.IsPlayerControlled && !Pawn.Drafted))
            {
                return;
            }
            if (!Props.hideMoteWhenNotDrafted || parent.pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                {
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                }
                mote?.Maintain();
            }
            if (Pawn.Spawned && (Props.interval <= 0 || Pawn.IsHashIntervalTick(Props.interval))
                && (!Props.RequireDrafted || !Pawn.IsPlayerControlled || Pawn.Drafted))
            {
                Interval();
            }
        }

        protected virtual bool Validator(IAttackTarget target)
        {
            bool b = target.Thing.HostileTo(Pawn);
            if (!Props.toFriendly && !b) return false;
            if (!Props.toHostile && b) return false;
            if (Props.toHostile && b && target.ThreatDisabled(Pawn)) return false;
            if (target.Thing.Position.DistanceToSquared(Pawn.Position) > Props.rangeSquared) return false;
            return !Props.requireLOS || GenSight.LineOfSight(Pawn.Position, target.Thing.Position, Pawn.Map);
        }

        protected virtual void Interval()
        {
            if (Props.maxTargets <= 0)
            {
                ApplyToList(PotentialTargets);
            }
            else if (MaxTargets > 0)
            {
                //Remove any disqualified targets
                if (TargetCache.Any())
                {
                    var targetCache = TargetCache.ListFullCopy();

                    foreach (var target in targetCache)
                    {
                        if (!Validator(target)) TargetCache.Remove(target);
                    }
                }
                //Add new targets if needed
                if (TargetCache.Count < MaxTargets)
                {
                    foreach (var newTarget in PotentialTargets.InRandomOrder())
                    {
                        TargetCache.Add(newTarget);
                        if (TargetCache.Count >= MaxTargets) break;
                    }
                }
                //Finally, deal damage
                ApplyToList(TargetCache);
            }

        }

        protected virtual void ApplyToList(IEnumerable<IAttackTarget> targets)
        {
            foreach (var target in targets)
            {
                if (target == Pawn) continue;
                ApplyToTarget(target.Thing);
            }
        }

        protected virtual void ApplyToTarget(Thing target)
        {
            if (Props.logEntryDef != null && Props.rulePackDef != null)
            {
                Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(target, Props.rulePackDef, Pawn) { def = Props.logEntryDef });
            }
            Props.effecterDef?.SpawnMaintainedIfPossible(target, target);
        }
    }
    public abstract class HediffCompProperties_Aura : HediffCompProperties
    {
        public HediffCompProperties_Aura()
        {
            compClass = typeof(HediffComp_Aura);
        }
        public bool RequireDrafted = true, toHostile = true, toFriendly = false, toSameFaction = false, requireLOS = false, hideMoteWhenNotDrafted = true;

        public float range = 10f;

        public int interval = 10;

        public EffecterDef effecterDef;

        public ThingDef mote;

        public int maxTargets = -1;

        public float rangeSquared;


        public LogEntryDef logEntryDef;
        public RulePackDef rulePackDef;

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            rangeSquared = range * range;
        }
    }
}
