using CombatExtended;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    public class ExplosionCEDirectional : ExplosionCE
    {
        // Copied from CombatExtended.ExplosionCE.get_ExplosionCellsToHit, but with affectedAngle dealt with
        public override IEnumerable<IntVec3> ExplosionCellsToHit
        {
            get
            {
                IntVec3 center = Position;
                float angleMin = affectedAngle?.min ?? 0f;
                float angleMax = affectedAngle?.max ?? 0f;

                var roofed = center.Roofed(Map);
                var aboveRoofs = height >= CollisionVertical.WallCollisionHeight;

                var openCells = new List<IntVec3>();
                var adjWallCells = new List<IntVec3>();

                var num = GenRadial.NumCellsInRadius(radius);
                for (var i = 0; i < num; i++)
                {
                    var intVec = center + GenRadial.RadialPattern[i];
                    if (!intVec.InBounds(Map))
                    {
                        continue;
                    }
                    if (affectedAngle.HasValue)
                    {
                        float distance = (intVec - center).LengthHorizontal;
                        float distanceRatio = distance / radius;
                        if (!(distance > 0.5f))
                        {
                            continue;
                        }
                        float angle = Mathf.Atan2(-(intVec.z - center.z), intVec.x - center.x) * 57.29578f;
                        if (angle - angleMin < -0.5f * distanceRatio || angle - angleMax > 0.5f * distanceRatio)
                        {
                            continue;
                        }
                    }
                    if (aboveRoofs)
                    {
                        if ((!roofed && GenSight.LineOfSight(center, intVec, Map, false, null, 0, 0))
                                || !intVec.Roofed(Map))
                        {
                            openCells.Add(intVec);
                        }
                    }
                    else if (GenSight.LineOfSight(center, intVec, Map, true, null, 0, 0))
                    {
                        if (needLOSToCell1.HasValue || needLOSToCell2.HasValue)
                        {
                            bool flag = needLOSToCell1.HasValue && GenSight.LineOfSight(needLOSToCell1.Value, intVec, Map, false, null, 0, 0);
                            bool flag2 = needLOSToCell2.HasValue && GenSight.LineOfSight(needLOSToCell2.Value, intVec, Map, false, null, 0, 0);
                            if (!flag && !flag2)
                            {
                                continue;
                            }
                        }
                        openCells.Add(intVec);
                    }
                }
                foreach (var intVec2 in openCells)
                {
                    if (intVec2.Walkable(Map))
                    {
                        for (var k = 0; k < 4; k++)
                        {
                            var intVec3 = intVec2 + GenAdj.CardinalDirections[k];
                            if (intVec3.InHorDistOf(center, radius) && intVec3.InBounds(Map) && !intVec3.Standable(Map) && intVec3.GetEdifice(Map) != null && !openCells.Contains(intVec3) && adjWallCells.Contains(intVec3))
                            {
                                adjWallCells.Add(intVec3);
                            }
                        }
                    }
                }
                return openCells.Concat(adjWallCells);
            }
        }
    }

    public static class GenExplosionCEDirectional
    {
        // Copied from CombatExtended.GenExplosionCE.DoExplosion, but with a different Explosion class
        public static void DoExplosion(
            IntVec3 center,
            Map map,
            float radius,
            DamageDef damType,
            Thing instigator,
            int damAmount = -1,
            float armorPenetration = -1f,
            SoundDef explosionSound = null,
            ThingDef weapon = null,
            ThingDef projectile = null,
            Thing intendedTarget = null,
            ThingDef postExplosionSpawnThingDef = null,
            float postExplosionSpawnChance = 0f,
            int postExplosionSpawnThingCount = 1,
            GasType? postExplosionGasType = null,
            bool applyDamageToExplosionCellsNeighbors = false,
            ThingDef preExplosionSpawnThingDef = null,
            float preExplosionSpawnChance = 0f,
            int preExplosionSpawnThingCount = 1,
            float chanceToStartFire = 0f,
            bool damageFalloff = false,
            float? direction = null,
            List<Thing> ignoredThings = null,
            FloatRange? affectedAngle = null,
            bool doVisualEffects = true,
            float propagationSpeed = 1f,
            float excludeRadius = 0f,
            bool doSoundEffects = true,
            ThingDef postExplosionSpawnThingDefWater = null,
            float screenShakeFactor = 1f,
            SimpleCurve flammabilityChanceCurve = null,
            List<IntVec3> overrideCells = null,

            // CE parameters
            float height = 0f, float scaleFactor = 1f, bool destroyAfterwards = false, ThingWithComps explosionParentToDestroy = null)
        {
            // Allows destroyed things to be exploded with appropriate scaleFactor
            if (scaleFactor <= 0f)
            {
                scaleFactor = 1f;
            }
            else
            {
                scaleFactor = Mathf.Clamp(scaleFactor, GenExplosionCE.MinExplosionScale, GenExplosionCE.MaxExplosionScale);
            }

            if (map == null)
            {
                Log.Warning("CombatExtended :: Tried to do explosionCE in a null map.");
                return;
            }
            if (damAmount < 0)
            {
                damAmount = damType.defaultDamage;
                armorPenetration = damType.defaultArmorPenetration;
                if (damAmount < 0)
                {
                    Log.ErrorOnce("CombatExtended :: Attempted to trigger an explosionCE without defined damage", 910948823);
                    damAmount = 1;
                }
            }

            explosionSound = explosionSound ?? damType.soundExplosion;

            if (explosionSound == null)
            {
                Log.Error("CombatExtended :: SoundDef was null for DamageDef " + damType.defName + " as well as instigator " + instigator.ThingID);
            }

            damAmount = Mathf.RoundToInt(damAmount * scaleFactor);
            radius *= scaleFactor;
            armorPenetration *= scaleFactor;

            ExplosionCEDirectional explosion = GenSpawn.Spawn(MiliraCE_ThingDefOf.ExplosionCEDirectional, center, map) as ExplosionCEDirectional;
            IntVec3? needLOSToCell = null;
            IntVec3? needLOSToCell2 = null;
            if (direction.HasValue)
            {
                CalculateNeededLOSToCells(center, map, direction.Value, out needLOSToCell, out needLOSToCell2);
            }
            explosion.height = height;
            explosion.radius = radius;
            explosion.damType = damType;
            explosion.instigator = instigator;
            explosion.damAmount = damAmount;
            explosion.armorPenetration = armorPenetration;
            explosion.weapon = weapon;
            explosion.projectile = projectile;
            explosion.intendedTarget = intendedTarget;
            explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
            explosion.preExplosionSpawnChance = preExplosionSpawnChance;
            explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
            explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
            explosion.postExplosionSpawnChance = postExplosionSpawnChance;
            explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
            explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            explosion.chanceToStartFire = chanceToStartFire;
            explosion.damageFalloff = damageFalloff;
            explosion.needLOSToCell1 = needLOSToCell;
            explosion.needLOSToCell2 = needLOSToCell2;
            explosion.postExplosionGasType = postExplosionGasType;
            explosion.affectedAngle = affectedAngle;
            explosion.doVisualEffects = doVisualEffects;
            explosion.propagationSpeed = propagationSpeed;
            explosion.excludeRadius = excludeRadius;
            explosion.doSoundEffects = doSoundEffects;
            explosion.postExplosionSpawnThingDefWater = postExplosionSpawnThingDefWater;
            explosion.screenShakeFactor = screenShakeFactor;
            explosion.flammabilityChanceCurve = flammabilityChanceCurve;
            explosion.overrideCells = overrideCells;
            explosion.StartExplosionCE(explosionSound, ignoredThings);

            // Needed to allow CompExplosive to use stackCount
            if (destroyAfterwards && !explosionParentToDestroy.Destroyed)
            {
                explosionParentToDestroy?.Kill();
            }
        }

        // Copied from Verse.GenExplosion.CalculateNeededLOSToCells
        private static void CalculateNeededLOSToCells(IntVec3 position, Map map, float direction, out IntVec3? needLOSToCell1, out IntVec3? needLOSToCell2)
        {
            needLOSToCell1 = null;
            needLOSToCell2 = null;
            if (position.CanBeSeenOverFast(map))
            {
                return;
            }
            direction = GenMath.PositiveMod(direction, 360f);
            IntVec3 intVec = position;
            intVec.z++;
            IntVec3 intVec2 = position;
            intVec2.z--;
            IntVec3 intVec3 = position;
            intVec3.x--;
            IntVec3 intVec4 = position;
            intVec4.x++;
            if (direction < 90f)
            {
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec3;
                }
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec;
                }
            }
            else if (direction < 180f)
            {
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec;
                }
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec4;
                }
            }
            else if (direction < 270f)
            {
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec4;
                }
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec2;
                }
            }
            else
            {
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec2;
                }
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec3;
                }
            }
        }
    }
}
