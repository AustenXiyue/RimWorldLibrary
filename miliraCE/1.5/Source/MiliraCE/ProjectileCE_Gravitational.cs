using AncotLibrary;
using CombatExtended;
using CombatExtended.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    // TODO: Could it be better if implemented with a custom CompExplosiveCE?
    public class ProjectileCE_Gravitational : ProjectileCE
    {
        private int ticksToDetonation;

        public FieldForceProjectile_Extension Props => def.GetModExtension<FieldForceProjectile_Extension>();

        // Copied from ProjectileCE_Explosive
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
        }

        // Copied from ProjectileCE_Explosive, mostly
        public override void Tick()
        {
            base.Tick();
            if (ticksToDetonation <= 0)
            {
                return;
            }
            ticksToDetonation--;
            if (ticksToDetonation <= 0)
            {
                DoAttractive(null);
            }
            else
            {
                if (!((def.projectile as ProjectilePropertiesCE).suppressionFactor > 0f) || !landed)
                {
                    return;
                }
                foreach (Pawn item in ExactPosition.ToIntVec3().PawnsInRange(base.Map, 3f + def.projectile.explosionRadius + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f)))
                {
                    ApplySuppression(item, 1f - (float)(ticksToDetonation / def.projectile.explosionDelay));
                }
            }
        }

        // Copied from ProjectileCE_Explosive, mostly
        public override void Impact(Thing hitThing)
        {
            if (hitThing is Pawn)
            {
                Vector3 drawPos = hitThing.DrawPos;
                drawPos.y = ExactPosition.y;
                ExactPosition = drawPos;
                base.Position = ExactPosition.ToIntVec3();
            }
            if (def.projectile.explosionDelay == 0)
            {
                DoAttractive(hitThing);
                return;
            }
            landed = true;
            ticksToDetonation = def.projectile.explosionDelay;
            float dangerFactor = (def.projectile as ProjectilePropertiesCE).dangerFactor;
            if (dangerFactor > 0f)
            {
                base.DangerTracker.Notify_DangerRadiusAt(base.Position, def.projectile.explosionRadius + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f), def.projectile.GetDamageAmount(1) * dangerFactor);
                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher?.Faction);
            }
        }

        public void DoAttractive(Thing hitThing)
        {
            Map map = base.Map;
            float explosionRadius = def.projectile.explosionRadius;
            float distance = DamageAmount;
            List<Thing> list = new List<Thing>();
            foreach (IntVec3 item in GenRadial.RadialCellsAround(base.Position, explosionRadius, useCenter: true))
            {
                list.AddRange(item.GetThingList(map));
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Pawn victim && GenSight.LineOfSight(list[i].Position, base.Position, map))
                {
                    ForceMovementUtility.ApplyGravitationalForce(base.Position, victim, distance, Props.removeHediffsAffected);
                }
            }
            base.Impact(hitThing);
        }
    }
}
