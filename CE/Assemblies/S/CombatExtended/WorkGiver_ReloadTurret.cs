using System.Collections.Generic;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended.CombatExtended.LoggerUtils;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class WorkGiver_ReloadTurret : WorkGiver_Scanner
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.GetComponent<TurretTracker>().Turrets;
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		return GetThingPriority(pawn, t.Thing);
	}

	private float GetThingPriority(Pawn pawn, Thing t, bool forced = false)
	{
		CELogger.Message($"pawn: {pawn}. t: {t}. forced: {forced}", showOutOfDebugMode: false, "GetThingPriority");
		Building_Turret building_Turret = t as Building_Turret;
		Building_TurretGunCE obj = building_Turret as Building_TurretGunCE;
		if (obj != null && !obj.Active)
		{
			return 1f;
		}
		CompAmmoUser ammo = building_Turret.GetAmmo();
		if (ammo != null && ammo.EmptyMagazine)
		{
			return 9f;
		}
		if (building_Turret.GetMannable() == null)
		{
			return 5f;
		}
		return 1f;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (forced)
		{
			CELogger.Message("Job is forced. Not skipping.", showOutOfDebugMode: false, "ShouldSkip");
			return false;
		}
		if (pawn.CurJob == null)
		{
			CELogger.Message("Pawn " + pawn.ThingID + " has no job. Not skipping.", showOutOfDebugMode: false, "ShouldSkip");
			return false;
		}
		if (pawn.CurJobDef == JobDefOf.ManTurret)
		{
			CELogger.Message("Pawn " + pawn.ThingID + "'s current job is ManTurret. Not skipping.", showOutOfDebugMode: false, "ShouldSkip");
			return false;
		}
		if (pawn.CurJob.playerForced)
		{
			CELogger.Message("Pawn " + pawn.ThingID + "'s current job is forced by the player. Skipping.", showOutOfDebugMode: false, "ShouldSkip");
			return true;
		}
		CELogger.Message("Pawn " + pawn.ThingID + "'s current job is " + pawn.CurJobDef.reportString + ". Skipping", showOutOfDebugMode: false, "ShouldSkip");
		return pawn.CurJobDef == CE_JobDefOf.ReloadTurret || pawn.CurJobDef == CE_JobDefOf.ReloadWeapon;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_Turret building_Turret))
		{
			return false;
		}
		float thingPriority = GetThingPriority(pawn, t, forced);
		CELogger.Message($"Priority check completed. Got {thingPriority}", showOutOfDebugMode: false, "HasJobOnThing");
		CompAmmoUser ammo = building_Turret.GetAmmo();
		CELogger.Message($"Turret uses ammo? {ammo?.UseAmmo}", showOutOfDebugMode: false, "HasJobOnThing");
		if (!building_Turret.GetReloadable())
		{
			return false;
		}
		CELogger.Message($"Total magazine size: {ammo.MagSize}. Needed: {ammo.MissingToFullMagazine}", showOutOfDebugMode: false, "HasJobOnThing");
		return JobGiverUtils_Reload.CanReload(pawn, building_Turret, forced);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Building_Turret building_Turret = t as Building_Turret;
		if (building_Turret == null)
		{
			CELogger.Error($"{pawn} tried to make a reload job on a {t} which isn't a turret. This should never be reached.", showOutOfDebugMode: true, "JobOnThing");
		}
		return JobGiverUtils_Reload.MakeReloadJob(pawn, building_Turret);
	}
}
