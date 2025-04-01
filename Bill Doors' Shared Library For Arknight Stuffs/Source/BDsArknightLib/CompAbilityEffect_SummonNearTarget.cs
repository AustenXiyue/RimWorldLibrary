using System.Linq;
using Verse;
using Verse.AI;

namespace BDsArknightLib
{
    public class CompAbilityEffect_SummonNearTarget : CompAbilityEffect_Summon
    {
        CompProperties_AbilityEffect_SummonNearTarget Props => props as CompProperties_AbilityEffect_SummonNearTarget;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var cells = GenRadial.RadialCellsAround(target.Cell, Props.radius, false).Where(c => parent.pawn.Map.reachability.CanReach(c, target.Cell, PathEndMode.Touch, TraverseMode.NoPassClosedDoors));
            for (int i = Props.summonCount; i > 0; i--)
            {
                GeneratePawn(cells.RandomElement(), target);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            GenDraw.DrawRadiusRing(target.Cell, Props.radius);
        }
    }

    public class CompProperties_AbilityEffect_SummonNearTarget : CompProperties_AbilityEffect_Summon
    {
        public CompProperties_AbilityEffect_SummonNearTarget()
        {
            compClass = typeof(CompAbilityEffect_SummonNearTarget);
        }

        public float radius = 5;
    }
}
