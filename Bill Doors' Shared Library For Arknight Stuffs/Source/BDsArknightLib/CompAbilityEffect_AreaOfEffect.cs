using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BDsArknightLib
{

    //When you want to do something with eveything/cell in radius
    public abstract class CompAbilityEffect_AreaOfEffect : CompAbilityEffect_Area
    {
        CompProperties_AbilityAreaOfEffect Props => (CompProperties_AbilityAreaOfEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.centerEffecter?.SpawnMaintainedIfPossible(target, dest, Map, Props.centerEffecterScale);

            var cells = GenRadial.RadialCellsAround(target.Cell, Props.radius, true);
            if (Props.inRandomOrder) cells = cells.InRandomOrder();
            foreach (var intVec in cells)
            {
                if (!intVec.InBounds(Map)) continue;
                if (!ExtraValidator(intVec)) continue;
                if (Props.requireLOS && !GenSight.LineOfSight(target.Cell, intVec, Map)) continue;
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


}
