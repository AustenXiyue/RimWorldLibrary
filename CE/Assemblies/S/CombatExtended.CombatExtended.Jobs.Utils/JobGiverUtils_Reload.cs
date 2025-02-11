using System;
using CombatExtended.CombatExtended.LoggerUtils;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.CombatExtended.Jobs.Utils;

internal class JobGiverUtils_Reload
{
	private const float MaxPathCost = 300000f;

	private const int MagicMaxPawns = 10;

	private const float MaxAmmoSearchRadiusForNonPlayerPawns = 40f;

	public static Job MakeReloadJob(Pawn pawn, Building_Turret turret)
	{
		CompAmmoUser ammo = turret.GetAmmo();
		if (ammo == null)
		{
			CELogger.Error($"{pawn} tried to create a reload job on a thing ({turret}) that's not reloadable.", showOutOfDebugMode: true, "MakeReloadJob");
			return null;
		}
		if (!ammo.UseAmmo)
		{
			return MakeReloadJobNoAmmo(turret);
		}
		Thing thing = FindBestAmmo(pawn, turret);
		if (thing == null)
		{
			CELogger.Error($"{pawn} tried to create a reload job without ammo. This should have been checked earlier.", showOutOfDebugMode: true, "MakeReloadJob");
			return null;
		}
		CELogger.Message($"Making a reload job for {pawn}, {turret} and {thing}", showOutOfDebugMode: false, "MakeReloadJob");
		Job job = JobMaker.MakeJob(CE_JobDefOf.ReloadTurret, turret, thing);
		job.count = Mathf.Min(thing.stackCount, ammo.MissingToFullMagazine);
		return job;
	}

	public static Job MakeReloadJob(Pawn pawn, Building_AutoloaderCE AutoLoader)
	{
		CompAmmoUser compAmmoUser = AutoLoader.CompAmmoUser;
		if (compAmmoUser == null)
		{
			CELogger.Error($"{pawn} tried to create a reload job on a thing ({AutoLoader}) that's not reloadable.", showOutOfDebugMode: true, "MakeReloadJob");
			return null;
		}
		if (!compAmmoUser.UseAmmo)
		{
			CELogger.Error($"{pawn} tried to create a reload job on a thing ({AutoLoader}) that's doesn't need ammo.", showOutOfDebugMode: true, "MakeReloadJob");
			return null;
		}
		Thing thing = FindBestAmmo(pawn, AutoLoader);
		if (thing == null)
		{
			CELogger.Error($"{pawn} tried to create a reload job without ammo. This should have been checked earlier.", showOutOfDebugMode: true, "MakeReloadJob");
			return null;
		}
		CELogger.Message($"Making a reload job for {pawn}, {AutoLoader} and {thing}", showOutOfDebugMode: false, "MakeReloadJob");
		Job job = JobMaker.MakeJob(CE_JobDefOf.ReloadAutoLoader, AutoLoader, thing);
		job.count = Mathf.Min(thing.stackCount, compAmmoUser.MissingToFullMagazine);
		return job;
	}

	private static Job MakeReloadJobNoAmmo(Building_Turret turret)
	{
		CompAmmoUser ammo = turret.GetAmmo();
		if (ammo == null)
		{
			CELogger.Error("Tried to create a reload job on a thing that's not reloadable.", showOutOfDebugMode: true, "MakeReloadJobNoAmmo");
			return null;
		}
		return JobMaker.MakeJob(CE_JobDefOf.ReloadTurret, turret, null);
	}

