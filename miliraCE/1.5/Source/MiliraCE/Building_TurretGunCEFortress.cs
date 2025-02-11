using CombatExtended;
using Milira;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    [StaticConstructorOnStartup]
    public class Building_TurretGunCEFortress : Building_TurretGunCE
    {
        protected override bool CanSetForcedTarget => PlayerControlled;

        private static readonly Material Turret = MaterialPool.MatFrom("Milira/Building/Security/MilianHeavyTurretPlasma_TopII", ShaderDatabase.Cutout, Color.white);

        protected CompThingContainer_Milian CompThingContainer_Milian => this.TryGetComp<CompThingContainer_Milian>();

        private static readonly SimpleCurve ArmorSharpToDamageReductionCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(40f, 0.6f)
        };

        private float DamageReductionFactor
        {
            // Mostly copied from Milira.Building_TurretGunFortress.get_damageReductionFactor
            get
            {
                Pawn innerPawn = CompThingContainer_Milian?.innerPawn;
                if (innerPawn == null)
                {
                    return 1f;
                }
                return ArmorSharpToDamageReductionCurve.Evaluate(TryGetOverallArmor(innerPawn, StatDefOf.ArmorRating_Sharp));
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(60))
            {
                Modification_AutoRepair();
            }
        }

        // Copied from CombatExtended.Building_TurretGunCE.DrawAt, but replaced base.DrawAt with Milira's custom draw code
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 drawOffset = Vector3.zero;
            float angleOffset = 0f;
            if (Controller.settings.RecoilAnim)
            {
                CE_Utility.Recoil(def.building.turretGunDef, AttackVerb, out drawOffset, out angleOffset, top.CurRotation, handheld: false);
            }
            top.DrawTurret(drawLoc, drawOffset, angleOffset);
            DrawTurretTop();
        }

        // Copied from Milira.Building_TurretGunFortress.DrawTurretTop
        public void DrawTurretTop()
        {
            Vector3 vector = new Vector3(def.building.turretTopOffset.x, 0f, def.building.turretTopOffset.y).RotatedBy(top.CurRotation);
            float turretTopDrawSize = def.building.turretTopDrawSize;
            float num = base.CurrentEffectiveVerb?.AimAngleOverride ?? top.CurRotation;
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 drawPos = DrawPos;
            drawPos.y += 0.2f;
            matrix.SetTRS(drawPos + Altitudes.AltIncVect + vector, (-90f + num).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
            Graphics.DrawMesh(MeshPool.plane10, matrix, Turret, 0);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (!dinfo.Def.isExplosive)
            {
                dinfo.SetAmount(dinfo.Amount * (1f - DamageReductionFactor));
            }
            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        // Mostly copied from CombatExtended.ITab_Inventory
        public static float TryGetOverallArmor(Pawn pawn, StatDef stat)
        {
            float weightedSum = 0f;
            List<Apparel> wornApparels = pawn.apparel?.WornApparel;
            Apparel shield = wornApparels?.FirstOrDefault((Apparel x) => x is Apparel_Shield);
            foreach (BodyPartRecord partRecord in pawn.RaceProps.body.AllParts)
            {
                if (partRecord.depth != BodyPartDepth.Outside || (!((double)partRecord.coverage >= 0.1) && !partRecord.def.tags.Contains(BodyPartTagDefOf.BreathingPathway) && !partRecord.def.tags.Contains(BodyPartTagDefOf.SightSource)))
                {
                    continue;
                }
                float partSum = pawn.PartialStat(stat, partRecord);
                if (wornApparels != null)
                {
                    foreach (Apparel item in wornApparels)
                    {
                        partSum += item.PartialStat(stat, partRecord);
                    }
                    if (shield != null && !shield.def.apparel.CoversBodyPart(partRecord) && (shield.def?.GetModExtension<ShieldDefExtension>()?.PartIsCoveredByShield(partRecord, pawn)).GetValueOrDefault())
                    {
                        partSum += shield.GetStatValue(stat);
                    }
                }
                weightedSum += partRecord.coverage * partSum;
            }
            return weightedSum;
        }

        // Copied from Milira.Building_TurretGunFortress.Modification_AutoRepair
        public void Modification_AutoRepair()
        {
            if (ModsConfig.IsActive("Ancot.MilianModification") && CompThingContainer_Milian.innerPawn.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_FortressDamageControl) != null)
            {
                HitPoints += 6;
                HitPoints = Mathf.Min(HitPoints, base.MaxHitPoints);
            }
        }
    }
}
