using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BDsArknightLib
{
    public abstract class CompAbilityEffect_AreaCount : CompAbilityEffect_Area
    {
        CompProperties_AbilityAreaCount Props => (CompProperties_AbilityAreaCount)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.centerEffecter?.SpawnMaintainedIfPossible(target, dest, Map, Props.centerEffecterScale);
            int c = Props.count;
            if (Props.alwaysCenter)
            {
                ApplyPerCell(target.Cell);
                c--;
            }

            var cells = GenRadial.RadialCellsAround(target.Cell, Props.radius, true).ToList();
            while (c > 0 && cells.Any())
            {
                IntVec3 intVec = cells.RandomElement();
                cells.Remove(intVec);
                if (!intVec.InBounds(Map) || !ExtraValidator(intVec) || (Props.requireLOS && !GenSight.LineOfSight(target.Cell, intVec, Map)))
                {
                    continue;
                }
                c--;
                ApplyPerCell(intVec);
            }
        }

        public override void ApplyPerCell(IntVec3 cell)
        {
            base.ApplyPerCell(cell);
            if (Props.applyToThing)
            {
                foreach (var thing in Map.thingGrid.ThingsListAt(cell))
                {
                    if (!ExtraValidator(thing)) continue;
                    ApplyPerThing(thing);
                }
            }
        }
    }

    public abstract class CompAbilityEffect_TargetCount : CompAbilityEffect_Area
    {
        CompProperties_AbilityAreaCount Props => (CompProperties_AbilityAreaCount)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.centerEffecter?.SpawnMaintainedIfPossible(target, dest, Map, Props.centerEffecterScale);
            int c = Props.count;

            var cells = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, Props.radius, true).ToList();
            if (Props.alwaysCenter && target.HasThing)
            {
                ApplyPerThing(target.Thing);
                c--;
                cells.Remove(target.Thing);
            }
            while (c > 0 && cells.Any())
            {
                var t = cells.RandomElement();
                cells.Remove(t);
                if (!ExtraValidator(t) || (Props.requireLOS && !GenSight.LineOfSightToThing(target.Cell, t, Map)))
                {
                    continue;
                }
                c--;
                ApplyPerThing(t);
            }
        }
    }

    public abstract class CompProperties_AbilityAreaCount : CompProperties_AbilityAreaOfEffect
    {
        public int count;

        public bool alwaysCenter = false;
    }
}
