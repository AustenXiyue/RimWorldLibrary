using BDsArknightLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BDsNydiaExp
{
    public class CompAbilityEffect_LaunchProjectiles : CompAbilityEffect_TargetCount
    {
        CompProperties_AbilityLaunchProjectiles Props => props as CompProperties_AbilityLaunchProjectiles;

        public ThingDef ProjectileDef => Props.projectileDef;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
        }

        public override void ApplyPerThing(Thing thing)
        {
            base.ApplyPerThing(thing);
            Projectile projectile = (Projectile)GenSpawn.Spawn(ProjectileDef, Pawn.Position, Pawn.Map);
            projectile.Launch(Pawn, thing, thing, ProjectileHitFlags.IntendedTarget);
        }

        protected override bool ExtraValidator(Thing thing)
        {
            if (thing is IAttackTarget tgt)
            {
                if (Pawn.HostileTo(thing) || thing.Faction == targetedFaction)
                {
                    return !tgt.ThreatDisabled(Pawn);
                }
            }
            return Pawn.HostileTo(thing);
        }
    }

    public class CompProperties_AbilityLaunchProjectiles : CompProperties_AbilityAreaCount
    {
        public CompProperties_AbilityLaunchProjectiles()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectiles);
        }

        public ThingDef projectileDef;
    }
}
