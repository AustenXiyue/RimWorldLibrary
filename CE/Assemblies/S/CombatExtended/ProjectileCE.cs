using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using CombatExtended.Lasers;
using CombatExtended.Utilities;
using ProjectileImpactFX;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended;

public abstract class ProjectileCE : ThingWithComps
{
	protected const int SuppressionRadius = 3;

	protected const int collisionCheckSize = 5;

	protected bool lerpPosition = true;

	protected bool kinit = false;

	protected float ballisticCoefficient;

	protected float mass;

	protected float radius;

	protected float gravity;

	protected Vector3 velocity;

	protected float initialSpeed;

	protected int ticksToTruePosition;

	public bool OffMapOrigin = false;

	public Vector2 origin;

	public IntVec3 OriginIV3;

	public Vector2 Destination;

	protected float? damageAmount;

	public Thing equipment;

	public ThingDef equipmentDef;

	public Thing launcher;

	public LocalTargetInfo intendedTarget;

	public float minCollisionDistance;

	public bool canTargetSelf;

	public bool castShadow = true;

	public bool logMisses = true;

	protected bool ignoreRoof;

	public GlobalTargetInfo globalTargetInfo = GlobalTargetInfo.Invalid;

	public GlobalTargetInfo globalSourceInfo = GlobalTargetInfo.Invalid;

	public bool landed;

	public int intTicksToImpact;

	protected Sustainer ambientSustainer;

	protected float suppressionAmount;

	public Thing mount;

	public float AccuracyFactor;

	public float startingTicksToImpact;

	public int FlightTicks = 0;

	private Vector3 exactPosition;

	public Vector3 LastPos;

	protected DangerTracker _dangerTracker = null;

	private int lastShotLine = -1;

	private Ray shotLine;

	public float shotAngle = 0f;

	public float shotRotation = 0f;

	public float shotHeight = 0f;

	public float shotSpeed = -1f;

	private float GravityFactor = 1.96f;

	protected Material[] shadowMaterial;

	private static readonly List<Thing> potentialCollisionCandidates = new List<Thing>();

	protected bool InstigatorGuilty => !(launcher is Pawn pawn) || !pawn.Drafted;

	public Thing intendedTargetThing => intendedTarget.Thing;

	public virtual float DamageAmount
	{
		get
		{
			if (!damageAmount.HasValue)
			{
				float weaponDamageMultiplier = equipment?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
				damageAmount = def.projectile.GetDamageAmount(weaponDamageMultiplier);
			}
			if (lerpPosition)
			{
				return damageAmount.Value;
			}
			return damageAmount.Value * RemainingKineticEnergyPct;
		}
	}

	public float RemainingKineticEnergyPct => shotSpeed * shotSpeed / (initialSpeed * initialSpeed);

	public int ticksToImpact
	{
		get
		{
			return intTicksToImpact;
		}
		set
		{
			intTicksToImpact = value;
		}
	}

	public virtual float Height => ExactPosition.y;

	public virtual Vector3 ExactPosition
	{
		get
		{
			return exactPosition;
		}
		set
		{
			exactPosition = value;
			base.Position = exactPosition.ToIntVec3();
		}
	}

	public Vector3 ExactMinusLastPos
	{
		get
		{
			Log.ErrorOnce("ExactMinusLastPos is deprecated and will be removed in 1.5", 50022);
			return ExactPosition - LastPos;
		}
	}

	public override Vector3 DrawPos
	{
		get
		{
			float num = Mathf.Max(0f, ExactPosition.y * 0.84f);
			if (FlightTicks < ticksToTruePosition)
			{
				num *= (float)FlightTicks / (float)ticksToTruePosition;
			}
			return new Vector3(ExactPosition.x, def.Altitude, ExactPosition.z + num);
		}
	}

	protected DangerTracker DangerTracker => _dangerTracker ?? (_dangerTracker = base.Map.GetDangerTracker());

	public Ray ShotLine
	{
		get
		{
			if (lastShotLine != FlightTicks)
			{
				shotLine = new Ray(LastPos, ExactPosition - LastPos);
				lastShotLine = FlightTicks;
			}
			return shotLine;
		}
	}

	public Quaternion DrawRotation
	{
		get
		{
			Vector2 vector = Destination - origin;
			float x = vector.x / startingTicksToImpact;
			float num = (vector.y - shotHeight) / startingTicksToImpact + shotSpeed * Mathf.Sin(shotAngle) / 60f - GravityFactor * (float)FlightTicks / 3600f;
			return Quaternion.AngleAxis(57.29578f * Mathf.Atan2(0f - num, x) + 90f, Vector3.up);
		}
	}

	public virtual Quaternion ExactRotation => Quaternion.AngleAxis(shotRotation, Vector3.down);

	protected Material ShadowMaterial
	{
		get
		{
			if (shadowMaterial == null)
			{
				if (Graphic is Graphic_Collection g)
				{
					shadowMaterial = GetShadowMaterial(g);
				}
				else
				{
					shadowMaterial = new Material[1];
					shadowMaterial[0] = Graphic.GetColoredVersion(ShaderDatabase.Transparent, Color.black, Color.black).MatSingle;
				}
			}
			return shadowMaterial[Rand.Range(0, shadowMaterial.Length)];
		}
	}

	protected float DistanceTraveled => CE_Utility.MaxProjectileRange(shotHeight, shotSpeed, shotAngle, GravityFactor);

	protected virtual Vector2 Vec2Position(float ticks = -1f)
	{
		Log.ErrorOnce("Vec2Position(float) is deprecated and will be removed in 1.5", 50021);
		if (ticks < 0f)
		{
			return Vec2Position();
		}
		return Vector2.Lerp(origin, Destination, ticks / startingTicksToImpact);
	}

