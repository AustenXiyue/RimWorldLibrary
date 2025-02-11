using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class Verb_LaunchProjectileCE : Verb
{
	private float estimatedTargDist = -1f;

	protected int numShotsFired = 0;

	protected Vector2 newTargetLoc = new Vector2(0f, 0f);

	protected Vector2 sourceLoc = new Vector2(0f, 0f);

	protected float shotAngle = 0f;

	protected float shotRotation = 0f;

	protected float distance = 10f;

	public CompCharges compCharges = null;

	public CompAmmoUser compAmmo = null;

	public CompFireModes compFireModes = null;

	public CompChangeableProjectile compChangeable = null;

	public CompApparelReloadable compReloadable = null;

	private float shotSpeed = -1f;

	private float rotationDegrees = 0f;

	private float angleRadians = 0f;

	private bool shootingAtDowned = false;

	private LocalTargetInfo lastTarget = null;

	protected IntVec3 lastTargetPos = IntVec3.Invalid;

	protected float lastShotAngle;

	protected float lastShotRotation;

	protected float lastRecoilDeg;

	protected float? storedShotReduction = null;

	protected ShootLine? lastShootLine;

	protected bool repeating = false;

	protected bool doRetarget = true;

	public bool isTurretMannable = false;

	private LightingTracker _lightingTracker = null;

	private List<IntVec3> tempLeanShootSources = new List<IntVec3>();

	public VerbPropertiesCE VerbPropsCE => verbProps as VerbPropertiesCE;

	public ProjectilePropertiesCE projectilePropsCE => Projectile.projectile as ProjectilePropertiesCE;

	public Pawn ShooterPawn => CasterPawn ?? CE_Utility.TryGetTurretOperator(caster);

	public Thing Shooter => ShooterPawn ?? caster;

	protected override int ShotsPerBurst
	{
		public get
		{
			float num = base.ShotsPerBurst;
			if (base.EquipmentSource != null)
			{
				float statValue = base.EquipmentSource.GetStatValue(CE_StatDefOf.BurstShotCount);
				if (statValue > 0f)
				{
					num = statValue;
				}
			}
			return (int)num;
		}
	}

	public CompCharges CompCharges
	{
		get
		{
			if (compCharges == null && base.EquipmentSource != null)
			{
				compCharges = base.EquipmentSource.TryGetComp<CompCharges>();
			}
			return compCharges;
		}
	}

	protected float ShotSpeed
	{
		get
		{
			if (CompCharges != null)
			{
				if (CompCharges.GetChargeBracket((currentTarget.Cell - caster.Position).LengthHorizontal, ShotHeight, projectilePropsCE.Gravity, out var bracket))
				{
					shotSpeed = bracket.x;
				}
			}
			else
			{
				shotSpeed = Projectile.projectile.speed;
			}
			return shotSpeed;
		}
	}

	public virtual float ShotHeight => new CollisionVertical(caster).shotHeight;

	private Vector3 ShotSource
	{
		get
		{
			Vector3 drawPos = caster.DrawPos;
			return new Vector3(drawPos.x, ShotHeight, drawPos.z);
		}
	}

	public float ShootingAccuracy
	{
		get
		{
			isTurretMannable = Caster.TryGetComp<CompMannable>() != null;
			return Mathf.Min(CasterShootingAccuracyValue(Shooter), 4.5f);
		}
	}

	public float AimingAccuracy => Mathf.Min(Shooter.GetStatValue(CE_StatDefOf.AimingAccuracy), 1.5f);

	public float SightsEfficiency => base.EquipmentSource?.GetStatValue(CE_StatDefOf.SightsEfficiency) ?? 1f;

	public virtual float SwayAmplitude => Mathf.Max(0f, (4.5f - ShootingAccuracy) * (base.EquipmentSource?.GetStatValue(CE_StatDefOf.SwayFactor) ?? 1f));

	public virtual CompAmmoUser CompAmmo => compAmmo ?? (compAmmo = base.EquipmentSource?.TryGetComp<CompAmmoUser>());

	public virtual ThingDef Projectile
	{
		get
		{
			if (CompChangeable != null && CompChangeable.Loaded)
			{
				return CompChangeable.Projectile;
			}
			return VerbPropsCE.defaultProjectile;
		}
	}

	public CompChangeableProjectile CompChangeable
	{
		get
		{
			if (compChangeable == null && base.EquipmentSource != null)
			{
				compChangeable = base.EquipmentSource.TryGetComp<CompChangeableProjectile>();
			}
			return compChangeable;
		}
	}

	public virtual CompFireModes CompFireModes
	{
		get
		{
			if (compFireModes == null && base.EquipmentSource != null)
			{
				compFireModes = base.EquipmentSource.TryGetComp<CompFireModes>();
			}
			return compFireModes;
		}
	}

	public CompApparelReloadable CompReloadable
	{
		get
		{
			if (compReloadable == null && base.EquipmentSource != null)
			{
				compReloadable = base.EquipmentSource.TryGetComp<CompApparelReloadable>();
			}
			return compReloadable;
		}
	}

	public float RecoilAmount
	{
		get
		{
			float result = VerbPropsCE.recoilAmount;
			if (base.EquipmentSource != null)
			{
				float statValue = base.EquipmentSource.GetStatValue(CE_StatDefOf.Recoil);
				if (statValue > 0f)
				{
					result = statValue;
				}
			}
			return result;
		}
	}

	protected LightingTracker LightingTracker
	{
		get
		{
			if (_lightingTracker == null || _lightingTracker.map == null || _lightingTracker.map.Index < 0 || _lightingTracker.map != caster.Map)
			{
				_lightingTracker = caster.Map.GetLightingTracker();
			}
			return _lightingTracker;
		}
	}

	public override void Reset()
	{
		base.Reset();
		resetRetarget();
	}

	protected void resetRetarget()
	{
		shootingAtDowned = false;
		lastTarget = null;
		lastTargetPos = IntVec3.Invalid;
		lastShootLine = null;
		repeating = false;
		storedShotReduction = null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref shootingAtDowned, "shootingAtDowned", defaultValue: false);
		Scribe_TargetInfo.Look(ref lastTarget, "lastTarget");
		Scribe_Values.Look(ref lastTargetPos, "lastTargetPos", IntVec3.Invalid);
		Scribe_Values.Look(ref repeating, "repeating", defaultValue: false);
		Scribe_Values.Look(ref lastShotAngle, "lastShotAngle", 0f);
		Scribe_Values.Look(ref lastShotRotation, "lastShotRotation", 0f);
		Scribe_Values.Look(ref lastRecoilDeg, "lastRecoilDeg", 0f);
	}

	public override bool Available()
	{
		if (!base.Available())
		{
			return false;
		}
		if (CasterIsPawn && CasterPawn.Faction != Faction.OfPlayer && CasterPawn.mindState.MeleeThreatStillThreat && CasterPawn.mindState.meleeThreat.AdjacentTo8WayOrInside(CasterPawn))
		{
			return false;
		}
		return Projectile != null;
	}

	private float CasterShootingAccuracyValue(Thing caster)
	{
		return (caster is Pawn) ? caster.GetStatValue(StatDefOf.ShootingAccuracyPawn) : caster.GetStatValue(StatDefOf.ShootingAccuracyTurret);
	}

	public override void WarmupComplete()
	{
		lastTarget = null;
		if (ShooterPawn == null || ShooterPawn.pather != null)
		{
			Pawn shooterPawn = ShooterPawn;
			if (shooterPawn != null && shooterPawn.Spawned && currentTarget.Thing is Pawn && Rand.Chance(0.25f))
			{
				((TauntThrower)ShooterPawn.Map.GetComponent(typeof(TauntThrower)))?.TryThrowTaunt(CE_RulePackDefOf.AttackMote, ShooterPawn);
			}
			numShotsFired = 0;
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(Shooter, (!currentTarget.HasThing) ? null : currentTarget.Thing, (base.EquipmentSource == null) ? null : base.EquipmentSource.def, Projectile, VerbPropsCE.burstShotCount > 1));
		}
	}

	public virtual void ShiftTarget(ShiftVecReport report, bool calculateMechanicalOnly = false, bool isInstant = false, bool midBurst = false)
	{
		float num;
		FloatRange heightRange2;
		if (!calculateMechanicalOnly)
		{
			Vector3 vector = caster.TrueCenter();
			sourceLoc.Set(vector.x, vector.z);
			if (numShotsFired == 0)
			{
				estimatedTargDist = report.GetRandDist();
			}
			Vector3 vector2 = report.target.Thing?.TrueCenter() ?? report.target.Cell.ToVector3Shifted();
			if (report.targetPawn != null)
			{
				vector2 += report.targetPawn.Drawer.leaner.LeanOffset * 0.5f;
			}
			newTargetLoc.Set(vector2.x, vector2.z);
			newTargetLoc += report.GetRandCircularVec();
			newTargetLoc = sourceLoc + (newTargetLoc - sourceLoc).normalized * estimatedTargDist;
			if (!isInstant)
			{
				newTargetLoc += report.GetRandLeadVec();
			}
			rotationDegrees = 0f;
			angleRadians = 0f;
			GetSwayVec(ref rotationDegrees, ref angleRadians);
			GetRecoilVec(ref rotationDegrees, ref angleRadians);
			num = 0f;
			FloatRange heightRange = new CollisionVertical(report.cover).HeightRange;
			if (Projectile.projectile.flyOverhead)
			{
				num = heightRange.max;
				goto IL_07d1;
			}
			CollisionVertical collisionVertical = new CollisionVertical(currentTarget.Thing);
			heightRange2 = collisionVertical.HeightRange;
			if (heightRange2.min < heightRange.max)
			{
				heightRange2.min = heightRange.max;
				if (heightRange2.max <= heightRange.max)
				{
					CompFireModes obj = CompFireModes;
					if ((obj != null && obj.CurrentAimMode == AimMode.SuppressFire) || VerbPropsCE.ignorePartialLoSBlocker)
					{
						heightRange2.max = heightRange.max * 2f;
					}
				}
			}
			else if (currentTarget.Thing is Pawn pawn)
			{
				heightRange2.min = collisionVertical.BottomHeight;
				heightRange2.max = collisionVertical.MiddleHeight;
				CompFireModes obj2 = CompFireModes;
				bool flag = obj2 == null || obj2.CurrentAimMode != AimMode.SuppressFire;
				if ((ShootingAccuracy >= 2.45f) | isTurretMannable)
				{
					if ((CasterPawn?.Faction ?? Caster.Faction) == Faction.OfPlayer)
					{
						CompFireModes obj3 = CompFireModes;
						if (obj3 == null || obj3.targetMode != TargettingMode.automatic)
						{
							if (flag)
							{
								CompFireModes obj4 = CompFireModes;
								if (obj4 != null && obj4.targetMode == TargettingMode.head)
								{
									heightRange2.min = collisionVertical.MiddleHeight;
									heightRange2.max = collisionVertical.Max;
								}
								else
								{
									CompFireModes obj5 = CompFireModes;
									if (obj5 != null && obj5.targetMode == TargettingMode.legs)
									{
										heightRange2.min = collisionVertical.Min;
										heightRange2.max = collisionVertical.BottomHeight;
									}
									else
									{
										heightRange2.min = collisionVertical.BottomHeight;
										heightRange2.max = collisionVertical.MiddleHeight;
									}
								}
							}
							goto IL_0785;
						}
					}
					if (pawn?.kindDef?.RaceProps?.Humanlike == true)
					{
						if (flag)
						{
							Func<Apparel, float> selector = (Apparel x) => x?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0.1f;
							BodyPartRecord Torso = (from X in pawn.health.hediffSet.GetNotMissingParts()
								where X.IsCorePart
								select X).First();
							List<Apparel> source = pawn?.apparel?.WornApparel?.FindAll((Apparel F) => F.def.apparel.CoversBodyPart(Torso)) ?? null;
							Apparel apparel = source.MaxByWithFallback(selector);
							BodyPartRecord Head = (from X in pawn.health.hediffSet.GetNotMissingParts()
								where X.def.defName == "Head"
								select X).FirstOrFallback(Torso);
							List<Apparel> source2 = pawn?.apparel?.WornApparel?.FindAll((Apparel F) => F.def.apparel.CoversBodyPart(Head)) ?? null;
							Apparel apparel2 = source2.MaxByWithFallback(selector);
							BodyPartRecord Leg = (from X in pawn.health.hediffSet.GetNotMissingParts()
								where X.def.defName == "Leg"
								select X).FirstOrFallback(Torso);
							List<Apparel> source3 = pawn?.apparel?.WornApparel?.FindAll((Apparel F) => F.def.apparel.CoversBodyPart(Leg)) ?? null;
							Apparel apparel3 = source3.MaxByWithFallback(selector);
							Apparel apparel4 = apparel;
							bool flag2 = (apparel?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0.1f) >= (apparel2?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0f);
							bool flag3 = projectilePropsCE.armorPenetrationSharp >= (apparel?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0.1f);
							if (flag2 && !flag3)
							{
								apparel4 = apparel2;
							}
							bool flag4 = (apparel4?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0f) >= (apparel3?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0f) + 4f;
							bool flag5 = projectilePropsCE.armorPenetrationSharp >= (apparel2?.GetStatValue(StatDefOf.ArmorRating_Sharp) ?? 0.1f);
							if (flag4 && !flag5 && !flag3)
							{
								apparel4 = apparel3;
							}
							if (apparel4 == apparel2)
							{
								heightRange2.min = collisionVertical.MiddleHeight;
								heightRange2.max = collisionVertical.Max;
							}
							else if (apparel4 == apparel3)
							{
								heightRange2.min = collisionVertical.Min;
								heightRange2.max = collisionVertical.BottomHeight;
							}
						}
						else
						{
							heightRange2.min = collisionVertical.MiddleHeight;
							heightRange2.max = collisionVertical.Max;
						}
					}
				}
			}
			goto IL_0785;
		}
		goto IL_0889;
		IL_0785:
		num = heightRange2.Average;
		if (projectilePropsCE != null)
		{
			num += projectilePropsCE.aimHeightOffset;
		}
		if (num > 2f && report.roofed)
		{
			num = 2f;
		}
		goto IL_07d1;
		IL_07d1:
		if (!midBurst)
		{
			if (projectilePropsCE.isInstant)
			{
				lastShotAngle = Mathf.Atan2(num - ShotHeight, (newTargetLoc - sourceLoc).magnitude);
			}
			else
			{
				lastShotAngle = ProjectileCE.GetShotAngle(ShotSpeed, (newTargetLoc - sourceLoc).magnitude, num - ShotHeight, Projectile.projectile.flyOverhead, projectilePropsCE.Gravity);
			}
		}
		angleRadians += lastShotAngle;
		goto IL_0889;
		IL_0889:
		Vector2 vector3 = ((projectilePropsCE.isInstant && projectilePropsCE.damageFalloff) ? new Vector2(0f, 0f) : report.GetRandSpreadVec());
		Vector2 vector4 = newTargetLoc - sourceLoc;
		if (!midBurst)
		{
			lastShotRotation = -90f + 57.29578f * Mathf.Atan2(vector4.y, vector4.x);
		}
		shotRotation = (lastShotRotation + rotationDegrees + vector3.x) % 360f;
		shotAngle = angleRadians + vector3.y * ((float)Math.PI / 180f);
		distance = (newTargetLoc - sourceLoc).magnitude;
	}

	private void GetRecoilVec(ref float rotation, ref float angle)
	{
		float recoilAmount = RecoilAmount;
		float num = recoilAmount * 0.5f;
		float minInclusive = 0f - num;
		float maxInclusive = recoilAmount;
		float minInclusive2 = (0f - recoilAmount) / 3f;
		float num2 = ((numShotsFired == 0) ? 0f : Mathf.Pow(5f - ShootingAccuracy, (float)Mathf.Min(10, numShotsFired) / 6.25f));
		float num3 = Mathf.Pow(5f - ShootingAccuracy, (float)Mathf.Min(10, numShotsFired + 1) / 6.25f);
		rotation += num2 * Rand.Range(minInclusive, num);
		float num4 = Rand.Range(minInclusive2, maxInclusive);
		angle += num2 * ((float)Math.PI / 180f) * num4;
		lastRecoilDeg += num3 * num4;
	}

	public void GetSwayVec(ref float rotation, ref float angle)
	{
		float num = Find.TickManager.TicksAbs + Shooter.thingIDNumber;
		rotation += SwayAmplitude * Mathf.Sin(num * 0.022f);
		angle += 0.004363323f * SwayAmplitude * Mathf.Sin(num * 0.0165f);
	}

	public virtual ShiftVecReport ShiftVecReportFor(LocalTargetInfo target)
	{
		IntVec3 cell = target.Cell;
		ShiftVecReport shiftVecReport = new ShiftVecReport();
		shiftVecReport.target = target;
		shiftVecReport.aimingAccuracy = AimingAccuracy;
		shiftVecReport.sightsEfficiency = SightsEfficiency;
		shiftVecReport.blindFiring = ShooterPawn != null && !ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight);
		if (ShooterPawn != null && !ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
		{
			shiftVecReport.sightsEfficiency = 0f;
		}
		shiftVecReport.shotDist = (cell - caster.Position).LengthHorizontal;
		shiftVecReport.maxRange = EffectiveRange;
		shiftVecReport.lightingShift = CE_Utility.GetLightingShift(Shooter, LightingTracker.CombatGlowAtFor(caster.Position, cell));
		if (!caster.Position.Roofed(caster.Map) || !cell.Roofed(caster.Map))
		{
			shiftVecReport.weatherShift = 1f - caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
		}
		shiftVecReport.shotSpeed = ShotSpeed;
		shiftVecReport.swayDegrees = SwayAmplitude;
		float num = ((projectilePropsCE != null) ? projectilePropsCE.spreadMult : 0f);
		shiftVecReport.spreadDegrees = (base.EquipmentSource?.GetStatValue(CE_StatDefOf.ShotSpread) ?? 0f) * num;
		GetHighestCoverAndSmokeForTarget(target, out var cover, out var smokeDensity, out var roofed);
		shiftVecReport.cover = cover;
		shiftVecReport.smokeDensity = smokeDensity;
		shiftVecReport.roofed = roofed;
		return shiftVecReport;
	}

	public virtual ShiftVecReport ShiftVecReportFor(GlobalTargetInfo target)
	{
		if (!target.IsValid || !target.Cell.IsValid || target.Map == null)
		{
			return null;
		}
		ProjectilePropertiesCE projectilePropertiesCE = Projectile.projectile as ProjectilePropertiesCE;
		if (projectilePropertiesCE.shellingProps == null)
		{
			Log.Error("CE: Tried to ShiftVecReportFor for a global target for a projectile " + Projectile.defName + " that doesn't have shellingInfo!");
			return null;
		}
		int num = Find.WorldGrid.TraversalDistanceBetween(target.Tile, caster.Map.Tile);
		LocalTargetInfo target2 = default(LocalTargetInfo);
		target2.cellInt = target.Cell;
		target2.thingInt = target.Thing;
		IntVec3 cell = target.Cell;
		ShiftVecReport shiftVecReport = new ShiftVecReport();
		shiftVecReport.target = target2;
		shiftVecReport.aimingAccuracy = AimingAccuracy;
		shiftVecReport.sightsEfficiency = SightsEfficiency;
		shiftVecReport.shotDist = num * 5;
		shiftVecReport.maxRange = projectilePropertiesCE.shellingProps.range * 5f;
		shiftVecReport.shotSpeed = ShotSpeed * 2.5f;
		shiftVecReport.swayDegrees = SwayAmplitude;
		float num2 = ((projectilePropsCE != null) ? projectilePropsCE.spreadMult : 0f);
		shiftVecReport.spreadDegrees = (base.EquipmentSource?.GetStatValue(CE_StatDefOf.ShotSpread) ?? 0f) * num2;
		shiftVecReport.cover = null;
		if (target.Map != null)
		{
			shiftVecReport.weatherShift = (1f - target.Map.weatherManager.CurWeatherAccuracyMultiplier) * 1.5f + (1f - caster.Map.weatherManager.CurWeatherAccuracyMultiplier) * 0.5f;
			shiftVecReport.lightingShift = 1f;
			shiftVecReport.smokeDensity = 10f;
		}
		else
		{
			shiftVecReport.smokeDensity = 0f;
			shiftVecReport.weatherShift = 1f;
			shiftVecReport.lightingShift = 1f;
		}
		return shiftVecReport;
	}

	public float AdjustShotHeight(Thing caster, LocalTargetInfo target, ref float shotHeight)
	{
		GetHighestCoverAndSmokeForTarget(target, out var cover, out var _, out var _);
		float y = CE_Utility.GetBoundsFor(caster).max.y;
		float y2 = CE_Utility.GetBoundsFor(cover).max.y;
		float num = (CE_Utility.GetBoundsFor(target.Thing).max.y - y2) / 2f + y2;
		if (num > shotHeight)
		{
			if (num > y)
			{
				num = y;
			}
			float x = target.Thing.Position.DistanceTo(caster.Position);
			float y3 = num - shotHeight;
			float result = 0f - Mathf.Atan2(y3, x);
			shotHeight = num;
			return result;
		}
		return 0f;
	}

	private bool GetHighestCoverAndSmokeForTarget(LocalTargetInfo target, out Thing cover, out float smokeDensity, out bool roofed)
	{
		Map map = caster.Map;
		Thing thing = target.Thing;
		Thing thing2 = null;
		float num = 0f;
		roofed = false;
		smokeDensity = 0f;
		List<IntVec3> list = GenSightCE.AllPointsOnLineOfSight(target.Cell, caster.Position);
		if (list.Count < 3)
		{
			cover = null;
			return false;
		}
		bool flag = false;
		if (Projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			flag = projectilePropertiesCE.isInstant;
		}
		int num2 = (flag ? list.Count : (list.Count / 2));
		for (int i = 0; i < num2; i++)
		{
			IntVec3 intVec = list[i];
			if (intVec.AdjacentTo8Way(caster.Position))
			{
				continue;
			}
			Gas gas = intVec.GetGas(map);
			if (intVec.AnyGas(map, GasType.BlindSmoke))
			{
				smokeDensity += 0.7f;
			}
			if (!roofed)
			{
				roofed = map.roofGrid.RoofAt(intVec) != null;
			}
			if (!flag && i > list.Count / 2)
			{
				continue;
			}
			Pawn firstPawn = intVec.GetFirstPawn(map);
			Thing thing3 = ((firstPawn == null) ? intVec.GetCover(map) : firstPawn);
			if (thing3 != null && (thing == null || !thing3.Equals(thing)) && thing3.def.Fillage == FillCategory.Partial && !thing3.IsPlant())
			{
				float max = new CollisionVertical(thing3).Max;
				if (thing2 == null || num < max)
				{
					thing2 = thing3;
					num = max;
				}
				if (Controller.settings.DebugDrawTargetCoverChecks)
				{
					map.debugDrawer.FlashCell(intVec, num, num.ToString());
				}
			}
		}
		cover = thing2;
		return cover != null;
	}

	public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
	{
		string report;
		return CanHitTargetFrom(root, targ, out report);
	}

	public bool CanHitTarget(LocalTargetInfo targ, out string report)
	{
		return CanHitTargetFrom(caster.Position, targ, out report);
	}

	public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ, out string report)
	{
		report = "";
		if (caster is Building_TurretGunCE { targetingWorldMap: not false } building_TurretGunCE && building_TurretGunCE.globalTargetInfo.IsValid)
		{
			return true;
		}
		if (caster?.Map == null || !targ.Cell.InBounds(caster.Map) || !root.InBounds(caster.Map))
		{
			report = "CE_OutofBounds".Translate();
			return false;
		}
		if (targ.Thing != null && targ.Thing == caster)
		{
			if (!verbProps.targetParams.canTargetSelf)
			{
				report = "CE_NoSelfTarget".Translate();
				return false;
			}
			return true;
		}
		if (Projectile.projectile.flyOverhead)
		{
			RoofDef roofDef = caster.Map.roofGrid.RoofAt(targ.Cell);
			if (roofDef != null && roofDef.isThickRoof)
			{
				report = "CE_BlockedRoof".Translate();
				return false;
			}
		}
		if (ShooterPawn != null)
		{
			if (ShooterPawn.story != null && ShooterPawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				report = "IsIncapableOfViolenceLower".Translate(ShooterPawn.Name.ToStringShort);
				return false;
			}
			bool flag = caster.def.building?.IsTurret ?? false;
			if (ShooterPawn.apparel != null)
			{
				List<Apparel> wornApparel = ShooterPawn.apparel.WornApparel;
				foreach (Apparel item in wornApparel)
				{
					if (!item.AllowVerbCast(this) && !(item.TryGetComp<CompShield>() != null && flag))
					{
						report = "CE_BlockedShield".Translate() + item.LabelShort;
						return false;
					}
				}
			}
		}
		if (!TryFindCEShootLineFromTo(root, targ, out var _))
		{
			float num = (root - targ.Cell).LengthHorizontalSquared;
			if (num > EffectiveRange * EffectiveRange)
			{
				report = "CE_BlockedMaxRange".Translate();
			}
			else if (num < verbProps.minRange * verbProps.minRange)
			{
				report = "CE_BlockedMinRange".Translate();
			}
			else
			{
				report = "CE_NoLoS".Translate();
			}
			return false;
		}
		return true;
	}

	protected bool Retarget()
	{
		if (!doRetarget)
		{
			return true;
		}
		doRetarget = false;
		if (currentTarget != lastTarget)
		{
			lastTarget = currentTarget;
			lastTargetPos = currentTarget.Cell;
			shootingAtDowned = currentTarget.Pawn?.Downed ?? true;
			return true;
		}
		if (shootingAtDowned)
		{
			return true;
		}
		if (currentTarget.Pawn == null || currentTarget.Pawn.Downed || !CanHitFromCellIgnoringRange(Caster.Position, currentTarget, out IntVec3 _))
		{
			Pawn pawn = null;
			Thing thing = Caster;
			foreach (Pawn item in Caster.Position.PawnsNearSegment(lastTargetPos, thing.Map, 3f).OfType<Pawn>())
			{
				if (item.Faction == currentTarget.Pawn?.Faction && item.Faction.HostileTo(thing.Faction) && !item.Downed && CanHitFromCellIgnoringRange(Caster.Position, (LocalTargetInfo)item, out IntVec3 _))
				{
					pawn = item;
					break;
				}
			}
			if (pawn != null)
			{
				currentTarget = new LocalTargetInfo(pawn);
				lastTarget = currentTarget;
				lastTargetPos = currentTarget.Cell;
				shootingAtDowned = false;
				if (thing is Building_TurretGunCE building_TurretGunCE)
				{
					float curRotation = (currentTarget.Cell.ToVector3Shifted() - building_TurretGunCE.DrawPos).AngleFlat();
					building_TurretGunCE.top.CurRotation = curRotation;
				}
				return true;
			}
			shootingAtDowned = true;
			return false;
		}
		return true;
	}

	public virtual void RecalculateWarmupTicks()
	{
	}

	public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
	{
		bool flag = base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
		if (flag && repeating && verbProps.warmupTime > 0f)
		{
			RecalculateWarmupTicks();
		}
		return flag;
	}

	public override bool TryCastShot()
	{
		Retarget();
		repeating = true;
		doRetarget = true;
		storedShotReduction = null;
		VerbPropertiesCE verbPropsCE = VerbPropsCE;
		bool flag = numShotsFired > 0;
		CompFireModes obj = CompFireModes;
		bool flag2 = obj != null && obj.CurrentAimMode == AimMode.SuppressFire;
		if (TryFindCEShootLineFromTo(caster.Position, currentTarget, out var resultingLine))
		{
			lastShootLine = resultingLine;
		}
		else if (flag)
		{
			if (verbPropsCE.interruptibleBurst && !flag2)
			{
				return false;
			}
			if (!lastShootLine.HasValue)
			{
				return false;
			}
			resultingLine = lastShootLine.Value;
			currentTarget = new LocalTargetInfo(lastTargetPos);
		}
		else
		{
			if (!flag2)
			{
				return false;
			}
			if (!currentTarget.IsValid || currentTarget.ThingDestroyed)
			{
				return false;
			}
			resultingLine = new ShootLine(caster.Position, currentTarget.Cell);
			lastShootLine = resultingLine;
		}
		if (projectilePropsCE.pelletCount < 1)
		{
			Log.Error(base.EquipmentSource.LabelCap + " tried firing with pelletCount less than 1.");
			return false;
		}
		bool flag3 = false;
		float spreadDegrees = 0f;
		float aperatureSize = 0f;
		int ticksToTruePosition = VerbPropsCE.ticksToTruePosition;
		if (Projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			flag3 = projectilePropertiesCE.isInstant;
			spreadDegrees = (base.EquipmentSource?.GetStatValue(CE_StatDefOf.ShotSpread) ?? 0f) * projectilePropertiesCE.spreadMult;
			aperatureSize = 0.03f;
		}
		ShiftVecReport shiftVecReport = ShiftVecReportFor(currentTarget);
		bool calculateMechanicalOnly = false;
		for (int i = 0; i < projectilePropsCE.pelletCount; i++)
		{
			ProjectileCE projectileCE = (ProjectileCE)ThingMaker.MakeThing(Projectile);
			GenSpawn.Spawn(projectileCE, resultingLine.Source, caster.Map);
			ShiftTarget(shiftVecReport, calculateMechanicalOnly, flag3, flag);
			projectileCE.canTargetSelf = false;
			float magnitude = (sourceLoc - currentTarget.Cell.ToIntVec2.ToVector2Shifted()).magnitude;
			projectileCE.minCollisionDistance = GetMinCollisionDistance(magnitude);
			projectileCE.intendedTarget = currentTarget;
			projectileCE.mount = caster.Position.GetThingList(caster.Map).FirstOrDefault((Thing t) => t is Pawn && t != caster);
			projectileCE.AccuracyFactor = shiftVecReport.accuracyFactor * shiftVecReport.swayDegrees * ((float)(numShotsFired + 1) * 0.75f);
			if (flag3)
			{
				float shotHeight = ShotHeight;
				float num = AdjustShotHeight(caster, currentTarget, ref shotHeight);
				projectileCE.RayCast(Shooter, verbProps, sourceLoc, shotAngle + num, shotRotation, shotHeight, ShotSpeed, spreadDegrees, aperatureSize, base.EquipmentSource);
			}
			else
			{
				projectileCE.Launch(Shooter, sourceLoc, shotAngle, shotRotation, ShotHeight, ShotSpeed, base.EquipmentSource, distance);
			}
			calculateMechanicalOnly = true;
		}
		LightingTracker.Notify_ShotsFiredAt(caster.Position, VerbPropsCE.muzzleFlashScale);
		calculateMechanicalOnly = false;
		numShotsFired++;
		if (ShooterPawn != null && CompReloadable != null)
		{
			CompReloadable.UsedOnce();
		}
		lastShotTick = Find.TickManager.TicksGame;
		return true;
	}

	public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
	{
		needLOSToCenter = true;
		ThingDef projectile = Projectile;
		if (projectile == null)
		{
			return 0f;
		}
		float num = projectile.projectile.explosionRadius + projectile.projectile.explosionRadiusDisplayPadding;
		float forcedMissRadius = verbProps.ForcedMissRadius;
		if (forcedMissRadius > 0f && verbProps.burstShotCount > 1)
		{
			num += forcedMissRadius;
		}
		return num;
	}

	protected float GetMinCollisionDistance(float targetDistance)
	{
		float num = 1.5f;
		float num2 = 0.2f;
		if (targetDistance <= num / num2)
		{
			return Mathf.Min(num, targetDistance * 0.75f);
		}
		return targetDistance * num2;
	}

	public virtual void VerbTickCE()
	{
	}

	public virtual bool TryFindCEShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
	{
		if (targ.HasThing && targ.Thing.Map != caster.Map)
		{
			resultingLine = default(ShootLine);
			return false;
		}
		if (EffectiveRange <= 1.42f)
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return ReachabilityImmediate.CanReachImmediate(root, targ, caster.Map, PathEndMode.Touch, null);
		}
		CellRect cellRect = ((!targ.HasThing) ? CellRect.SingleCell(targ.Cell) : targ.Thing.OccupiedRect());
		float num = cellRect.ClosestDistSquaredTo(root);
		if (num > EffectiveRange * EffectiveRange || num < verbProps.minRange * verbProps.minRange)
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return false;
		}
		if (Projectile.projectile.flyOverhead)
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return true;
		}
		Vector3 vector = root.ToVector3Shifted();
		vector.y = ShotHeight;
		BuildingProperties building = caster.def.building;
		if (building != null && building.IsTurret)
		{
			vector = ShotSource;
		}
		if (CanHitFromCellIgnoringRange(vector, targ, out var goodDest))
		{
			resultingLine = new ShootLine(root, goodDest);
			return true;
		}
		if (CasterIsPawn)
		{
			ShootLeanUtility.LeanShootingSourcesFromTo(root, cellRect.ClosestCellTo(root), caster.Map, tempLeanShootSources);
			foreach (IntVec3 item in tempLeanShootSources.OrderBy((IntVec3 c) => c.DistanceTo(targ.Cell)))
			{
				float num2 = 0.499f;
				Vector3 vector2 = (item - root).ToVector3() * num2;
				if (CanHitFromCellIgnoringRange(vector + vector2, targ, out goodDest))
				{
					resultingLine = new ShootLine(item, goodDest);
					return true;
				}
			}
		}
		resultingLine = new ShootLine(root, targ.Cell);
		return false;
	}

	private bool CanHitFromCellIgnoringRange(Vector3 shotSource, LocalTargetInfo targ, out IntVec3 goodDest)
	{
		if (targ.Thing != null && targ.Thing.Map != caster.Map)
		{
			goodDest = IntVec3.Invalid;
			return false;
		}
		if (CanHitCellFromCellIgnoringRange(shotSource, targ.Cell, targ.Thing))
		{
			goodDest = targ.Cell;
			return true;
		}
		goodDest = IntVec3.Invalid;
		return false;
	}

	protected virtual bool CanHitCellFromCellIgnoringRange(Vector3 shotSource, IntVec3 targetLoc, Thing targetThing = null)
	{
		if (verbProps.mustCastOnOpenGround && (!targetLoc.Standable(caster.Map) || caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
		{
			return false;
		}
		if (verbProps.requireLineOfSight)
		{
			Vector3 vector;
			if (targetThing != null)
			{
				float shotHeight = shotSource.y;
				AdjustShotHeight(caster, targetThing, ref shotHeight);
				shotSource.y = shotHeight;
				Vector3 drawPos = targetThing.DrawPos;
				vector = new Vector3(drawPos.x, new CollisionVertical(targetThing).Max, drawPos.z);
				if (targetThing is Pawn pawn)
				{
					vector += pawn.Drawer.leaner.LeanOffset * 0.6f;
				}
			}
			else
			{
				vector = targetLoc.ToVector3Shifted();
			}
			Ray shotLine = new Ray(shotSource, vector - shotSource);
			AimMode? aimMode = CompFireModes?.CurrentAimMode;
			Predicate<IntVec3> predicate = delegate(IntVec3 cell)
			{
				Thing thing = cell.GetFirstPawn(caster.Map) ?? cell.GetCover(caster.Map);
				if (thing != null && thing != ShooterPawn && thing != caster && thing != targetThing && !thing.IsPlant() && (!(thing is Pawn) || !thing.HostileTo(caster)))
				{
					if (thing is Pawn { Downed: false, Faction: not null } pawn2 && ShooterPawn?.Faction != null && (ShooterPawn.Faction == pawn2.Faction || ShooterPawn.Faction.RelationKindWith(pawn2.Faction) == FactionRelationKind.Ally) && !pawn2.AdjacentTo8WayOrInside(ShooterPawn))
					{
						return false;
					}
					if ((VerbPropsCE.ignorePartialLoSBlocker || aimMode == AimMode.SuppressFire) && thing.def.Fillage != FillCategory.Full)
					{
						return true;
					}
					Bounds boundsFor = CE_Utility.GetBoundsFor(thing);
					if (thing.def.Fillage != FillCategory.Full && thing.AdjacentTo8WayOrInside(caster))
					{
						float num = cell.DistanceTo(targetLoc);
						float num2 = shotSource.ToIntVec3().DistanceTo(targetLoc);
						if (num2 > num)
						{
							return thing is Pawn || boundsFor.size.y < shotSource.y;
						}
					}
					if (boundsFor.IntersectRay(shotLine))
					{
						if (Controller.settings.DebugDrawPartialLoSChecks)
						{
							caster.Map.debugDrawer.FlashCell(cell, 0f, boundsFor.extents.y.ToString());
						}
						return false;
					}
					if (Controller.settings.DebugDrawPartialLoSChecks)
					{
						caster.Map.debugDrawer.FlashCell(cell, 0.7f, boundsFor.extents.y.ToString());
					}
				}
				return true;
			};
			foreach (IntVec3 item in GenSightCE.PointsOnLineOfSight(shotSource, targetLoc.ToVector3Shifted()))
			{
				if (Controller.settings.DebugDrawPartialLoSChecks)
				{
					caster.Map.debugDrawer.FlashCell(item, 0.4f);
				}
				if (item != shotSource.ToIntVec3() && item != targetLoc && !predicate(item))
				{
					return false;
				}
			}
		}
		return true;
	}
}
