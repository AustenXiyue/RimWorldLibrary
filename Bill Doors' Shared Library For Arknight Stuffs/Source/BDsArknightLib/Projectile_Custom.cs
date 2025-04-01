using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using System.Collections.Generic;

namespace BDsArknightLib
{
    public class Projectile_Custom : Bullet
    {
        protected ModExtension_Projectile extension => def.GetModExtension<ModExtension_Projectile>();

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            LaunchEffect(usedTarget);
        }

        public virtual void LaunchEffect(LocalTargetInfo target)
        {
            if (extension?.launchEffecter != null)
            {
                Effecter effecter = extension.launchEffecter.Spawn();
                effecter.scale = extension.muzzleScale;
                effecter.offset = Vector3.forward.RotatedBy((target.Cell.ToVector3Shifted() - launcher.DrawPos).Yto0().AngleFlat()) * extension.muzzleOffset;
                effecter.Trigger(launcher, target.ToTargetInfo(launcher.Map));
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield && !def.projectile.soundImpact.NullOrUndefined())
            {
                def.projectile.soundImpact.PlayOneShot(SoundInfo.InMap(this));
            }
            if (extension?.hitEffecter != null)
            {
                var hitPosV3 = hitThing == null ? ExactPosition : hitThing.DrawPos;
                var hitOffset = hitPosV3 - hitPosV3.ToIntVec3().ToVector3Shifted();
                extension.hitEffecter.Spawn(hitPosV3.ToIntVec3(), Map, hitOffset, extension.hitEffecterScale);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }

    public class Projectile_CustomLaser : Bullet
    {
        public override Vector3 ExactPosition => ExactPosOverride == Vector3.zero ? base.ExactPosition : ExactPosOverride;

        protected Vector3 ExactPosOverride = Vector3.zero;

        protected ModExtension_Projectile extension => def.GetModExtension<ModExtension_Projectile>();

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            if (!CheckForFreeInterceptBetween(origin, usedTarget.CenterVector3))
            {
                ticksToImpact = 0;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (extension != null)
            {
                var hitPosV3 = hitThing == null ? ExactPosition : hitThing.DrawPos + new Vector3(Rand.Range(-0.4f, 0.4f), 0, Rand.Range(-0.4f, 0.4f));

                var hitPosIV3 = hitPosV3.ToIntVec3();
                var hitOffset = hitPosV3 - hitPosV3.ToIntVec3().ToVector3Shifted();
                var originOffset = (Vector3.forward * extension.beamOriginOffset).RotatedBy(Vector3.SignedAngle(Vector3.forward, (hitPosV3 - origin).Yto0(), Vector3.up));

                var Ray = (hitPosV3 - origin - originOffset).Yto0();
                var scale = Ray.MagnitudeHorizontal();


                extension.beamEndEffecterDef?.Spawn(hitPosIV3, Map, hitOffset);
                extension.beamOriginEffecterDef?.Spawn(origin.ToIntVec3(), Map, originOffset);
                if (extension.beamLineFleckDef != null)
                {

                    var data = FleckMaker.GetDataStatic((hitPosV3 + origin + originOffset) / 2, Map, extension.beamLineFleckDef);
                    data.exactScale = new Vector3(extension.beamLineFleckDef.graphicData.drawSize.x, 1, scale / extension.beamLineFleckDef.graphicData.drawSize.y);
                    data.rotation = Vector3.SignedAngle(Vector3.forward, Ray, Vector3.up);
                    Map.flecks.CreateFleck(data);
                }
            }
            base.Impact(hitThing, blockedByShield);
        }

        private static List<IntVec3> checkedCells = new List<IntVec3>();
        private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, lastExactPos, newExactPos))
                {
                    ExactPosOverride = newExactPos;
                    Impact(null, blockedByShield: true);
                    return true;
                }
            }
            IntVec3 intVec = lastExactPos.ToIntVec3();
            IntVec3 intVec2 = newExactPos.ToIntVec3();
            if (intVec2 == intVec)
            {
                return false;
            }
            if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
            {
                return false;
            }
            if (intVec2.AdjacentToCardinal(intVec))
            {
                return CheckForFreeIntercept(intVec2);
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(origin, intVec2) <= 0f)
            {
                return false;
            }
            Vector3 vect = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 vector = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            checkedCells.Clear();
            int num2 = 0;
            IntVec3 intVec3;
            do
            {
                vect += vector;
                intVec3 = vect.ToIntVec3();
                if (!checkedCells.Contains(intVec3))
                {
                    if (CheckForFreeIntercept(intVec3))
                    {
                        return true;
                    }
                    checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
            }
            while (!(intVec3 == intVec2));
            return false;
        }

        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (destination.ToIntVec3() == c)
            {
                return false;
            }
            float num = VerbUtility.InterceptChanceFactorFromDistance(origin, c);
            if (num <= 0f)
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!CanHit(thing))
                {
                    continue;
                }
                bool flag2 = false;
                if (thing.def.Fillage == FillCategory.Full)
                {
                    if (!(thing is Building_Door building_Door) || !building_Door.Open)
                    {
                        Impact(thing);
                        return true;
                    }
                    flag2 = true;
                }
                float num2 = 0f;
                if (thing is Pawn pawn)
                {
                    num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0)
                    {
                        num2 *= 0.1f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        if (preventFriendlyFire)
                        {
                            num2 = 0f;
                        }
                        else
                        {
                            num2 *= Find.Storyteller.difficulty.friendlyFireChanceFactor;
                        }
                    }
                }
                else if (thing.def.fillPercent > 0.2f)
                {
                    num2 = (flag2 ? 0.05f : ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * 0.15f) : (thing.def.fillPercent * 1f)));
                }
                num2 *= num;
                if (num2 > 1E-05f)
                {
                    if (Rand.Chance(num2))
                    {
                        Impact(thing);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
