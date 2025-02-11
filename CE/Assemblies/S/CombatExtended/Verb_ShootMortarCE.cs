using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Verb_ShootMortarCE : Verb_ShootCE
{
	public LocalTargetInfo mokeTargetInfo = LocalTargetInfo.Invalid;

	public GlobalTargetInfo globalTargetInfo = GlobalTargetInfo.Invalid;

	public GlobalTargetInfo globalSourceInfo = GlobalTargetInfo.Invalid;

	private IntVec3 shiftedGlobalCell = IntVec3.Invalid;

	private bool targetHasMarker = false;

	private int startingTile;

	private int destinationTile;

	private int globalDistance;

	private Vector3 direction;

	private new int numShotsFired;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref startingTile, "startingTile", 0);
		Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
		Scribe_Values.Look(ref globalDistance, "globalDistance", 0);
		Scribe_Values.Look(ref shotRotation, "shotRotation", 0f);
		Scribe_Values.Look(ref shotAngle, "shotAngle", 0f);
		Scribe_Values.Look(ref shiftedGlobalCell, "shiftedGlobalCell");
		Scribe_TargetInfo.Look(ref globalTargetInfo, "globalTargetInfo");
		Scribe_TargetInfo.Look(ref globalSourceInfo, "globalSourceInfo");
		Scribe_TargetInfo.Look(ref mokeTargetInfo, "mokeTargetInfo");
	}

	public bool TryStartShelling(GlobalTargetInfo sourceInfo, GlobalTargetInfo targetInfo)
	{
		globalTargetInfo = targetInfo;
		globalSourceInfo = sourceInfo;
		mokeTargetInfo = GetLocalTargetFor(targetInfo);
		if (!TryStartCastOn(mokeTargetInfo, surpriseAttack: true))
		{
			globalTargetInfo = (globalSourceInfo = GlobalTargetInfo.Invalid);
			mokeTargetInfo = LocalTargetInfo.Invalid;
			return false;
		}
		return true;
	}

	public override ShiftVecReport ShiftVecReportFor(LocalTargetInfo target)
	{
		ShiftVecReport shiftVecReport = base.ShiftVecReportFor(target);
		shiftVecReport.circularMissRadius = GetMissRadiusForDist(shiftVecReport.shotDist);
		ArtilleryMarker artilleryMarker = null;
		if (currentTarget.HasThing && currentTarget.Thing.HasAttachment(ThingDef.Named("ArtilleryMarker")))
		{
			artilleryMarker = (ArtilleryMarker)currentTarget.Thing.GetAttachment(ThingDef.Named("ArtilleryMarker"));
		}
		else if (currentTarget.Cell.InBounds(caster.Map))
		{
			artilleryMarker = (ArtilleryMarker)currentTarget.Cell.GetFirstThing(caster.Map, ThingDef.Named("ArtilleryMarker"));
		}
		if (artilleryMarker != null)
		{
			shiftVecReport.aimingAccuracy = artilleryMarker.aimingAccuracy;
			shiftVecReport.sightsEfficiency = artilleryMarker.sightsEfficiency;
			shiftVecReport.weatherShift = artilleryMarker.weatherShift;
			shiftVecReport.lightingShift = artilleryMarker.lightingShift;
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_Spotting, KnowledgeAmount.SpecificInteraction);
		}
		else if (shiftVecReport.shotDist > 75f || !GenSight.LineOfSight(caster.Position, shiftVecReport.target.Cell, caster.Map, skipFirstCell: true))
		{
			shiftVecReport.indirectFireShift = base.VerbPropsCE.indirectFirePenalty * shiftVecReport.shotDist;
			shiftVecReport.weatherShift = 0f;
			shiftVecReport.lightingShift = 0f;
		}
		return shiftVecReport;
	}

	public override ShiftVecReport ShiftVecReportFor(GlobalTargetInfo target)
	{
		if (!target.IsMapTarget || globalTargetInfo.Map == null)
		{
			return null;
		}
		ShiftVecReport shiftVecReport = base.ShiftVecReportFor(target);
		shiftVecReport.circularMissRadius = GetGlobalMissRadiusForDist(shiftVecReport.shotDist);
		shiftVecReport.weatherShift = (1f - globalTargetInfo.Map.weatherManager.CurWeatherAccuracyMultiplier) * 1.5f + (1f - globalSourceInfo.Map.weatherManager.CurWeatherAccuracyMultiplier) * 0.5f;
		ArtilleryMarker artilleryMarker = null;
		if (target.HasThing && target.Thing.HasAttachment(ThingDef.Named("ArtilleryMarker")))
		{
			artilleryMarker = (ArtilleryMarker)target.Thing.GetAttachment(ThingDef.Named("ArtilleryMarker"));
		}
		else if (target.Cell.InBounds(caster.Map))
		{
			artilleryMarker = (ArtilleryMarker)target.Cell.GetFirstThing(target.Map, ThingDef.Named("ArtilleryMarker"));
		}
		if (artilleryMarker != null)
		{
			targetHasMarker = true;
			shiftVecReport.circularMissRadius *= 0.5f;
			shiftVecReport.smokeDensity *= 0.5f;
			shiftVecReport.weatherShift *= 0.25f;
			shiftVecReport.lightingShift *= 0.25f;
			shiftVecReport.aimingAccuracy = artilleryMarker.aimingAccuracy;
			shiftVecReport.sightsEfficiency = artilleryMarker.sightsEfficiency;
		}
		else
		{
			targetHasMarker = false;
		}
		return shiftVecReport;
	}

	protected virtual LocalTargetInfo GetLocalTargetFor(GlobalTargetInfo targetInfo)
	{
		startingTile = caster.Map.Tile;
		destinationTile = targetInfo.Tile;
		direction = (Find.WorldGrid.GetTileCenter(startingTile) - Find.WorldGrid.GetTileCenter(destinationTile)).normalized;
		globalDistance = (int)CE_Utility.DistanceBetweenTiles(targetInfo.Tile, caster.Map.Tile);
		Ray ray = new Ray(caster.DrawPos, direction);
		IntVec3 cell = ray.ExitCell(caster.Map);
		return new LocalTargetInfo(cell);
	}

	public virtual bool TryCastGlobalShot()
	{
		ShootLine shootLine = new ShootLine(caster.positionInt, mokeTargetInfo.Cell);
		if (base.projectilePropsCE.pelletCount < 1)
		{
			Log.Error(base.EquipmentSource.LabelCap + " tried firing with pelletCount less than 1.");
			return false;
		}
		bool isInstant = false;
		if (Projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			isInstant = projectilePropertiesCE.isInstant;
		}
		ShiftVecReport report = ShiftVecReportFor(globalTargetInfo);
		ShiftVecReport shiftVecReport = ShiftVecReportFor(currentTarget);
		shiftedGlobalCell = globalTargetInfo.Cell;
		bool calculateMechanicalOnly = false;
		for (int i = 0; i < base.projectilePropsCE.pelletCount; i++)
		{
			ProjectileCE projectileCE = (ProjectileCE)ThingMaker.MakeThing(Projectile);
			GenSpawn.Spawn(projectileCE, shootLine.Source, caster.Map);
			ShiftGlobalTarget(report);
			ShiftTarget(shiftVecReport, calculateMechanicalOnly, isInstant);
			float num = base.ShotSpeed * 5f;
			projectileCE.globalTargetInfo = default(GlobalTargetInfo);
			projectileCE.globalTargetInfo.cellInt = shiftedGlobalCell;
			projectileCE.globalTargetInfo.mapInt = globalTargetInfo.Map;
			projectileCE.globalTargetInfo.tileInt = globalTargetInfo.Tile;
			projectileCE.canTargetSelf = false;
			Thing thing = globalTargetInfo.Thing;
			projectileCE.intendedTarget = ((thing != null) ? ((LocalTargetInfo)thing) : currentTarget);
			projectileCE.globalSourceInfo = globalSourceInfo;
			projectileCE.mount = caster.Position.GetThingList(caster.Map).FirstOrDefault((Thing t) => t is Pawn && t != caster);
			projectileCE.AccuracyFactor = shiftVecReport.accuracyFactor * shiftVecReport.swayDegrees * ((float)(numShotsFired + 1) * 0.75f);
			projectileCE.Launch(base.Shooter, sourceLoc, shotAngle, shotRotation, ShotHeight, num, base.EquipmentSource);
			calculateMechanicalOnly = true;
		}
		base.LightingTracker.Notify_ShotsFiredAt(caster.Position, base.VerbPropsCE.muzzleFlashScale);
		calculateMechanicalOnly = false;
		numShotsFired++;
		if (base.ShooterPawn != null && base.CompReloadable != null)
		{
			base.CompReloadable.UsedOnce();
		}
		lastShotTick = Find.TickManager.TicksGame;
		return true;
	}

	public override bool TryCastShot()
	{
		if (!globalTargetInfo.IsValid)
		{
			return base.TryCastShot();
		}
		if (TryCastGlobalShot())
		{
			return OnCastSuccessful();
		}
		return false;
	}

	protected virtual float GetMissRadiusForDist(float targDist)
	{
		float num = verbProps.range;
		if (base.CompCharges != null && base.CompCharges.GetChargeBracket(targDist, ShotHeight, base.projectilePropsCE.Gravity, out var bracket))
		{
			num = bracket.y;
		}
		float num2 = targDist / num;
		float num3 = ((num2 <= 0.5f) ? (1f - num2) : (0.5f + (num2 - 0.5f) / 2f));
		return base.VerbPropsCE.circularError * num3;
	}

	protected virtual float GetGlobalMissRadiusForDist(float targDist)
	{
		float num = base.projectilePropsCE.shellingProps.range * 5f;
		float num2 = targDist / num;
		float num3 = ((num2 <= 0.5f) ? (1f - num2) : (0.5f + (num2 - 0.5f) / 2f));
		return base.VerbPropsCE.circularError * num3;
	}

	private void ShiftGlobalTarget(ShiftVecReport report)
	{
		if (report != null && shiftedGlobalCell.IsValid)
		{
			float num = ((!targetHasMarker) ? 0.444f : 0.111f);
			report.shotDist = Mathf.Max(report.shotDist, report.maxRange * num);
			float shotDist = report.shotDist;
			Vector2 origin = new Vector2(shiftedGlobalCell.x, shiftedGlobalCell.z);
			Vector3 normalized = (Find.WorldGrid.GetTileCenter(startingTile) - Find.WorldGrid.GetTileCenter(destinationTile)).normalized;
			float randDist = report.GetRandDist();
			Vector2 vector = UnityEngine.Random.insideUnitCircle * Mathf.Clamp(report.spreadDegrees * (float)Math.PI / 360f, -1f, 1f) * (randDist - report.shotDist);
			Ray2D ray2D = new Ray2D(origin, -1f * normalized);
			Vector2 randCircularVec = report.GetRandCircularVec();
			Vector2 vector2 = ray2D.GetPoint(randDist - shotDist) + randCircularVec + vector;
			shiftedGlobalCell = new IntVec3((int)vector2.x, 0, (int)vector2.y);
		}
	}
}
