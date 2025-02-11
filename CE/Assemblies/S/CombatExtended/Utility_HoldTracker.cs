using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

internal static class Utility_HoldTracker
{
	private static int _tickLastPurge;

	public static void Notify_HoldTrackerItem(this Pawn pawn, Thing item, int count)
	{
		if (pawn.GetLoadout().defaultLoadout)
		{
			return;
		}
		List<HoldRecord> list = LoadoutManager.GetHoldRecords(pawn);
		if (list == null)
		{
			list = new List<HoldRecord>();
			LoadoutManager.AddHoldRecords(pawn, list);
		}
		HoldRecord holdRecord = list.FirstOrDefault((HoldRecord hr) => hr.thingDef == item.def);
		if (holdRecord != null)
		{
			if (holdRecord.pickedUp)
			{
				holdRecord.count = GetMagazineAwareStackCount(pawn, holdRecord.thingDef) + count;
			}
			else
			{
				holdRecord.count += count;
			}
		}
		else
		{
			holdRecord = new HoldRecord(item.def, count);
			list.Add(holdRecord);
		}
	}

	public static bool HoldTrackerIsHeld(this Pawn pawn, Thing thing)
	{
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		if (holdRecords != null && holdRecords.Any((HoldRecord hr) => hr.thingDef == thing.def))
		{
			return true;
		}
		return false;
	}

	public static bool HoldTrackerAnythingHeld(this Pawn pawn)
	{
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		if (holdRecords == null || holdRecords.NullOrEmpty())
		{
			return false;
		}
		return holdRecords.Any((HoldRecord r) => r.pickedUp);
	}

	public static void HoldTrackerClear(this Pawn pawn)
	{
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		holdRecords.Clear();
	}

	public static void HoldTrackerCleanUp(this Pawn pawn)
	{
		if (_tickLastPurge <= GenTicks.TicksAbs)
		{
			LoadoutManager.PurgeHoldTrackerRolls();
			_tickLastPurge = GenTicks.TicksAbs + 60000;
		}
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (holdRecords == null || compInventory == null)
		{
			return;
		}
		for (int num = holdRecords.Count - 1; num > 0; num--)
		{
			if (holdRecords[num].pickedUp && compInventory.container.TotalStackCountOfDef(holdRecords[num].thingDef) <= 0)
			{
				holdRecords.RemoveAt(num);
			}
		}
	}

	public static void HoldTrackerForget(this Pawn pawn, Thing thing)
	{
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		if (holdRecords == null)
		{
			Log.Error(string.Concat(pawn.Name, " wasn't being tracked by HoldTracker and tried to forget a ThingDef ", thing.def, "."));
		}
		else
		{
			HoldRecord holdRecord = holdRecords.FirstOrDefault((HoldRecord hr) => hr.thingDef == thing.def);
			if (holdRecord != null)
			{
				holdRecords.RemoveAt(holdRecords.IndexOf(holdRecord));
			}
		}
	}

	public static List<HoldRecord> GetHoldRecords(this Pawn pawn)
	{
		return LoadoutManager.GetHoldRecords(pawn);
	}

	public static bool HasExcessThing(this Pawn pawn)
	{
		ThingWithComps dropEquipment;
		Thing dropThing;
		int dropCount;
		return pawn.GetExcessEquipment(out dropEquipment) || pawn.GetExcessThing(out dropThing, out dropCount);
	}

	public static bool GetExcessEquipment(this Pawn pawn, out ThingWithComps dropEquipment)
	{
		Loadout loadout = pawn.GetLoadout();
		dropEquipment = null;
		if (loadout == null || (loadout != null && loadout.Slots.NullOrEmpty()) || pawn.equipment?.Primary == null)
		{
			return false;
		}
		if (pawn.IsItemQuestLocked(pawn.equipment?.Primary))
		{
			return false;
		}
		LoadoutSlot loadoutSlot = loadout.Slots.FirstOrDefault((LoadoutSlot s) => s.count >= 1 && ((s.thingDef != null && s.thingDef == pawn.equipment.Primary.def) || (s.genericDef != null && s.genericDef.lambda(pawn.equipment.Primary.def))));
		HoldRecord holdRecord = pawn.GetHoldRecords()?.FirstOrDefault((HoldRecord s) => s.count >= 1 && s.thingDef != null && s.thingDef == pawn.equipment.Primary.def);
		if (loadoutSlot == null && holdRecord == null)
		{
			dropEquipment = pawn.equipment.Primary;
			return true;
		}
		return false;
	}

