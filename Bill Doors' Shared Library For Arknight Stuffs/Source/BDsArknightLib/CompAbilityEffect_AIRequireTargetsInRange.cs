using RimWorld;
using System.Security.Cryptography;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BDsArknightLib
{
    public abstract class CompAbilityEffect_AIRequireTargetsInRange : CompAbilityEffect
    {
        protected Pawn Pawn => parent.pawn;
        protected Map Map => Pawn.Map;

        protected Faction targetedFaction;
        new CompProperties_AbilityAreaOfEffect Props => (CompProperties_AbilityAreaOfEffect)props;

        protected virtual bool ExtraValidator(Thing thing)
        {
            return true;
        }
        protected virtual bool ExtraValidator(IntVec3 cell)
        {
            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            targetedFaction = target.Thing?.Faction;
            base.Apply(target, dest);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            int targetCount = 0;

            var cells = GenRadial.RadialCellsAround(target.Cell, Props.radius, true);
            foreach (var intVec in cells)
            {
                if (!intVec.InBounds(Map)) continue;
                if (Props.requireLOS && !GenSight.LineOfSight(target.Cell, intVec, Map)) continue;
                if (!ExtraValidator(intVec)) continue;

                foreach (var thing in Map.thingGrid.ThingsListAt(intVec))
                {
                    if (Props.AIcastRequireHostile && thing.HostileTo(Pawn))
                    {
                        TryAddTargetCount(thing);
                    }
                    if (Props.AIcastRequireFriendly && thing.Faction == Pawn.Faction)
                    {
                        TryAddTargetCount(thing);
                    }
                }
            }
            return targetCount >= Props.AIcastRequireTargetCount;

            void TryAddTargetCount(Thing p)
            {
                if (ExtraValidator(p)) targetCount++;
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, Props.radius);
        }
    }

    public abstract class CompAbilityEffect_Area : CompAbilityEffect_AIRequireTargetsInRange
    {
        CompProperties_AbilityAreaOfEffect Props => (CompProperties_AbilityAreaOfEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.centerEffecter?.SpawnMaintainedIfPossible(target, dest, Map, Props.centerEffecterScale);
        }

        public virtual void ApplyPerCell(IntVec3 cell)
        {
            Props.cellEffecter?.SpawnMaintainedIfPossible(cell, cell, Map, Props.centerEffecterScale);
        }
        public virtual void ApplyPerThing(Thing thing)
        {
            Props.thingEffecter?.SpawnMaintainedIfPossible(thing, thing, Map, Props.centerEffecterScale);

            if (Props.applyToThingRule != null && Props.logEntryDef != null)
            {
                Find.BattleLog.Add(new BattleLogEntry_AbilityWithIcon(Pawn, thing, parent.def, Props.applyToThingRule) { def = Props.logEntryDef });
            }
        }
    }

    public abstract class CompProperties_AbilityAreaOfEffect : CompProperties_AbilityEffect
    {
        public float radius = 5;

        public bool AIcastRequireHostile = true, AIcastRequireFriendly = false, requireLOS = true, applyToThing = true, inRandomOrder = true;

        public int AIcastRequireTargetCount = 3;

        public EffecterDef centerEffecter, cellEffecter, thingEffecter;
        public float centerEffecterScale = 1, cellEffecterScale = 1, thingEffecterScale = 1;

        public RulePackDef applyToThingRule;
        public LogEntryDef logEntryDef;
    }
}
