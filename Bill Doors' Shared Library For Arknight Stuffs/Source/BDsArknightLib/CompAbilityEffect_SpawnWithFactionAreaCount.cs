using RimWorld;
using Verse;

namespace BDsArknightLib
{
    public class CompAbilityEffect_SpawnWithFactionAreaCount : CompAbilityEffect_AreaCount
    {
        CompProperties_AbilitySpawnWithFactionAreaCount Props => props as CompProperties_AbilitySpawnWithFactionAreaCount;
        public override void ApplyPerCell(IntVec3 cell)
        {
            base.ApplyPerCell(cell);
            Thing t = ThingMaker.MakeThing(Props.thingDef);
            t.SetFactionDirect(parent.ConstantCaster.Faction);
            GenSpawn.Spawn(t, cell, parent.pawn.Map, WipeMode.VanishOrMoveAside);
        }
    }

    public class CompProperties_AbilitySpawnWithFactionAreaCount : CompProperties_AbilityAreaCount
    {
        public CompProperties_AbilitySpawnWithFactionAreaCount()
        {
            compClass = typeof(CompAbilityEffect_SpawnWithFactionAreaCount);
        }

        public ThingDef thingDef;
    }
}
