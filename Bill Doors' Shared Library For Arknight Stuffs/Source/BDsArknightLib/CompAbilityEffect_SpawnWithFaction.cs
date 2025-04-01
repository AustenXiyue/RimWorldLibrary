using Verse;
using RimWorld;

namespace BDsArknightLib
{
    public class CompAbilityEffect_SpawnWithFaction : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnWithFaction Props => (CompProperties_AbilitySpawnWithFaction)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Thing t = ThingMaker.MakeThing(Props.thingDef);
            t.SetFactionDirect(parent.ConstantCaster.Faction);
            GenPlace.TryPlaceThing(t, target.Cell, parent.pawn.Map, ThingPlaceMode.Near);
            if (Props.sendSkipSignal)
            {
                CompAbilityEffect_Teleport.SendSkipUsedSignal(target, parent.pawn);
            }
            Props.effecter?.SpawnMaintainedIfPossible(target, dest, parent.pawn.Map, Props.effecterScale);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Cell.Filled(parent.pawn.Map) || (!Props.allowOnBuildings && target.Cell.GetEdifice(parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }
    }

    public class CompProperties_AbilitySpawnWithFaction : CompProperties_AbilitySpawn
    {
        public EffecterDef effecter;

        public float effecterScale = 1;

        public CompProperties_AbilitySpawnWithFaction()
        {
            compClass = typeof(CompAbilityEffect_SpawnWithFaction);
        }
    }
}
