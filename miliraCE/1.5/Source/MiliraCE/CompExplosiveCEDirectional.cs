using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    public class CompProperties_ExplosiveCEDirectional : CompProperties_ExplosiveCE
    {
        public float directionalDamageAmountBase = 0f;

        public DamageDef directionalExplosiveDamageType;

        public float directionalExplosiveRadius = 0f;

        public float directionalExplosiveAngle = 360f;

        public CompProperties_ExplosiveCEDirectional()
        {
            compClass = typeof(CompExplosiveCEDirectional);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (directionalExplosiveDamageType == null)
            {
                directionalExplosiveDamageType = DamageDefOf.Bomb;
            }
        }
    }

    public class CompExplosiveCEDirectional : CompExplosiveCE
    {
        private CompProperties_ExplosiveCEDirectional Props => props as CompProperties_ExplosiveCEDirectional;

        // Mostly copied from CombatExtended.CompExplosiveCE.Explode and Milira.RailGunBullet
        public override void Explode(Thing instigator, Vector3 pos, Map map, float scaleFactor = 1f, float? direction = null, List<Thing> ignoredThings = null)
        {
            IntVec3 intVec = pos.ToIntVec3();
            if (map == null)
            {
                Log.Warning("Tried to do explodeCE in a null map.");
                return;
            }
            if (!intVec.InBounds(map))
            {
                Log.Warning("Tried to explodeCE out of bounds");
                return;
            }
            foreach (CompFragments comp in parent.GetComps<CompFragments>())
            {
                comp.Throw(pos, map, instigator);
            }
            if (parent.def != null)
            {
                if (Props.explosiveRadius > 0f)
                {
                    DoExplosion(
                        intVec, pos.y, map,
                        instigator,
                        Props.damageAmountBase, Props.explosiveDamageType,
                        Props.explosiveRadius, null, direction,
                        scaleFactor, ignoredThings);
                }
                if (Props.directionalExplosiveRadius > 0f && Props.directionalExplosiveAngle > 0f && Props.directionalExplosiveAngle <= 360f && direction is float directionAngle)
                {
                    float half = Props.directionalExplosiveAngle * 0.5f;
                    float lower = directionAngle - half, upper = directionAngle + half;
                    if (lower < -180f)
                    {
                        DoExplosion(intVec, pos.y, map,
                            instigator,
                            Props.directionalDamageAmountBase, Props.directionalExplosiveDamageType,
                            Props.directionalExplosiveRadius, new FloatRange(lower + 360, 180), direction,
                            scaleFactor, ignoredThings);
                        DoExplosion(intVec, pos.y, map,
                            instigator,
                            Props.directionalDamageAmountBase, Props.directionalExplosiveDamageType,
                            Props.directionalExplosiveRadius, new FloatRange(-180, upper), direction,
                            scaleFactor, ignoredThings);
                    }
                    else if (upper > 180f)
                    {
                        DoExplosion(intVec, pos.y, map,
                            instigator,
                            Props.directionalDamageAmountBase, Props.directionalExplosiveDamageType,
                            Props.directionalExplosiveRadius, new FloatRange(lower, 180), direction,
                            scaleFactor, ignoredThings);
                        DoExplosion(intVec, pos.y, map,
                            instigator,
                            Props.directionalDamageAmountBase, Props.directionalExplosiveDamageType,
                            Props.directionalExplosiveRadius, new FloatRange(-180, upper - 360), direction,
                            scaleFactor, ignoredThings);
                    }
                    else
                    {
                        DoExplosion(intVec, pos.y, map,
                            instigator,
                            Props.directionalDamageAmountBase, Props.directionalExplosiveDamageType,
                            Props.directionalExplosiveRadius, new FloatRange(lower, upper), direction,
                            scaleFactor, ignoredThings);
                    }
                }
            }
        }

        private void DoExplosion(
            IntVec3 intVecPos, float y, Map map,
            Thing instigator,
            float damageAmountBase, DamageDef damageType,
            float explosiveRadius, FloatRange? affectedAngle = null, float? direction = null,
            float scaleFactor = 1f, List<Thing> ignoredThings = null)
        {
            GenExplosionCEDirectional.DoExplosion(
                intVecPos,
                map,
                explosiveRadius,
                damageType,
                instigator,
                GenMath.RoundRandom(damageAmountBase),
                (float)Props.GetExplosionArmorPenetration(),
                Props.explosionSound,
                null,
                parent.def,
                null,
                Props.postExplosionSpawnThingDef,
                Props.postExplosionSpawnChance,
                Props.postExplosionSpawnThingCount,
                Props.postExplosionGasType,
                Props.applyDamageToExplosionCellsNeighbors,
                Props.preExplosionSpawnThingDef,
                Props.preExplosionSpawnChance,
                Props.preExplosionSpawnThingCount,
                Props.chanceToStartFire,
                Props.damageFalloff,
                direction,
                ignoredThings,
                affectedAngle,
                doVisualEffects: true,
                1f,
                0f,
                doSoundEffects: true,
                null,
                1f,
                null,
                null,
                y,
                scaleFactor);
        }
    }
}
