using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public static class MTAAmmoUtility
{
	public static Thing FindBestAmmo(this Pawn pawn, ThingDef ammoDef)
	{
		if (pawn == null || ammoDef == null)
		{
			return null;
		}
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ammoDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn));
	}

	public static List<FloatMenuOption> BuildAmmoOptions(this CompAmmoUser ammoUser, CompMechAmmo forMech = null)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		if (ammoUser == null)
		{
			return list;
		}
		foreach (AmmoLink ammoLink in ammoUser.Props.ammoSet.ammoTypes)
		{
			if (ammoLink.ammo == ammoUser.SelectedAmmo)
			{
				continue;
			}
			FloatMenuOption item = new FloatMenuOption(ammoLink.ammo.ammoClass.label, delegate
			{
				if (ammoUser.CurMagCount > 0)
				{
					ammoUser.TryUnload();
				}
				ammoUser.SelectedAmmo = ammoLink.ammo;
				if (forMech != null)
				{
					forMech.TakeAmmoNow();
				}
			});
			list.Add(item);
		}
		return list;
	}

	public static int NeedAmmo(this CompAmmoUser ammoUser, AmmoDef ammoDef, int amount)
	{
		int num = 0;
		if (ammoUser == null)
		{
			return 0;
		}
		if (ammoUser.CurrentAmmo == ammoDef)
		{
			num = ammoUser.CurMagCount;
		}
		foreach (Thing item in ammoUser.Holder.inventory.innerContainer)
		{
			if (item.def == ammoDef)
			{
				num += item.stackCount;
			}
		}
		return amount - num;
	}
}