	public static bool CanReload(Pawn pawn, Thing thing, bool forced = false, bool emergency = false)
	{
		if (pawn == null || thing == null)
		{
			CELogger.Warn((pawn?.ToString() ?? "null pawn") + " could not reload " + (thing?.ToString() ?? "null thing") + " one of the two was null.", showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (!(thing is Building_Turret building_Turret))
		{
			CELogger.Warn(string.Format("{0} could not reload {1} because {2} is not a Turret. If you are a modder, make sure to use {3}.{4} for your turret's compClass.", pawn, thing, thing, "CombatExtended", "Building_TurretGunCE"), showOutOfDebugMode: false, "CanReload");
			return false;
		}
		CompAmmoUser ammo = building_Turret.GetAmmo();
		if (ammo == null)
		{
			CELogger.Warn(string.Format("{0} could not reload {1} because turret has no {2}.", pawn, building_Turret, "CompAmmoUser"), showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (building_Turret.GetReloading())
		{
			CELogger.Message($"{pawn} could not reload {building_Turret} because turret is already reloading.", showOutOfDebugMode: false, "CanReload");
			JobFailReason.Is("CE_TurretAlreadyReloading".Translate());
			return false;
		}
		if (building_Turret.IsBurning() && !emergency)
		{
			CELogger.Message($"{pawn} could not reload {building_Turret} because turret is on fire.", showOutOfDebugMode: false, "CanReload");
			JobFailReason.Is("CE_TurretIsBurning".Translate());
			return false;
		}
		if (ammo.FullMagazine)
		{
			CELogger.Message($"{pawn} could not reload {building_Turret} because it is full of ammo.", showOutOfDebugMode: false, "CanReload");
			JobFailReason.Is("CE_TurretFull".Translate());
			return false;
		}
		if (building_Turret.IsForbidden(pawn) || !pawn.CanReserve(building_Turret, 1, -1, null, forced))
		{
			CELogger.Message($"{pawn} could not reload {building_Turret} because it is forbidden or otherwise busy.", showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (building_Turret.Faction != pawn.Faction && pawn.Faction != null)
		{
			Faction faction = building_Turret.Faction;
			if (faction == null || faction.RelationKindWith(pawn.Faction) != FactionRelationKind.Ally)
			{
				CELogger.Message($"{pawn} could not reload {building_Turret} because the turret is unclaimed or hostile to them.", showOutOfDebugMode: false, "CanReload");
				JobFailReason.Is("CE_TurretNonAllied".Translate());
				return false;
			}
		}
		if (building_Turret.GetMannable()?.ManningPawn != pawn && !pawn.CanReserveAndReach(building_Turret, PathEndMode.ClosestTouch, forced ? Danger.Deadly : pawn.NormalMaxDanger(), 10))
		{
			CELogger.Message($"{pawn} could not reload {building_Turret} because turret is manned (or was recently manned) by someone else.", showOutOfDebugMode: false, "CanReload");
			return false;
		}
		if (ammo.UseAmmo && FindBestAmmo(pawn, building_Turret) == null)
		{
			JobFailReason.Is("CE_NoAmmoAvailable".Translate());
			return false;
		}
		return true;
	}

	public static Thing FindBestAmmo(Pawn pawn, Building_AutoloaderCE AutoLoader)
	{
		CompAmmoUser compAmmoUser = AutoLoader.CompAmmoUser;
		AmmoDef selectedAmmo = compAmmoUser.SelectedAmmo;
		Thing thing = FindBestAmmo(pawn, selectedAmmo);
		if (thing == null && compAmmoUser.EmptyMagazine && selectedAmmo.AmmoSetDefs != null && AutoLoader.Faction != Faction.OfPlayer)
		{
			foreach (AmmoSetDef ammoSetDef in selectedAmmo.AmmoSetDefs)
			{
				foreach (AmmoLink ammoType in ammoSetDef.ammoTypes)
				{
					thing = FindBestAmmo(pawn, ammoType.ammo);
					if (thing != null)
					{
						return thing;
					}
				}
			}
		}
		return thing;
	}

	private static Thing FindBestAmmo(Pawn pawn, Building_Turret turret)
	{
		CompAmmoUser ammo = turret.GetAmmo();
		AmmoDef selectedAmmo = ammo.SelectedAmmo;
		Thing thing = FindBestAmmo(pawn, selectedAmmo);
		if (thing == null && ammo.EmptyMagazine && selectedAmmo.AmmoSetDefs != null && turret.Faction != Faction.OfPlayer)
		{
			foreach (AmmoSetDef ammoSetDef in selectedAmmo.AmmoSetDefs)
			{
				foreach (AmmoLink ammoType in ammoSetDef.ammoTypes)
				{
					thing = FindBestAmmo(pawn, ammoType.ammo);
					if (thing != null)
					{
						return thing;
					}
				}
			}
		}
		return thing;
	}

	private static Thing FindBestAmmo(Pawn pawn, AmmoDef requestedAmmo)
	{
		Predicate<Thing> validator = delegate(Thing potentialAmmo)
		{
			if (potentialAmmo is AmmoThing { IsCookingOff: not false })
			{
				return false;
			}
			if (potentialAmmo.IsBurning())
			{
				return false;
			}
			return !potentialAmmo.IsForbidden(pawn) && pawn.CanReserve(potentialAmmo) && GetPathCost(pawn, potentialAmmo) <= 300000f;
		};
		float maxDistance = ((pawn.Faction != Faction.OfPlayer) ? 40f : 9999f);
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(requestedAmmo), PathEndMode.ClosestTouch, TraverseParms.For(pawn), maxDistance, validator);
	}

	private static float GetPathCost(Pawn pawn, Thing thing)
	{
		IntVec3 position = thing.Position;
		IntVec3 position2 = pawn.Position;
		TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors);
		using PawnPath pawnPath = pawn.Map.pathFinder.FindPath(position2, position, traverseParms, PathEndMode.Touch);
		return pawnPath.TotalCost;
	}
}