	public static bool HasAnythingForDrop(this Pawn pawn)
	{
		Thing dropThing;
		int dropCount;
		return pawn.GetAnythingForDrop(out dropThing, out dropCount);
	}

	public static bool GetAnythingForDrop(this Pawn pawn, out Thing dropThing, out int dropCount)
	{
		dropThing = null;
		dropCount = 0;
		if (pawn.inventory == null || pawn.inventory.innerContainer == null)
		{
			return false;
		}
		Loadout loadout = pawn.GetLoadout();
		if (loadout == null || loadout.Slots.NullOrEmpty())
		{
			List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
			if (holdRecords != null)
			{
				foreach (Thing thing in pawn.inventory.innerContainer)
				{
					if (!pawn.IsItemQuestLocked(thing))
					{
						int num = pawn.inventory.innerContainer.TotalStackCountOfDef(thing.def);
						HoldRecord holdRecord = holdRecords.FirstOrDefault((HoldRecord hr) => hr.thingDef == thing.def);
						if (holdRecord == null)
						{
							dropThing = thing;
							dropCount = ((num > dropThing.stackCount) ? dropThing.stackCount : num);
							return true;
						}
						if (num > holdRecord.count)
						{
							dropThing = thing;
							dropCount = num - holdRecord.count;
							dropCount = ((dropCount > dropThing.stackCount) ? dropThing.stackCount : dropCount);
							return true;
						}
					}
				}
			}
			else
			{
				dropThing = pawn.inventory.innerContainer.Where((Thing inventoryItem) => !pawn.IsItemQuestLocked(inventoryItem))?.RandomElement();
				dropCount = dropThing?.stackCount ?? 0;
			}
			return false;
		}
		return pawn.GetExcessThing(out dropThing, out dropCount);
	}

	public static Dictionary<ThingDef, Integer> GetStorageByThingDef(this Pawn pawn)
	{
		Dictionary<ThingDef, Integer> dictionary = new Dictionary<ThingDef, Integer>();
		if (pawn.equipment?.Primary != null)
		{
			dictionary.Add(pawn.equipment.Primary.def, new Integer(1));
			CompAmmoUser compAmmoUser = pawn.equipment.Primary.TryGetComp<CompAmmoUser>();
			if (compAmmoUser != null && compAmmoUser.UseAmmo && compAmmoUser.CurrentAmmo != null)
			{
				dictionary.Add(compAmmoUser.CurrentAmmo, new Integer(compAmmoUser.CurMagCount));
			}
		}
		foreach (Thing item in pawn.inventory.innerContainer)
		{
			Thing innerIfMinified = item.GetInnerIfMinified();
			if (innerIfMinified != null && !dictionary.ContainsKey(innerIfMinified.def))
			{
				dictionary.Add(innerIfMinified.def, new Integer(0));
			}
			dictionary[innerIfMinified.def].value += innerIfMinified.stackCount;
			CompAmmoUser compAmmoUser = innerIfMinified.TryGetComp<CompAmmoUser>();
			if (compAmmoUser != null && compAmmoUser.UseAmmo && compAmmoUser.CurrentAmmo != null)
			{
				if (dictionary.ContainsKey(compAmmoUser.CurrentAmmo))
				{
					dictionary[compAmmoUser.CurrentAmmo].value += compAmmoUser.CurMagCount;
				}
				else
				{
					dictionary.Add(compAmmoUser.CurrentAmmo, new Integer(compAmmoUser.CurMagCount));
				}
			}
		}
		return dictionary;
	}

