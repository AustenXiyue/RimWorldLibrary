using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MiliraCE
{
    public class ProjectileFleckDataCEColorable : ProjectileFleckDataCE
    {
        public Color fleckColor;
    }

    public class CompProperties_ProjectileFleckColorable : CompProperties_ProjectileFleck
    {
        public new List<ProjectileFleckDataCEColorable> FleckDatas;

        public CompProperties_ProjectileFleckColorable()
        {
            compClass = typeof(CompProjectileFleckColorable);
        }
    }

    // Copied from CombatExtended.CompProjectileFleck, but has fleck's instanceColor set
    public class CompProjectileFleckColorable : CompProjectileFleck
    {
        private ProjectileCE projectile;

        private CompProperties_ProjectileFleckColorable Props => (CompProperties_ProjectileFleckColorable)props;

        private bool IsOn
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (projectile == null)
                {
                    return false;
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            projectile = parent as ProjectileCE;
        }

        public override void CompTick()
        {
            if (IsOn)
            {
                if (lastPos == Vector3.zero && parent.Spawned)
                {
                    lastPos = parent.DrawPos;
                }
                Emit();
            }
        }

        private void Emit()
        {
            Vector3 vector = lastPos - parent.DrawPos;
            foreach (ProjectileFleckDataCEColorable fleckData in Props.FleckDatas)
            {
                if ((fleckData.cutoffTickRange.max >= 0 && age >= fleckData.cutoffTickRange.max) || !fleckData.shouldEmit(age))
                {
                    continue;
                }
                float num = 0f;
                Vector3 zero = Vector3.zero;
                for (int i = 0; i < fleckData.emissionAmount; i++)
                {
                    FleckCreationData dataStatic = FleckMaker.GetDataStatic(parent.DrawPos - fleckData.originOffsetInternal * vector + zero, parent.MapHeld, fleckData.fleck, (fleckData.scale.RandomInRange + num) * fleckData.cutoffScaleOffset(age));
                    dataStatic.rotation = fleckData.rotation.RandomInRange;
                    dataStatic.instanceColor = fleckData.fleckColor;
                    for (int j = 0; j < fleckData.flecksPerEmission; j++)
                    {
                        parent.MapHeld.flecks.CreateFleck(dataStatic);
                    }
                    if (fleckData.emissionAmount > 1)
                    {
                        zero += vector / fleckData.emissionAmount;
                        num += fleckData.scaleOffsetInternal;
                    }
                }
            }
            lastPos = parent.DrawPos;
        }
    }
}