	protected virtual Vector2 Vec2Position()
	{
		return Vector2.Lerp(origin, Destination, (float)FlightTicks / startingTicksToImpact);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving && launcher != null && launcher.Destroyed)
		{
			launcher = null;
		}
		CE_Scriber.Late(this, delegate(string id)
		{
			Scribe_References.Look(ref launcher, "launcher_" + id);
		});
		Scribe_TargetInfo.Look(ref globalSourceInfo, "globalSourceInfo");
		Scribe_TargetInfo.Look(ref globalTargetInfo, "globalTargetInfo");
		Scribe_TargetInfo.Look(ref intendedTarget, "intendedTarget");
		Scribe_Values.Look(ref origin, "origin", default(Vector2), forceSave: true);
		Scribe_References.Look(ref launcher, "launcher");
		Scribe_References.Look(ref equipment, "equipment");
		Scribe_Values.Look(ref intTicksToImpact, "ticksToImpact", 0, forceSave: true);
		Scribe_Values.Look(ref startingTicksToImpact, "startingTicksToImpact", 0f, forceSave: true);
		Scribe_Defs.Look(ref equipmentDef, "equipmentDef");
		Scribe_Values.Look(ref landed, "landed", defaultValue: false);
		Scribe_Values.Look(ref shotAngle, "shotAngle", 0f, forceSave: true);
		Scribe_Values.Look(ref shotRotation, "shotRotation", 0f, forceSave: true);
		Scribe_Values.Look(ref shotHeight, "shotHeight", 0f, forceSave: true);
		Scribe_Values.Look(ref shotSpeed, "shotSpeed", 0f, forceSave: true);
		Scribe_Values.Look(ref canTargetSelf, "canTargetSelf", defaultValue: false);
		Scribe_Values.Look(ref logMisses, "logMisses", defaultValue: true);
		Scribe_Values.Look(ref castShadow, "castShadow", defaultValue: true);
		Scribe_Values.Look(ref lerpPosition, "lerpPosition", defaultValue: true);
		Scribe_Values.Look(ref ignoreRoof, "ignoreRoof", defaultValue: true);
		Scribe_Values.Look(ref exactPosition, "exactPosition");
		Scribe_Values.Look(ref GravityFactor, "gravityFactor", 1.96f);
		Scribe_Values.Look(ref LastPos, "lastPosition");
		Scribe_Values.Look(ref FlightTicks, "flightTicks", 0);
		Scribe_Values.Look(ref OriginIV3, "originIV3", new IntVec3(origin));
		Scribe_Values.Look(ref Destination, "destination", origin + Vector2.up.RotatedBy(shotRotation) * DistanceTraveled);
	}

	public virtual void Throw(Thing launcher, Vector3 origin, Vector3 heading, Thing equipment = null)
	{
		ExactPosition = (LastPos = origin);
		shotHeight = origin.y;
		this.origin = new Vector2(origin.x, origin.z);
		OriginIV3 = new IntVec3(this.origin);
		Destination = this.origin + Vector2.up.RotatedBy(shotRotation) * DistanceTraveled;
		shotSpeed = Math.Max(heading.magnitude, def.projectile.speed);
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		castShadow = projectilePropertiesCE.castShadow;
		velocity = heading;
		this.launcher = launcher;
		this.equipment = equipment;
		equipmentDef = equipment?.def;
		if (!def.projectile.soundAmbient.NullOrUndefined())
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(info);
		}
		ballisticCoefficient = projectilePropertiesCE.ballisticCoefficient.RandomInRange;
		mass = projectilePropertiesCE.mass.RandomInRange;
		radius = projectilePropertiesCE.diameter.RandomInRange / 2000f;
		gravity = projectilePropertiesCE.Gravity;
		initialSpeed = shotSpeed;
	}

	public virtual void RayCast(Thing launcher, VerbProperties verbProps, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0f, float shotSpeed = -1f, float spreadDegrees = 0f, float aperatureSize = 0.03f, Thing equipment = null)
	{
		float num = Mathf.Sin(0.00052359875f) + aperatureSize;
		float num2 = 1f / (num * num * 3.14159f);
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		shotRotation = (float)Math.PI / 180f * shotRotation + 1.570795f;
		Vector3 vector = new Vector3(Mathf.Cos(shotRotation) * Mathf.Cos(shotAngle), Mathf.Sin(shotAngle), Mathf.Sin(shotRotation) * Mathf.Cos(shotAngle));
		Vector3 vector2 = new Vector3(origin.x, shotHeight, origin.y);
		Map map = launcher.Map;
		Vector3 vector3 = vector * verbProps.range + vector2;
		this.shotAngle = shotAngle;
		this.shotHeight = shotHeight;
		this.shotRotation = shotRotation;
		this.launcher = launcher;
		this.origin = origin;
		this.equipment = equipment;
		equipmentDef = equipment?.def ?? null;
		Ray ray = new Ray(vector2, vector);
		LaserBeamCE laserBeamCE = this as LaserBeamCE;
		float num3 = Mathf.Sin(spreadDegrees / 2f * ((float)Math.PI / 180f));
		Vector3 point = ray.GetPoint((equipmentDef as LaserGunDef)?.barrelLength ?? 0.9f);
		Bounds boundsFor = CE_Utility.GetBoundsFor(intendedTargetThing);
		for (int i = 1; (float)i < verbProps.range; i++)
		{
			float num4 = ((float)i * num3 + aperatureSize) * ((float)i * num3 + aperatureSize) * 3.14159f;
			if (projectilePropertiesCE.damageFalloff)
			{
				laserBeamCE.DamageModifier = 1f / (num2 * num4);
			}
			Vector3 point2 = ray.GetPoint(i);
			if (point2.y < 0f)
			{
				vector3 = point2;
				landed = true;
				ExactPosition = point2;
				base.Position = ExactPosition.ToIntVec3();
				break;
			}
			IntVec3 c = point2.ToIntVec3();
			if (!c.InBounds(map))
			{
				point2 = (ExactPosition = ray.GetPoint(i - 1));
				vector3 = point2;
				landed = true;
				LastPos = vector3;
				base.Position = ExactPosition.ToIntVec3();
				laserBeamCE.SpawnBeam(point, vector3);
				RayCastSuppression(point.ToIntVec3(), vector3.ToIntVec3());
				laserBeamCE.Impact(null, point);
				return;
			}
			foreach (Thing item in base.Map.thingGrid.ThingsListAtFast(c))
			{
				if (this == item || !CE_Utility.GetBoundsFor(item).IntersectRay(ray, out var _) || (i < 2 && item != intendedTargetThing))
				{
					continue;
				}
				if (item is Plant plant)
				{
					if (!Rand.Chance(item.def.fillPercent * plant.Growth))
					{
						continue;
					}
				}
				else if (item is Building && !Rand.Chance(item.def.fillPercent))
				{
					continue;
				}
				ExactPosition = point2;
				vector3 = point2;
				landed = true;
				LastPos = vector3;
				base.Position = ExactPosition.ToIntVec3();
				laserBeamCE.SpawnBeam(point, vector3);
				RayCastSuppression(point.ToIntVec3(), vector3.ToIntVec3());
				laserBeamCE.Impact(item, point);
				return;
			}
		}
		if (laserBeamCE != null)
		{
			laserBeamCE.SpawnBeam(point, vector3);
			RayCastSuppression(point.ToIntVec3(), vector3.ToIntVec3());
			Destroy();
		}
	}

	protected void RayCastSuppression(IntVec3 muzzle, IntVec3 destination, Map map = null)
	{
		if (muzzle == destination)
		{
			return;
		}
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		if (projectilePropertiesCE.suppressionFactor <= 0f || projectilePropertiesCE.airborneSuppressionFactor <= 0f)
		{
			return;
		}
		if (map == null)
		{
			map = base.Map;
		}
		foreach (Pawn item in muzzle.PawnsNearSegment(destination, map, 3f).Except(muzzle.PawnsInRange(map, 3f)))
		{
			ApplySuppression(item);
		}
	}

	public virtual void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0f, float shotSpeed = -1f, Thing equipment = null, float distance = -1f)
	{
		this.shotAngle = shotAngle;
		this.shotHeight = shotHeight;
		this.shotRotation = shotRotation;
		this.shotSpeed = Math.Max(shotSpeed, def.projectile.speed);
		ticksToTruePosition = (def.projectile as ProjectilePropertiesCE).TickToTruePos;
		if (def.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			castShadow = projectilePropertiesCE.castShadow;
			lerpPosition = projectilePropertiesCE.lerpPosition;
			GravityFactor = projectilePropertiesCE.Gravity;
		}
		if (shotHeight >= 2f && launcher.Spawned && base.Position.Roofed(launcher.Map))
		{
			ignoreRoof = true;
		}
		Launch(launcher, origin, equipment);
	}

	public virtual void Launch(Thing launcher, Vector2 origin, Thing equipment = null)
	{
		this.launcher = launcher;
		this.origin = origin;
		OriginIV3 = new IntVec3(origin);
		Destination = origin + Vector2.up.RotatedBy(shotRotation) * DistanceTraveled;
		this.equipment = equipment;
		equipmentDef = equipment?.def;
		if (!def.projectile.soundAmbient.NullOrUndefined())
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(info);
		}
		startingTicksToImpact = GetFlightTime() * 60f;
		ticksToImpact = Mathf.CeilToInt(startingTicksToImpact);
		ExactPosition = (LastPos = new Vector3(origin.x, shotHeight, origin.y));
	}

	public virtual void InterceptProjectile(object interceptor, Vector3 impactPosition, bool destroyCompletely = false)
	{
		ExactPosition = impactPosition;
		landed = true;
		ticksToImpact = 0;
		if (destroyCompletely)
		{
			Destroy();
		}
		else
		{
			Impact(null);
		}
	}

	public virtual void InterceptProjectile(object interceptor, Vector3 shieldPosition, float shieldRadius, bool destroyCompletely = false)
	{
		InterceptProjectile(interceptor, BlockerRegistry.GetExactPosition(OriginIV3.ToVector3(), ExactPosition, shieldPosition, shieldRadius * shieldRadius));
	}

	protected bool CheckIntercept(Thing interceptorThing, CompProjectileInterceptor interceptorComp, bool withDebug = false)
	{
		Vector3 vector = interceptorThing.Position.ToVector3ShiftedWithAltitude(0.5f);
		float num = interceptorComp.Props.radius;
		float num2 = num + def.projectile.SpeedTilesPerTick + 0.1f;
		float num3 = num2 * num2;
		Vector3 vector2 = ExactPosition;
		if ((vector2 - vector).sqrMagnitude > num3)
		{
			return false;
		}
		if (!interceptorComp.Active)
		{
			return false;
		}
		if (interceptorComp.Props.interceptGroundProjectiles && def.projectile.flyOverhead)
		{
			return false;
		}
		if (interceptorComp.Props.interceptAirProjectiles && !def.projectile.flyOverhead)
		{
			return false;
		}
		if ((launcher == null || !launcher.HostileTo(interceptorThing)) && !interceptorComp.debugInterceptNonHostileProjectiles && !interceptorComp.Props.interceptNonHostileProjectiles)
		{
			return false;
		}
		if (!interceptorComp.Props.interceptOutgoingProjectiles && (vector - LastPos).sqrMagnitude <= num * num)
		{
			return false;
		}
		if (!CE_Utility.IntersectionPoint(LastPos, vector2, vector, num, out var sect, catchOutbound: true, interceptorComp.Props.interceptAirProjectiles && def.projectile.flyOverhead))
		{
			return false;
		}
		vector2 = (ExactPosition = sect.OrderBy((Vector3 x) => (OriginIV3.ToVector3() - x).sqrMagnitude).First());
		landed = true;
		interceptorComp.lastInterceptAngle = LastPos.AngleToFlat(interceptorThing.TrueCenter());
		interceptorComp.lastInterceptTicks = Find.TickManager.TicksGame;
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		if (Rand.Chance(projectilePropertiesCE?.empShieldBreakChance ?? 0f) && interceptorComp.Props.disarmedByEmpForTicks > 0)
		{
			DamageDef damageDef = ((def.projectile.damageDef == DamageDefOf.EMP) ? def.projectile.damageDef : projectilePropertiesCE?.secondaryDamage?.Select((SecondaryDamage sd) => sd.def).FirstOrDefault((DamageDef sdDef) => sdDef == DamageDefOf.EMP));
			if (damageDef != null)
			{
				interceptorComp.BreakShieldEmp(new DamageInfo(damageDef, damageDef.defaultDamage));
				interceptorComp.currentHitPoints = 0;
				interceptorComp.nextChargeTick = Find.TickManager.TicksGame;
			}
		}
		if (interceptorComp.currentHitPoints > 0)
		{
			interceptorComp.currentHitPoints -= Mathf.FloorToInt(DamageAmount);
			if (interceptorComp.currentHitPoints <= 0)
			{
				interceptorComp.currentHitPoints = 0;
				interceptorComp.nextChargeTick = Find.TickManager.TicksGame;
				interceptorComp.BreakShieldHitpoints(new DamageInfo(projectilePropertiesCE.damageDef, DamageAmount));
				return true;
			}
		}
		Effecter effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
		effecter.Trigger(new TargetInfo(vector2.ToIntVec3(), interceptorThing.Map), TargetInfo.Invalid);
		effecter.Cleanup();
		return true;
	}

	protected bool CheckForCollisionBetween()
	{
		bool result = false;
		Map map = base.Map;
		IntVec3 intVec = LastPos.ToIntVec3();
		IntVec3 intVec2 = ExactPosition.ToIntVec3();
		List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
		for (int i = 0; i < list.Count; i++)
		{
			if (CheckIntercept(list[i], list[i].TryGetComp<CompProjectileInterceptor>()))
			{
				if (def.projectile.flyOverhead)
				{
					Destroy();
					return true;
				}
				landed = true;
				Impact(null);
				return true;
			}
		}
		if (BlockerRegistry.CheckForCollisionBetweenCallback(this, LastPos, ExactPosition))
		{
			return true;
		}
		if (ticksToImpact < 0 || def.projectile.flyOverhead)
		{
			return false;
		}
		if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
		{
			return false;
		}
		if (Controller.settings.DebugDrawInterceptChecks)
		{
			base.Map.debugDrawer.FlashLine(intVec, intVec2);
		}
		IOrderedEnumerable<IntVec3> orderedEnumerable = from x in GenSight.PointsOnLineOfSight(intVec, intVec2).Union(new IntVec3[2] { intVec, intVec2 }).Distinct()
			orderby (x.ToVector3Shifted() - LastPos).MagnitudeHorizontalSquared()
			select x;
		foreach (IntVec3 item in orderedEnumerable)
		{
			if (CheckCellForCollision(item))
			{
				intVec2 = item;
				result = true;
				break;
			}
			if (Controller.settings.DebugDrawInterceptChecks)
			{
				base.Map.debugDrawer.FlashCell(item, 1f, "o");
			}
		}
		if (ExactPosition.y <= 3f)
		{
			RayCastSuppression(intVec, intVec2, map);
		}
		return result;
	}

	protected bool CheckCellForCollision(IntVec3 cell)
	{
		if (BlockerRegistry.CheckCellForCollisionCallback(this, cell, launcher))
		{
			return true;
		}
		bool flag = false;
		potentialCollisionCandidates.Clear();
		foreach (Thing item in base.Map.thingGrid.ThingsListAtFast(cell))
		{
			if (item is Pawn || item.def.Fillage != 0)
			{
				potentialCollisionCandidates.AddDistinct(item);
			}
		}
		Rot4 rot = Rot4.FromAngleFlat(shotRotation);
		if (rot.rotInt > 1)
		{
			rot = rot.Opposite;
		}
		if (Controller.settings.DebugDrawInterceptChecks)
		{
			base.Map.debugDrawer.debugCells.Clear();
			base.Map.debugDrawer.DebugDrawerUpdate();
		}
		foreach (IntVec3 item2 in GenAdj.CellsAdjacentCardinal(cell, rot, new IntVec2(5, 0)))
		{
			if (item2 == cell || !item2.InBounds(base.Map))
			{
				continue;
			}
			foreach (Thing item3 in base.Map.thingGrid.ThingsListAtFast(item2))
			{
				if (item3 is Pawn)
				{
					potentialCollisionCandidates.AddDistinct(item3);
				}
			}
			if (Controller.settings.DebugDrawInterceptChecks)
			{
				base.Map.debugDrawer.FlashCell(item2, 0.7f);
			}
		}
		if (LastPos.y > 2f)
		{
			if (TryCollideWithRoof(cell))
			{
				return true;
			}
			flag = true;
		}
		potentialCollisionCandidates.SortBy((Thing thing) => (thing.DrawPos - LastPos).sqrMagnitude);
		foreach (Thing potentialCollisionCandidate in potentialCollisionCandidates)
		{
			if (potentialCollisionCandidate is ProjectileCE || ((potentialCollisionCandidate == launcher || potentialCollisionCandidate == mount) && !canTargetSelf) || (potentialCollisionCandidate != intendedTargetThing && !def.projectile.alwaysFreeIntercept && !(potentialCollisionCandidate.Position.DistanceTo(OriginIV3) >= minCollisionDistance)) || !CanCollideWith(potentialCollisionCandidate, out var _))
			{
				continue;
			}
			if (BlockerRegistry.CheckForCollisionBetweenCallback(this, LastPos, potentialCollisionCandidate.TrueCenter()))
			{
				return true;
			}
			IntVec3 intVec = LastPos.ToIntVec3();
			IntVec3 intVec2 = potentialCollisionCandidate.TrueCenter().ToIntVec3();
			IOrderedEnumerable<IntVec3> orderedEnumerable = from x in GenSight.PointsOnLineOfSight(intVec, intVec2).Union(new IntVec3[2] { intVec, intVec2 }).Distinct()
				orderby (x.ToVector3Shifted() - LastPos).MagnitudeHorizontalSquared()
				select x;
			foreach (IntVec3 item4 in orderedEnumerable)
			{
				bool flag2 = false;
				flag2 = BlockerRegistry.CheckCellForCollisionCallback(this, item4, launcher);
				if (Controller.settings.DebugDrawInterceptChecks && base.Map != null)
				{
					base.Map.debugDrawer.FlashCell(item4, 1f, "a");
				}
				if (flag2)
				{
					return true;
				}
			}
			if (!TryCollideWith(potentialCollisionCandidate))
			{
				continue;
			}
			return true;
		}
		if (!flag && TryCollideWithRoof(cell))
		{
			return true;
		}
		return false;
	}

	protected virtual bool TryCollideWithRoof(IntVec3 cell)
	{
		if (!cell.Roofed(base.Map) || ignoreRoof)
		{
			return false;
		}
		if (!CE_Utility.GetBoundsFor(cell, cell.GetRoof(base.Map)).IntersectRay(ShotLine, out var distance))
		{
			return false;
		}
		if (distance * distance > (ExactPosition - LastPos).sqrMagnitude)
		{
			return false;
		}
		Vector3 point = ShotLine.GetPoint(distance);
		ExactPosition = point;
		landed = true;
		if (Controller.settings.DebugDrawInterceptChecks)
		{
			MoteMakerCE.ThrowText(cell.ToVector3Shifted(), base.Map, "x", Color.red);
		}
		Impact(null);
		return true;
	}

	protected bool CanCollideWith(Thing thing, out float dist)
	{
		dist = -1f;
		if (globalTargetInfo.IsValid)
		{
			return false;
		}
		if (thing == launcher && !canTargetSelf)
		{
			return false;
		}
		if (!CE_Utility.GetBoundsFor(thing).IntersectRay(ShotLine, out dist))
		{
			return false;
		}
		if (dist * dist > (ExactPosition - LastPos).sqrMagnitude)
		{
			return false;
		}
		return true;
	}

	protected bool TryCollideWith(Thing thing)
	{
		if (!CanCollideWith(thing, out var dist))
		{
			return false;
		}
		if (thing is Plant)
		{
			float num = (def.projectile.alwaysFreeIntercept ? 1f : ((thing.Position - OriginIV3).LengthHorizontal / 40f * AccuracyFactor));
			float chance = thing.def.fillPercent * num;
			if (Controller.settings.DebugShowTreeCollisionChance)
			{
				MoteMakerCE.ThrowText(thing.Position.ToVector3Shifted(), thing.Map, chance.ToString());
			}
			if (!Rand.ChanceSeeded(chance, thing.HashOffsetTicks()))
			{
				return false;
			}
		}
		Vector3 point = ShotLine.GetPoint(dist);
		if (!point.InBounds(base.Map))
		{
			if (OffMapOrigin)
			{
				landed = true;
				Destroy();
				return true;
			}
			string[] obj = new string[8]
			{
				"TryCollideWith out of bounds point from ShotLine: obj ",
				thing.ThingID,
				", proj ",
				base.ThingID,
				", dist ",
				dist.ToString(),
				", point ",
				null
			};
			Vector3 vector = point;
			obj[7] = vector.ToString();
			Log.Error(string.Concat(obj));
		}
		if (BlockerRegistry.BeforeCollideWithCallback(this, thing))
		{
			return true;
		}
		ExactPosition = point;
		landed = true;
		if (Controller.settings.DebugDrawInterceptChecks)
		{
			MoteMakerCE.ThrowText(thing.Position.ToVector3Shifted(), thing.Map, "x", Color.red);
		}
		Impact(thing);
		return true;
	}

	protected void ApplySuppression(Pawn pawn, float suppressionMultiplier = 1f)
	{
		if (pawn == null)
		{
			return;
		}
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		if (projectilePropertiesCE.suppressionFactor <= 0f || (!landed && projectilePropertiesCE.airborneSuppressionFactor <= 0f))
		{
			return;
		}
		CompShield compShield = pawn.TryGetComp<CompShield>();
		RaceProperties raceProps = pawn.RaceProps;
		if (raceProps != null && raceProps.Humanlike)
		{
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				CompShield compShield2 = wornApparel[i].TryGetComp<CompShield>();
				if (compShield2 != null)
				{
					compShield = compShield2;
					break;
				}
			}
		}
		CompSuppressable compSuppressable = pawn.TryGetComp<CompSuppressable>();
		if (compSuppressable != null && pawn.Faction != launcher?.Faction && (compShield == null || compShield.ShieldState == ShieldState.Resetting) && !compSuppressable.IgnoreSuppresion(OriginIV3))
		{
			suppressionAmount = (float)def.projectile.damageAmountBase * suppressionMultiplier;
			suppressionAmount *= projectilePropertiesCE.suppressionFactor;
			if (!landed)
			{
				suppressionAmount *= projectilePropertiesCE.airborneSuppressionFactor;
			}
			float num = projectilePropertiesCE.explosionRadius;
			if (num == 0f && this.TryGetComp<CompExplosiveCE>()?.props is CompProperties_ExplosiveCE compProperties_ExplosiveCE)
			{
				num = compProperties_ExplosiveCE.explosiveRadius;
				suppressionAmount = compProperties_ExplosiveCE.damageAmountBase;
			}
			if (num == 0f)
			{
				float num2 = projectilePropertiesCE?.armorPenetrationSharp ?? 0f;
				float num3 = ((num2 <= 0f) ? 0f : (1f - Mathf.Clamp(pawn.GetStatValue(CE_StatDefOf.AverageSharpArmor) * 0.5f / num2, 0f, 1f)));
				suppressionAmount *= num3;
			}
			else
			{
				float num4 = ExactPosition.x - pawn.DrawPos.x;
				float num5 = ExactPosition.z - pawn.DrawPos.z;
				float num6 = num + 3f;
				float num7 = Mathf.Clamp01(1f - (num4 * num4 + num5 * num5) / (num6 * num6));
				suppressionAmount *= num7;
			}
			compSuppressable.AddSuppression(suppressionAmount, OriginIV3);
		}
	}

	protected Vector3 MoveForward()
	{
		Vector3 vector = ExactPosition;
		float f = shotRotation * ((float)Math.PI / 180f) + 1.570795f;
		if (!kinit)
		{
			kinit = true;
			ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
			ballisticCoefficient = projectilePropertiesCE.ballisticCoefficient.RandomInRange;
			mass = projectilePropertiesCE.mass.RandomInRange;
			radius = projectilePropertiesCE.diameter.RandomInRange / 2000f;
			gravity = projectilePropertiesCE.Gravity;
			float num = shotSpeed / 60f;
			velocity = new Vector3(Mathf.Cos(f) * Mathf.Cos(shotAngle) * num, Mathf.Sin(shotAngle) * num, Mathf.Sin(f) * Mathf.Cos(shotAngle) * num);
			initialSpeed = num;
		}
		Accelerate();
		Vector3 result = vector + velocity;
		shotSpeed = velocity.magnitude;
		return result;
	}

	protected virtual void Accelerate()
	{
		AffectedByDrag();
		AffectedByGravity();
	}

	protected void AffectedByGravity()
	{
		velocity.y -= gravity / 60f;
	}

	protected void AffectedByDrag()
	{
		float num = radius;
		num *= num * 3.14159f;
		float num2 = 2.5f * shotSpeed * shotSpeed;
		float num3 = num2 * num / ballisticCoefficient;
		float num4 = (0f - num3) / mass;
		Vector3 normalized = velocity.normalized;
		velocity.x += num4 * normalized.x;
		velocity.y += num4 * normalized.y;
		velocity.z += num4 * normalized.z;
	}

	public override void Tick()
	{
		base.Tick();
		if (landed)
		{
			return;
		}
		LastPos = ExactPosition;
		ticksToImpact--;
		FlightTicks++;
		Vector3 vector2;
		if (lerpPosition)
		{
			Vector2 vector = Vec2Position();
			vector2 = new Vector3(vector.x, GetHeightAtTicks(FlightTicks), vector.y);
		}
		else
		{
			vector2 = MoveForward();
		}
		if (!vector2.InBounds(base.Map))
		{
			if (globalTargetInfo.IsValid)
			{
				TravelingShell travelingShell = (TravelingShell)WorldObjectMaker.MakeWorldObject(CE_WorldObjectDefOf.TravelingShell);
				if (launcher?.Faction != null)
				{
					travelingShell.SetFaction(launcher.Faction);
				}
				((WorldObject)travelingShell).tileInt = base.Map.Tile;
				travelingShell.SpawnSetup();
				Find.World.worldObjects.Add(travelingShell);
				travelingShell.launcher = launcher;
				travelingShell.equipmentDef = equipmentDef;
				travelingShell.globalSource = new GlobalTargetInfo(OriginIV3, base.Map);
				travelingShell.globalSource.tileInt = base.Map.Tile;
				travelingShell.globalSource.mapInt = base.Map;
				travelingShell.globalSource.worldObjectInt = base.Map.Parent;
				travelingShell.shellDef = def;
				travelingShell.globalTarget = globalTargetInfo;
				if (!travelingShell.TryTravel(base.Map.Tile, globalTargetInfo.Tile))
				{
					Log.Error($"CE: Travling shell {def} failed to launch!");
					travelingShell.Destroy();
				}
			}
			Destroy();
			return;
		}
		ExactPosition = vector2;
		if (CheckForCollisionBetween())
		{
			return;
		}
		base.Position = vector2.ToIntVec3();
		if (globalTargetInfo.IsValid)
		{
			return;
		}
		if (ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && def.projectile.soundImpactAnticipate != null)
		{
			def.projectile.soundImpactAnticipate.PlayOneShot(this);
		}
		if ((lerpPosition && ticksToImpact <= 0) || vector2.y <= 0f)
		{
			ImpactSomething();
			return;
		}
		if (ambientSustainer != null)
		{
			ambientSustainer.Maintain();
		}
		if (def.HasModExtension<TrailerProjectileExtension>())
		{
			TrailerProjectileExtension modExtension = def.GetModExtension<TrailerProjectileExtension>();
			if (modExtension != null && ticksToImpact % modExtension.trailerMoteInterval == 0)
			{
				for (int i = 0; i < modExtension.motesThrown; i++)
				{
					TrailThrower.ThrowSmoke(DrawPos, modExtension.trailMoteSize, base.Map, modExtension.trailMoteDef);
				}
			}
		}
		float num = OriginIV3.DistanceTo(((Thing)this).positionInt);
		float dangerFactor = (def.projectile as ProjectilePropertiesCE).dangerFactor;
		if (dangerFactor > 0f && vector2.y < 2f && num > 3f)
		{
			DangerTracker?.Notify_BulletAt(base.Position, (float)def.projectile.damageAmountBase * dangerFactor);
		}
		if (ignoreRoof && def.projectile.flyOverhead && shotAngle < 0f)
		{
			ignoreRoof = false;
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (FlightTicks != 0 || launcher == null || !(launcher is Pawn))
		{
			Quaternion exactRotation = ExactRotation;
			Quaternion drawRotation = DrawRotation;
			if (def.projectile.spinRate != 0f)
			{
				float num = 60f / def.projectile.spinRate;
				Quaternion quaternion = Quaternion.AngleAxis((float)Find.TickManager.TicksGame % num / num * 360f, Vector3.up);
				exactRotation *= quaternion;
				drawRotation *= quaternion;
			}
			Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), drawLoc, drawRotation, def.DrawMatSingle, 0);
			if (castShadow)
			{
				Graphics.DrawMesh(position: new Vector3(ExactPosition.x, def.Altitude - 0.001f, ExactPosition.z), mesh: MeshPool.GridPlane(def.graphicData.drawSize), rotation: exactRotation, material: ShadowMaterial, layer: 0);
			}
			Comps_PostDraw();
		}
	}

	public void ImpactSomething()
	{
		if (BlockerRegistry.ImpactSomethingCallback(this, launcher))
		{
			return;
		}
		IntVec3 intVec = ExactPosition.ToIntVec3();
		if (def.projectile.flyOverhead)
		{
			RoofDef roofDef = base.Map.roofGrid.RoofAt(intVec);
			if (roofDef != null)
			{
				if (roofDef.isThickRoof)
				{
					def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(intVec, base.Map));
					Destroy();
					return;
				}
				if (intVec.GetEdifice(base.Map) == null || intVec.GetEdifice(base.Map).def.Fillage != FillCategory.Full)
				{
					RoofCollapserImmediate.DropRoofInCells(intVec, base.Map);
				}
			}
		}
		Thing firstPawn = intVec.GetFirstPawn(base.Map);
		if (firstPawn != null && TryCollideWith(firstPawn))
		{
			return;
		}
		List<Thing> list = (from t in base.Map.thingGrid.ThingsListAt(intVec)
			where t is Pawn || t.def.Fillage != FillCategory.None
			select t).ToList();
		if (list.Count > 0)
		{
			foreach (Thing item in list)
			{
				if (TryCollideWith(item))
				{
					return;
				}
			}
		}
		ExactPosition = ExactPosition;
		landed = true;
		Impact(null);
	}

	public virtual void Impact(Thing hitThing)
	{
		if (Controller.settings.EnableExtraEffects)
		{
			ImpactFleckThrower.ThrowFleck(ExactPosition, base.Position, base.Map, def.projectile as ProjectilePropertiesCE, def, hitThing, shotRotation);
		}
		List<Thing> list = new List<Thing>();
		if (base.Position.IsValid && def.projectile.preExplosionSpawnChance > 0f && def.projectile.preExplosionSpawnThingDef != null && (Controller.settings.EnableAmmoSystem || !(def.projectile.preExplosionSpawnThingDef is AmmoDef)) && Rand.Value < def.projectile.preExplosionSpawnChance)
		{
			ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
			if (preExplosionSpawnThingDef.IsFilth && base.Position.Walkable(base.Map))
			{
				FilthMaker.TryMakeFilth(base.Position, base.Map, preExplosionSpawnThingDef);
			}
			else if (Controller.settings.ReuseNeolithicProjectiles)
			{
				Thing thing = ThingMaker.MakeThing(preExplosionSpawnThingDef);
				thing.stackCount = 1;
				thing.SetForbidden(value: true, warnOnFail: false);
				GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near);
				LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_ReusableNeolithicProjectiles, thing, OpportunityType.GoodToKnow);
				list.Add(thing);
			}
		}
		Vector3 vector = ExactPosition;
		if (!vector.ToIntVec3().IsValid)
		{
			Destroy();
			return;
		}
		if (def.projectile.explosionEffect != null)
		{
			Effecter effecter = def.projectile.explosionEffect.Spawn();
			effecter.Trigger(new TargetInfo(vector.ToIntVec3(), base.Map), new TargetInfo(vector.ToIntVec3(), base.Map));
			effecter.Cleanup();
		}
		if (def.projectile.landedEffecter != null)
		{
			def.projectile.landedEffecter.Spawn(base.Position, base.Map).Cleanup();
		}
		ProjectilePropertiesCE projectilePropertiesCE = def.projectile as ProjectilePropertiesCE;
		float scale = ((projectilePropertiesCE.detonateEffectsScaleOverride > 0f) ? projectilePropertiesCE.detonateEffectsScaleOverride : (projectilePropertiesCE.explosionRadius * 2f));
		if (projectilePropertiesCE.detonateMoteDef != null)
		{
			MoteMaker.MakeStaticMote(DrawPos, base.Map, CE_ThingDefOf.Mote_BigExplode, scale);
		}
		if (projectilePropertiesCE.detonateFleckDef != null)
		{
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(DrawPos, base.MapHeld, projectilePropertiesCE.detonateFleckDef, scale);
			base.MapHeld.flecks.CreateFleck(dataStatic);
		}
		ProjectilePropertiesCE projectilePropertiesCE2 = def.projectile as ProjectilePropertiesCE;
		CompExplosiveCE compExplosiveCE = this.TryGetComp<CompExplosiveCE>();
		if (compExplosiveCE == null)
		{
			foreach (CompFragments comp in GetComps<CompFragments>())
			{
				comp.Throw(vector, base.Map, launcher);
			}
		}
		if (compExplosiveCE != null || (def.projectile.explosionRadius > 0f && def.projectile.damageDef != null))
		{
			float num = 3f + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f);
			if (hitThing is Pawn { Dead: not false } pawn)
			{
				list.Add(pawn.Corpse);
			}
			List<Pawn> list2 = new List<Pawn>();
			float num2 = 0f;
			float? direction = origin.AngleTo(Vec2Position());
			if (def.projectile.explosionRadius > 0f)
			{
				IntVec3 center = vector.ToIntVec3();
				Map map = base.Map;
				float explosionRadius = def.projectile.explosionRadius;
				DamageDef damageDef = def.projectile.damageDef;
				Thing instigator = launcher;
				int damAmount = Mathf.FloorToInt(DamageAmount);
				float explosionArmorPenetration = def.projectile.GetExplosionArmorPenetration();
				SoundDef soundExplode = def.projectile.soundExplode;
				ThingDef weapon = equipmentDef;
				ThingDef projectile = def;
				ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
				float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
				int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
				GasType? postExplosionGasType = def.projectile.postExplosionGasType;
				bool applyDamageToExplosionCellsNeighbors = def.projectile.applyDamageToExplosionCellsNeighbors;
				ThingDef preExplosionSpawnThingDef2 = def.projectile.preExplosionSpawnThingDef;
				float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
				int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
				float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
				bool explosionDamageFalloff = def.projectile.explosionDamageFalloff;
				ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
				float screenShakeFactor = def.projectile.screenShakeFactor;
				float y = vector.y;
				GenExplosionCE.DoExplosion(center, map, explosionRadius, damageDef, instigator, damAmount, explosionArmorPenetration, soundExplode, weapon, projectile, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef2, preExplosionSpawnChance, preExplosionSpawnThingCount, explosionChanceToStartFire, explosionDamageFalloff, direction, list, null, doVisualEffects: true, 1f, 0f, doSoundEffects: true, postExplosionSpawnThingDefWater, screenShakeFactor, null, null, y);
				num2 = def.projectile.damageAmountBase;
				if (vector.y < 3f)
				{
					num += def.projectile.explosionRadius;
					if (projectilePropertiesCE2.suppressionFactor > 0f)
					{
						list2.AddRange(vector.ToIntVec3().PawnsInRange(base.Map, num));
					}
				}
			}
			if (compExplosiveCE != null)
			{
				num2 = (compExplosiveCE.props as CompProperties_ExplosiveCE).damageAmountBase;
				compExplosiveCE.Explode(this, vector, base.Map, 1f, direction, list);
				if (vector.y < 3f)
				{
					num += (compExplosiveCE.props as CompProperties_ExplosiveCE).explosiveRadius;
					if (projectilePropertiesCE2.suppressionFactor > 0f)
					{
						list2.AddRange(vector.ToIntVec3().PawnsInRange(base.Map, 3f + (compExplosiveCE.props as CompProperties_ExplosiveCE).explosiveRadius));
					}
				}
			}
			foreach (Pawn item in list2)
			{
				ApplySuppression(item);
			}
			if (projectilePropertiesCE2.dangerFactor > 0f)
			{
				DangerTracker.Notify_DangerRadiusAt(base.Position, num - 3f, num2 * projectilePropertiesCE2.dangerFactor);
			}
		}
		else if (projectilePropertiesCE2.dangerFactor > 0f)
		{
			DangerTracker?.Notify_BulletAt(ExactPosition.ToIntVec3(), (float)def.projectile.damageAmountBase * projectilePropertiesCE2.dangerFactor);
		}
		Destroy();
	}

	protected float GetHeightAtTicks(int ticks)
	{
		float num = (float)ticks / 60f;
		return (float)Math.Round(shotHeight + shotSpeed * Mathf.Sin(shotAngle) * num - GravityFactor * num * num / 2f, 3);
	}

	protected float GetFlightTime()
	{
		return (Mathf.Sin(shotAngle) * shotSpeed + Mathf.Sqrt(Mathf.Pow(Mathf.Sin(shotAngle) * shotSpeed, 2f) + 2f * GravityFactor * shotHeight)) / GravityFactor;
	}

	public static float GetShotAngle(float velocity, float range, float heightDifference, bool flyOverhead, float gravity)
	{
		float num = Mathf.Sqrt(Mathf.Pow(velocity, 4f) - gravity * (gravity * Mathf.Pow(range, 2f) + 2f * heightDifference * Mathf.Pow(velocity, 2f)));
		if (float.IsNaN(num))
		{
			Log.Warning("[CE] Tried to fire projectile to unreachable target cell, truncating to maximum distance.");
			return (float)Math.PI / 4f;
		}
		return Mathf.Atan((Mathf.Pow(velocity, 2f) + (flyOverhead ? 1f : (-1f)) * num) / (gravity * range));
	}

	protected static Material[] GetShadowMaterial(Graphic_Collection g)
	{
		Graphic[] subGraphics = g.subGraphics;
		return subGraphics.Select((Graphic item) => item.GetColoredVersion(ShaderDatabase.Transparent, Color.black, Color.black).MatSingle).ToArray();
	}
}
