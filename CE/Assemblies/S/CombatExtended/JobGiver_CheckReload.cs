using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_CheckReload : ThinkNode_JobGiver
{
	private const float reloadPriority = 9.1f;

	public override float GetPriority(Pawn pawn)
	{
		if (!pawn.Drafted && DoReloadCheck(pawn, out var _, out var _))
		{
			return 9.1f;
		}
		return 0f;
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		Job result = null;
		if (DoReloadCheck(pawn, out var reloadWeapon, out var reloadAmmo))
		{
			CompAmmoUser compAmmoUser = reloadWeapon.TryGetComp<CompAmmoUser>();
			if (!compAmmoUser.TryUnload())
			{
				return null;
			}
			if (compAmmoUser.UseAmmo && compAmmoUser.CurrentAmmo != reloadAmmo)
			{
				compAmmoUser.SelectedAmmo = reloadAmmo;
			}
			result = compAmmoUser.TryMakeReloadJob();
		}
		return result;
	}

	private bool DoReloadCheck(Pawn pawn, out ThingWithComps reloadWeapon, out AmmoDef reloadAmmo)
	{
		reloadWeapon = null;
		reloadAmmo = null;
		List<ThingWithComps> list = new List<ThingWithComps>();
		CompInventory inventory = pawn.TryGetComp<CompInventory>();
		Loadout loadout = pawn.GetLoadout();
		bool flag = loadout != null && !loadout.Slots.NullOrEmpty();
		if (inventory == null)
		{
			return false;
		}
		CompAmmoUser compAmmoUser;
		if ((compAmmoUser = pawn.equipment?.Primary?.TryGetComp<CompAmmoUser>()) != null && compAmmoUser.HasMagazine)
		{
			list.Add(pawn.equipment.Primary);
		}
		list.AddRange(inventory.rangedWeaponList.Where((ThingWithComps t) => t.TryGetComp<CompAmmoUser>() != null && t.GetComp<CompAmmoUser>().HasMagazine));
		if (list.NullOrEmpty())
		{
			return false;
		}
		foreach (ThingWithComps item in list)
		{
			compAmmoUser = item.TryGetComp<CompAmmoUser>();
			AmmoDef ammoType = compAmmoUser.CurrentAmmo;
			int curMagCount = compAmmoUser.CurMagCount;
			int magazineSize = compAmmoUser.MagSize;
			if (compAmmoUser.UseAmmo && flag && !TrackingSatisfied(pawn, ammoType, magazineSize))
			{
				AmmoDef ammoDef = (from al in compAmmoUser.Props.ammoSet.ammoTypes
					where al.ammo != ammoType
					select al.ammo).FirstOrDefault((AmmoDef ad) => TrackingSatisfied(pawn, ad, magazineSize) && inventory.AmmoCountOfDef(ad) >= magazineSize);
				if (ammoDef != null)
				{
					reloadWeapon = item;
					reloadAmmo = ammoDef;
					return true;
				}
			}
			if (compAmmoUser.CurMagCount < magazineSize && (!compAmmoUser.UseAmmo || inventory.AmmoCountOfDef(ammoType) >= magazineSize - compAmmoUser.CurMagCount))
			{
				reloadWeapon = item;
				reloadAmmo = ammoType;
				return true;
			}
		}
		return false;
	}

	private bool TrackingSatisfied(Pawn pawn, ThingDef def, int amount)
	{
		Loadout loadout = pawn.GetLoadout();
		foreach (LoadoutSlot slot in loadout.Slots)
		{
			if (slot.thingDef != null)
			{
				if (slot.thingDef == def)
				{
					amount -= slot.count;
				}
			}
			else if (slot.genericDef != null && slot.genericDef.lambda(def))
			{
				amount -= slot.count;
			}
			if (amount <= 0)
			{
				return true;
			}
		}
		List<HoldRecord> holdRecords = pawn.GetHoldRecords();
		if (!holdRecords.NullOrEmpty())
		{
			foreach (HoldRecord item in holdRecords)
			{
				if (item.thingDef == def)
				{
					amount -= item.count;
				}
			}
		}
		return false;
	}
}
