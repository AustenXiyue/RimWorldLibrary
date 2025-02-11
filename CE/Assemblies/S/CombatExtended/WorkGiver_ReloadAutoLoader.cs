using System.Collections.Generic;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended.CombatExtended.LoggerUtils;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class WorkGiver_ReloadAutoLoader : WorkGiver_Scanner
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.GetComponent<AutoLoaderTracker>().AutoLoaders;
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		return GetThingPriority(pawn, t.Thing);
	}

	private float GetThingPriority(Pawn pawn, Thing t, bool forced = false)
	{
		Building_AutoloaderCE building_AutoloaderCE = t as Building_AutoloaderCE;
		if (!building_AutoloaderCE.CompAmmoUser.EmptyMagazine || building_AutoloaderCE.isActive)
		{
			return 9f;
		}
		return 1f;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (forced)
		{
			return false;
		}
		if (pawn.CurJob == null)
		{
			return false;
		}
		if (pawn.CurJobDef == JobDefOf.ManTurret)
		{
			return false;
		}
		if (pawn.CurJob.playerForced)
		{
			return true;
		}
		return pawn.CurJobDef == CE_JobDefOf.ReloadTurret || pawn.CurJobDef == CE_JobDefOf.ReloadWeapon;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Building_AutoloaderCE building_AutoloaderCE = t as Building_AutoloaderCE;
		float thingPriority = GetThingPriority(pawn, t, forced);
		CompAmmoUser compAmmoUser = building_AutoloaderCE.CompAmmoUser;
		if (building_AutoloaderCE.isReloading || building_AutoloaderCE.isActive)
		{
			JobFailReason.Is("CE_AutoLoaderBusy".Translate());
			return false;
		}
		return CanReload(pawn, building_AutoloaderCE, forced);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Building_AutoloaderCE autoLoader = t as Building_AutoloaderCE;
		return JobGiverUtils_Reload.MakeReloadJob(pawn, autoLoader);
	}

	public static bool CanReload(Pawn pawn, Thing thing, bool forced = false, bool emergency = false)
	{
		if (pawn == null || thing == null)
		{
			CELogger.Warn((pawn?.ToString() ?? "null pawn") + " could not reload " + (thing?.ToString() ?? "null thing") + " one of the two was null.", showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (!(thing is Building_AutoloaderCE { CompAmmoUser: var compAmmoUser } building_AutoloaderCE))
		{
			CELogger.Warn(string.Format("{0} could not reload {1} because {2} is not a Building_AutoLoaderCE. If you are a modder, make sure to use {3}.{4} for your turret's compClass.", pawn, thing, thing, "CombatExtended", "Building_TurretGunCE"), showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (compAmmoUser == null)
		{
			CELogger.Warn(string.Format("{0} could not reload {1} because Building_AutoLoaderCE has no {2}.", pawn, building_AutoloaderCE, "CompAmmoUser"), showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (building_AutoloaderCE.IsBurning() && !emergency)
		{
			CELogger.Message($"{pawn} could not reload {building_AutoloaderCE} because Building_AutoLoaderCE is on fire.", showOutOfDebugMode: false, "CanReload");
			JobFailReason.Is("CE_TurretIsBurning".Translate());
			return false;
		}
		if (compAmmoUser.FullMagazine)
		{
			CELogger.Message($"{pawn} could not reload {building_AutoloaderCE} because it is full of ammo.", showOutOfDebugMode: false, "CanReload");
			JobFailReason.Is("CE_TurretFull".Translate());
			return false;
		}
		if (building_AutoloaderCE.IsForbidden(pawn) || !pawn.CanReserve(building_AutoloaderCE, 1, -1, null, forced))
		{
			CELogger.Message($"{pawn} could not reload {building_AutoloaderCE} because it is forbidden or otherwise busy.", showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (building_AutoloaderCE.Faction != pawn.Faction && pawn.Faction != null)
		{
			Faction faction = building_AutoloaderCE.Faction;
			if (faction == null || faction.RelationKindWith(pawn.Faction) != FactionRelationKind.Ally)
			{
				CELogger.Message($"{pawn} could not reload {building_AutoloaderCE} because the Building_AutoLoaderCE is unclaimed or hostile to them.", showOutOfDebugMode: false, "CanReload");
				JobFailReason.Is("CE_TurretNonAllied".Translate());
				return false;
			}
		}
		if (compAmmoUser.UseAmmo && JobGiverUtils_Reload.FindBestAmmo(pawn, building_AutoloaderCE) == null)
		{
			JobFailReason.Is("CE_NoAmmoAvailable".Translate());
			return false;
		}
		return true;
	}
}
