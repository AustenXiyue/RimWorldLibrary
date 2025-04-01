using BDsArknightLib;
using BillDoorsFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BDsNydiaExp
{
    public class ModExtension_NydiaProjectile : DefModExtension
    {
        public int launchTicks = 30;

        public int landingTicks = 50;

        public int flightTime = 150;

        public int fadeTime = 15;

        public float pullRadius = -1;

        public float pullSpeed = 5;
    }

    public class Verb_ShootNydiaRaiseAiming : Verb_Shoot
    {
        ModExtension_NydiaProjectile ext => verbProps.defaultProjectile.GetModExtension<ModExtension_NydiaProjectile>();
        public override float? AimAngleOverride
        {
            get
            {
                Stance_Busy stance_Busy = CasterPawn?.stances?.curStance as Stance_Busy;
                Vector3 vector = stance_Busy == null ? currentTarget.Cell.ToVector3Shifted() : stance_Busy.focusTarg.HasThing ? stance_Busy.focusTarg.Thing.DrawPos : stance_Busy.focusTarg.Cell.ToVector3Shifted();

                var landingTicks = ext?.landingTicks ?? 15;
                var archingApogee = (caster.DrawPos + vector) / 2 + new Vector3(0, 0, landingTicks * verbProps.defaultProjectile.projectile.SpeedTilesPerTick);
                return (archingApogee - caster.DrawPos).AngleFlat();
            }
        }

        protected override bool TryCastShot()
        {
            var cache = Projectile.projectile.flyOverhead;
            if (!cache)
            {
                Projectile.projectile.flyOverhead = true;
            }
            var b = base.TryCastShot();
            if (!cache)
            {
                Projectile.projectile.flyOverhead = false;
            }
            return b;
        }
    }

    public class Projectile_NydiaArchingProjectile : Projectile_Custom
    {
        ModExtension_NydiaProjectile ext => def.GetModExtension<ModExtension_NydiaProjectile>();

        Vector3 archingApogee;

        Vector3 lastDrawPos;

        public bool notDrawing => ticksToImpact > landingTicks && ticksToImpact < flightTime - launchTicks;

        int launchTicks => ext?.launchTicks ?? 30;

        int landingTicks => ext?.landingTicks ?? 50;

        int flightTime => ext?.flightTime ?? 150;

        int fadeTime => ext?.fadeTime ?? 15;

        int ticksPassed => flightTime - ticksToImpact;

        public override Vector3 ExactPosition
        {
            get
            {
                Vector3 vector = (origin - destination).Yto0() * ((float)ticksToImpact / flightTime);
                return destination.Yto0() + vector + Vector3.up * def.Altitude;
            }
        }

        protected float Alpha
        {
            get
            {
                if (ticksToImpact > flightTime - launchTicks)
                {
                    return (float)(launchTicks - ticksPassed) / fadeTime;
                }
                if (ticksToImpact < landingTicks)
                {
                    return (float)(landingTicks - ticksToImpact) / fadeTime;
                }
                return 0;
            }
        }


        public override Color DrawColor
        {
            get
            {
                var c = base.DrawColor;
                c.a = Alpha;
                return c;
            }
            set => base.DrawColor = value;
        }

        public override Material DrawMat => def.graphic.GetColoredVersion(def.graphic.Shader, DrawColor, DrawColorTwo).MatSingleFor(this);

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 vector;
                if (ticksToImpact == landingTicks)
                {
                    vector = destination + Vector3.forward * landingTicks * def.projectile.SpeedTilesPerTick;
                }
                else if (ticksToImpact > landingTicks)
                {
                    vector = (archingApogee - origin).normalized * ticksPassed * def.projectile.SpeedTilesPerTick + origin;
                }
                else
                {
                    var v = usedTarget.HasThing ? usedTarget.Thing.DrawPos : destination;
                    vector = (v - lastDrawPos) * (1f / ticksToImpact) + lastDrawPos;
                }
                return vector.Yto0() + Vector3.up * def.Altitude;
            }
        }

        public override Quaternion ExactRotation
        {
            get
            {
                if (ticksToImpact > landingTicks)
                {
                    return Quaternion.LookRotation((archingApogee - origin).Yto0());
                }
                var v = (usedTarget.HasThing ? usedTarget.Thing.DrawPos : destination) - Vector3.forward * 0.01f;
                return Quaternion.LookRotation((v - DrawPos).Yto0());
            }
        }

        public override void Tick()
        {
            lastDrawPos = DrawPos;
            base.Tick();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (notDrawing) return;
            base.DrawAt(drawLoc, flip);
        }

        public override void LaunchEffect(LocalTargetInfo target)
        {
            base.LaunchEffect(archingApogee.ToIntVec3());
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            archingApogee = (origin + usedTarget.Cell.ToVector3Shifted()) / 2 + new Vector3(0, 0, landingTicks * def.projectile.SpeedTilesPerTick);
            base.Launch(launcher, origin, usedTarget, intendedTarget, ProjectileHitFlags.IntendedTarget, preventFriendlyFire, equipment, targetCoverDef);
            ticksToImpact = flightTime;
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
            if (ext.pullRadius > 0 && hitThing != null)
            {
                var ts = GenRadial.RadialDistinctThingsAround(hitThing.PositionHeld, hitThing.MapHeld, ext.pullRadius, false).Where(t => t is Pawn p && !t.AdjacentTo8WayOrInside(hitThing) && p.Faction == hitThing.Faction);
                if (ts.Any())
                {
                    var thing = ts.RandomElement() as Pawn;
                    GameComponent_InvoluntaryMovingTracker.Tracker.AddData(new InvoluntaryMovingData(thing, ext.pullSpeed, pullTargetCell(hitThing.Position, thing.Position, thing.Map)));
                }
            }
        }

        IntVec3 pullTargetCell(IntVec3 center, IntVec3 target, Map map)
        {
            var cells = GenAdj.AdjacentCells.Select(c => c + center).Where(c2 => c2.InBounds(map)).ToList();
            cells.SortBy(c => (c + center).DistanceTo(target));
            return cells.First();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref archingApogee, "archingAarchingApogeengle");
            Scribe_Values.Look(ref lastDrawPos, "lastDrawPos");
        }
    }

    public class CompThrownFleckEmitterNydiaProj : CompThrownFleckEmitterIntermittent
    {
        Projectile_NydiaArchingProjectile proj => parent as Projectile_NydiaArchingProjectile;

        protected override bool IsOn => !proj.notDrawing && base.IsOn;
    }
}
