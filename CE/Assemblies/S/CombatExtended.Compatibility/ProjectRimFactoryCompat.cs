using System;
using ProjectRimFactory.Industry;
using Verse;

namespace CombatExtended.Compatibility;

public class ProjectRimFactoryCompat : IPatch
{
	public bool CanInstall()
	{
		return ModLister.GetActiveModWithIdentifier("spdskatr.projectrimfactory") != null;
	}

	public void Install()
	{
		Building_FuelingMachine.RegisterRefuelable(typeof(Building_TurretGunCE), (Func<Building, object>)FindCompAmmoUser, (Func<object, Thing, int>)TestAmmo, (Action<object, Thing>)ReloadAction);
	}

	private static int TestAmmo(object compObject, Thing ammo)
	{
		CompAmmoUser compAmmoUser = compObject as CompAmmoUser;
		if (ammo.def != compAmmoUser.SelectedAmmo)
		{
			return 0;
		}
		return Math.Min(compAmmoUser.MissingToFullMagazine, ammo.stackCount);
	}

	private static void ReloadAction(object compObject, Thing ammo)
	{
		CompAmmoUser compAmmoUser = compObject as CompAmmoUser;
		if (ammo.def != compAmmoUser.CurrentAmmo)
		{
			compAmmoUser.TryUnload();
		}
		compAmmoUser.LoadAmmo(ammo);
	}

	private static object FindCompAmmoUser(Building building)
	{
		CompAmmoUser compAmmo = (building as Building_TurretGunCE).CompAmmo;
		if (!compAmmo.FullMagazine)
		{
			return compAmmo;
		}
		return null;
	}
}