	public static bool GetExcessThing(this Pawn pawn, out Thing dropThing, out int dropCount)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		Loadout loadout = pawn.GetLoadout();
		List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(pawn);
		dropThing = null;
		dropCount = 0;
		if (compInventory == null || compInventory.container == null || loadout == null || loadout.Slots.NullOrEmpty())
		{
			return false;
		}
		Dictionary<ThingDef, Integer> storageByThingDef = pawn.GetStorageByThingDef();
		foreach (LoadoutSlot slot in loadout.Slots)
		{
			if (slot.thingDef != null && storageByThingDef.ContainsKey(slot.thingDef))
			{
				storageByThingDef[slot.thingDef].value -= slot.count;
				if (storageByThingDef[slot.thingDef].value <= 0)
				{
					storageByThingDef.Remove(slot.thingDef);
				}
			}
			if (slot.genericDef == null)
			{
				continue;
			}
			List<ThingDef> list = new List<ThingDef>();
			int num = slot.count;
			foreach (ThingDef item in storageByThingDef.Keys.Where((ThingDef td) => slot.genericDef.lambda(td)))
			{
				storageByThingDef[item].value -= num;
				if (storageByThingDef[item].value <= 0)
				{
					num = -storageByThingDef[item].value;
					list.Add(item);
					continue;
				}
				break;
			}
			foreach (ThingDef item2 in list)
			{
				storageByThingDef.Remove(item2);
			}
		}
		if (storageByThingDef.Any())
		{
			if (holdRecords != null && !holdRecords.NullOrEmpty())
			{
				foreach (ThingDef def in storageByThingDef.Keys)
				{
					HoldRecord holdRecord = holdRecords.FirstOrDefault((HoldRecord r) => r.thingDef == def);
					if (holdRecord == null)
					{
						dropThing = compInventory.container.FirstOrDefault((Thing t) => t.def == def && !pawn.IsItemQuestLocked(t));
						if (dropThing != null)
						{
							dropCount = ((storageByThingDef[def].value > dropThing.stackCount) ? dropThing.stackCount : storageByThingDef[def].value);
							return true;
						}
					}
					else if (holdRecord.count < storageByThingDef[def].value)
					{
						dropThing = pawn.inventory.innerContainer.FirstOrDefault((Thing t) => t.def == def && !pawn.IsItemQuestLocked(t));
						if (dropThing != null)
						{
							dropCount = storageByThingDef[def].value - holdRecord.count;
							dropCount = ((dropCount > dropThing.stackCount) ? dropThing.stackCount : dropCount);
							return true;
						}
					}
				}
			}
			else
			{
				foreach (ThingDef def2 in storageByThingDef.Keys)
				{
					dropThing = compInventory.container.FirstOrDefault((Thing t) => t.GetInnerIfMinified().def == def2 && !pawn.IsItemQuestLocked(t));
					if (dropThing != null)
					{
						dropCount = ((storageByThingDef[def2].value > dropThing.stackCount) ? dropThing.stackCount : storageByThingDef[def2].value);
						return true;
					}
				}
			}
		}
		return false;
	}

	private static int GetMagazineAwareStackCount(Pawn pawn, ThingDef thingDef)
	{
		int num = 0;
		CompInventory compInventory = pawn?.TryGetComp<CompInventory>();
		num += (compInventory?.container?.TotalStackCountOfDef(thingDef)).GetValueOrDefault();
		if (thingDef is AmmoDef)
		{
			if (pawn?.equipment?.Primary != null)
			{
				CompAmmoUser compAmmoUser = pawn.equipment.Primary.TryGetComp<CompAmmoUser>();
				if (compAmmoUser != null && compAmmoUser.HasMagazine && compAmmoUser.CurrentAmmo == thingDef)
				{
					num += compAmmoUser.CurMagCount;
				}
			}
			if (compInventory?.container != null)
			{
				foreach (Thing item in (IEnumerable<Thing>)compInventory.container)
				{
					CompAmmoUser compAmmoUser2 = item.TryGetComp<CompAmmoUser>();
					if (compAmmoUser2 != null && compAmmoUser2.HasMagazine && compAmmoUser2.CurrentAmmo == thingDef)
					{
						num += compAmmoUser2.CurMagCount;
					}
				}
			}
		}
		return num;
	}
}
