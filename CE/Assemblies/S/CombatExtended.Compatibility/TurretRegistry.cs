using System;
using System.Collections.Generic;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended.CombatExtended.LoggerUtils;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.Compatibility;

public static class TurretRegistry
{
	private static bool enabled;

	private static Dictionary<Type, Action<Building_Turret, bool>> setReloading;

	private static Dictionary<Type, Func<Building_Turret, bool>> getReloading;

	private static Dictionary<Type, Func<Building_Turret, CompAmmoUser>> getAmmo;

	private static Dictionary<Type, Func<Building_Turret, Thing>> getGun;

	private static void Enable()
	{
		enabled = true;
		setReloading = new Dictionary<Type, Action<Building_Turret, bool>>();
		getReloading = new Dictionary<Type, Func<Building_Turret, bool>>();
		getAmmo = new Dictionary<Type, Func<Building_Turret, CompAmmoUser>>();
		getGun = new Dictionary<Type, Func<Building_Turret, Thing>>();
	}

	public static void RegisterReloadableTurret(Type turretType, Action<Building_Turret, bool> setReload, Func<Building_Turret, bool> getReload, Func<Building_Turret, Thing> gun, Func<Building_Turret, CompAmmoUser> ammo = null)
	{
		if (!enabled)
		{
			Enable();
		}
		getReloading[turretType] = getReload;
		setReloading[turretType] = setReload;
		getAmmo[turretType] = ammo;
		getGun[turretType] = gun;
	}

	public static void SetReloading(this Building_Turret turret, bool reloading)
	{
		Action<Building_Turret, bool> value;
		if (turret is Building_TurretGunCE building_TurretGunCE)
		{
			building_TurretGunCE.isReloading = reloading;
		}
		else if (enabled && setReloading.TryGetValue(turret.GetType(), out value))
		{
			value(turret, reloading);
		}
		else
		{
			CELogger.Warn("Asked to set reloading on an unknown turret type: " + turret, showOutOfDebugMode: false, "SetReloading");
		}
	}

	public static bool GetReloading(this Building_Turret turret)
	{
		if (!(turret is Building_TurretGunCE { isReloading: var isReloading }))
		{
			if (enabled && getReloading.TryGetValue(turret.GetType(), out var value))
			{
				return value(turret);
			}
			CELogger.Warn("Asked to get reloading on an unknown turret type: " + turret, showOutOfDebugMode: false, "GetReloading");
			return false;
		}
		return isReloading;
	}

	public static CompAmmoUser GetAmmo(this Building_Turret turret)
	{
		if (!(turret is Building_TurretGunCE { CompAmmo: var compAmmo }))
		{
			if (enabled)
			{
				if (getAmmo.TryGetValue(turret.GetType(), out var value))
				{
					return value(turret);
				}
				if (getGun.TryGetValue(turret.GetType(), out var value2))
				{
					return value2(turret)?.TryGetComp<CompAmmoUser>();
				}
			}
			CELogger.Warn("Asked to get ammo on an unknown turret type: " + turret, showOutOfDebugMode: false, "GetAmmo");
			return null;
		}
		return compAmmo;
	}

	public static Thing GetGun(this Building_Turret turret)
	{
		if (!(turret is Building_TurretGunCE { Gun: var gun }))
		{
			if (enabled && getGun.TryGetValue(turret.GetType(), out var value))
			{
				return value(turret);
			}
			CELogger.Warn("Asked to get gun on an unknown turret type: " + turret, showOutOfDebugMode: false, "GetGun");
			return null;
		}
		return gun;
	}

	public static CompMannable GetMannable(this Building_Turret turret)
	{
		if (!(turret is Building_TurretGunCE { MannableComp: var mannableComp }))
		{
			return turret.TryGetComp<CompMannable>();
		}
		return mannableComp;
	}

	public static bool GetReloadable(this Building_Turret turret)
	{
		return turret.GetAmmo()?.HasMagazine ?? false;
	}

	public static bool ShouldReload(this Building_Turret turret, float threshold = 0.5f, bool ensureAmmoType = true)
	{
		CompAmmoUser ammo = turret.GetAmmo();
		if (ammo == null)
		{
			return false;
		}
		return (ammo.HasMagazine && (float)ammo.CurMagCount <= (float)ammo.MagSize * threshold) || (ensureAmmoType && ammo.CurrentAmmo != ammo.SelectedAmmo);
	}

	public static void TryForceReload(this Building_Turret turret)
	{
		turret.TryOrderReload(forced: true);
	}

	public static void TryOrderReload(this Building_Turret turret, bool forced = false)
	{
		if (turret is Building_TurretGunCE building_TurretGunCE)
		{
			building_TurretGunCE.TryOrderReload(forced);
			return;
		}
		CompAmmoUser ammo = turret.GetAmmo();
		if (ammo.CurrentAmmo == ammo.SelectedAmmo && (!ammo.HasMagazine || ammo.CurMagCount == ammo.MagSize))
		{
			return;
		}
		CompMannable mannable = turret.GetMannable();
		if (mannable == null || !mannable.MannedNow || (!forced && ammo.CurMagCount != 0))
		{
			return;
		}
		Pawn manningPawn = mannable.ManningPawn;
		if (manningPawn != null && JobGiverUtils_Reload.CanReload(manningPawn, turret))
		{
			Job job = JobGiverUtils_Reload.MakeReloadJob(manningPawn, turret);
			if (job != null)
			{
				manningPawn.jobs.StartJob(job, JobCondition.Ongoing, null, manningPawn.CurJob?.def != CE_JobDefOf.ReloadTurret, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
		}
	}
}
