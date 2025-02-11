using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class LoadoutPropertiesExtension : DefModExtension
{
	public FloatRange primaryMagazineCount = FloatRange.Zero;

	public AttachmentOption primaryAttachments;

	public int minAmmoCount;

	public FloatRange shieldMoney = FloatRange.Zero;

	public List<string> shieldTags;

	public float shieldChance = 0f;

	public SidearmOption forcedSidearm;

	public List<SidearmOption> sidearms;

	public AmmoCategoryDef forcedAmmoCategory;

	private static List<ThingStuffPair> allWeaponPairs;

	private static List<ThingStuffPair> allShieldPairs;

	private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

	private static List<ThingStuffPair> workingShields = new List<ThingStuffPair>();

	private static List<AttachmentLink> attachmentLinks = new List<AttachmentLink>();

	private static List<AttachmentLink> selectedAttachments = new List<AttachmentLink>();

	public static void Reset()
	{
		Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty();
		allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
		foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => isWeapon(td)))
		{
			float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
			float num2 = thingDef.generateCommonality / num;
			if (num2 == 1f)
			{
				continue;
			}
			for (int i = 0; i < allWeaponPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = allWeaponPairs[i];
				if (thingStuffPair.thing == thingDef)
				{
					allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
				}
			}
		}
		allShieldPairs = ThingStuffPair.AllWith((ThingDef td) => td.thingClass == typeof(Apparel_Shield));
	}

	public void GenerateLoadoutFor(Pawn pawn, float biocodeWeaponChance)
	{
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Violent) || !pawn.RaceProps.ToolUser)
		{
			return;
		}
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null)
		{
			Log.Error("CE tried generating loadout for " + pawn.ToStringSafe() + " without CompInventory");
			return;
		}
		if (forcedSidearm != null)
		{
			TryGenerateWeaponWithAmmoFor(pawn, compInventory, forcedSidearm, biocodeWeaponChance);
		}
		ThingWithComps primary = pawn.equipment.Primary;
		if (primary != null)
		{
			LoadWeaponWithRandAmmo(primary);
			compInventory.UpdateInventory();
			if (primaryAttachments != null && primary is WeaponPlatform weapon)
			{
				TryGenerateAttachments(compInventory, weapon, primaryAttachments);
			}
			TryGenerateAmmoFor(pawn.equipment.Primary, compInventory, Mathf.RoundToInt(primaryMagazineCount.RandomInRange));
		}
		TryGenerateShieldFor(pawn, compInventory, primary);
		if (sidearms.NullOrEmpty())
		{
			return;
		}
		foreach (SidearmOption sidearm in sidearms)
		{
			TryGenerateWeaponWithAmmoFor(pawn, compInventory, sidearm, biocodeWeaponChance);
		}
	}

	public void TryGenerateAttachments(CompInventory inventory, WeaponPlatform weapon, AttachmentOption option)
	{
		selectedAttachments.Clear();
		attachmentLinks.Clear();
		attachmentLinks.AddRange(weapon.Platform.attachmentLinks);
		if (option.attachmentTags != null && option.attachmentTags.Count > 0)
		{
			attachmentLinks.RemoveAll((AttachmentLink l) => l.attachment.attachmentTags.All((string s) => !option.attachmentTags.Contains(s)));
		}
		float num = inventory.GetAvailableWeight();
		float num2 = inventory.GetAvailableBulk();
		foreach (AttachmentLink link in attachmentLinks.InRandomOrder())
		{
			if ((float)selectedAttachments.Count >= option.attachmentCount.max || num <= 0f || num2 <= 0f)
			{
				break;
			}
			float statValueAbstract = link.attachment.GetStatValueAbstract(StatDefOf.Mass);
			float statValueAbstract2 = link.attachment.GetStatValueAbstract(CE_StatDefOf.Bulk);
			if (!(num < statValueAbstract) && !(num2 < statValueAbstract2) && !selectedAttachments.Any((AttachmentLink l) => !l.CompatibleWith(link)) && (option.attachmentCount.min > (float)selectedAttachments.Count || Rand.ChanceSeeded(weapon.def.generateAllowChance, weapon.thingIDNumber ^ weapon.def.shortHash ^ 0x1B3B648)))
			{
				selectedAttachments.Add(link);
				num -= statValueAbstract;
				num2 -= statValueAbstract2;
			}
		}
		weapon.TargetConfig = selectedAttachments.Select((AttachmentLink l) => l.attachment).ToList();
		weapon.attachments.Clear();
		weapon.attachments.AddRange(selectedAttachments);
		weapon.UpdateConfiguration();
	}

	private void TryGenerateWeaponWithAmmoFor(Pawn pawn, CompInventory inventory, SidearmOption option, float biocodeChance)
	{
		if (option.weaponTags.NullOrEmpty() || !Rand.Chance(option.generateChance))
		{
			return;
		}
		float randomInRange = option.sidearmMoney.RandomInRange;
		for (int i = 0; i < allWeaponPairs.Count; i++)
		{
			ThingStuffPair w2 = allWeaponPairs[i];
			if (w2.Price <= randomInRange && (option.weaponTags == null || option.weaponTags.Any((string tag) => w2.thing.weaponTags.Contains(tag))) && (w2.thing.generateAllowChance >= 1f || Rand.ChanceSeeded(w2.thing.generateAllowChance, pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
			{
				workingWeapons.Add(w2);
			}
		}
		if (workingWeapons.Count == 0)
		{
			return;
		}
		if (workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out var result))
		{
			ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
			if (Rand.Value < biocodeChance)
			{
				thingWithComps.TryGetComp<CompBiocodable>()?.CodeFor(pawn);
			}
			LoadWeaponWithRandAmmo(thingWithComps);
			if (inventory.CanFitInInventory(thingWithComps, out var _))
			{
				PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
				if (inventory.container.TryAdd(thingWithComps))
				{
					TryGenerateAmmoFor(thingWithComps, inventory, Mathf.RoundToInt(option.magazineCount.RandomInRange));
					inventory.UpdateInventory();
					if (option.attachments != null && thingWithComps is WeaponPlatform weapon)
					{
						TryGenerateAttachments(inventory, weapon, option.attachments);
					}
				}
			}
		}
		workingWeapons.Clear();
	}

	private void LoadWeaponWithRandAmmo(ThingWithComps gun)
	{
		CompAmmoUser compAmmoUser = gun.TryGetComp<CompAmmoUser>();
		if (compAmmoUser == null)
		{
			return;
		}
		if (!compAmmoUser.UseAmmo)
		{
			compAmmoUser.ResetAmmoCount();
			return;
		}
		IEnumerable<AmmoDef> source = from a in compAmmoUser.Props.ammoSet.ammoTypes
			where a.ammo.alwaysHaulable && !a.ammo.menuHidden && (a.ammo.generateAllowChance > 0f || a.ammo.ammoClass == forcedAmmoCategory)
			select a.ammo;
		AmmoDef newAmmo = source.RandomElementByWeight((AmmoDef a) => a.generateAllowChance);
		if (forcedAmmoCategory != null && source.Any((AmmoDef x) => x.ammoClass == forcedAmmoCategory))
		{
			newAmmo = source.Where((AmmoDef x) => x.ammoClass == forcedAmmoCategory).FirstOrFallback();
		}
		compAmmoUser.ResetAmmoCount(newAmmo);
	}

	private void TryGenerateAmmoFor(ThingWithComps gun, CompInventory inventory, int ammoCount)
	{
		if (ammoCount <= 0)
		{
			return;
		}
		int num = 1;
		int val = minAmmoCount;
		CompAmmoUser compAmmoUser = gun.TryGetComp<CompAmmoUser>();
		ThingDef thingDef;
		if (compAmmoUser == null || !compAmmoUser.UseAmmo)
		{
			if (!(gun.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.verbClass == typeof(Verb_ShootCEOneUse)))
			{
				List<string> weaponTags = gun.def.weaponTags;
				if (weaponTags == null || !weaponTags.Contains("CE_AmmoGen_Disposable"))
				{
					return;
				}
			}
			thingDef = gun.def;
		}
		else
		{
			thingDef = compAmmoUser.CurrentAmmo;
			int b = ((compAmmoUser.MagSizeOverride > 0) ? compAmmoUser.MagSizeOverride : compAmmoUser.MagSize);
			num = Mathf.Max(1, b);
			if (forcedAmmoCategory != null)
			{
				IEnumerable<AmmoDef> source = from a in compAmmoUser.Props.ammoSet.ammoTypes
					where a.ammo.alwaysHaulable && !a.ammo.menuHidden && (a.ammo.generateAllowChance > 0f || a.ammo.ammoClass == forcedAmmoCategory)
					select a.ammo;
				if (source.Any((AmmoDef x) => x.ammoClass == forcedAmmoCategory))
				{
					thingDef = source.Where((AmmoDef x) => x.ammoClass == forcedAmmoCategory).FirstOrFallback();
				}
			}
		}
		Thing thing = (thingDef.MadeFromStuff ? ThingMaker.MakeThing(thingDef, gun.Stuff) : ThingMaker.MakeThing(thingDef));
		thing.stackCount = Math.Max(ammoCount * num, val);
		if (inventory.CanFitInInventory(thing, out var count))
		{
			if (count < thing.stackCount)
			{
				thing.stackCount = count - count % num;
			}
			inventory.container.TryAdd(thing);
		}
	}

	private void TryGenerateShieldFor(Pawn pawn, CompInventory inventory, ThingWithComps primary)
	{
		if ((primary != null && !primary.def.weaponTags.Contains("CE_OneHandedWeapon")) || shieldTags.NullOrEmpty() || pawn.apparel == null || !Rand.Chance(shieldChance))
		{
			return;
		}
		float randomInRange = shieldMoney.RandomInRange;
		foreach (ThingStuffPair cur in allShieldPairs)
		{
			if (cur.Price < randomInRange && shieldTags.Any((string t) => cur.thing.apparel.tags.Contains(t)) && (cur.thing.generateAllowChance >= 1f || Rand.ValueSeeded(pawn.thingIDNumber ^ 0x4188544) <= cur.thing.generateAllowChance) && pawn.apparel.CanWearWithoutDroppingAnything(cur.thing) && ApparelUtility.HasPartsToWear(pawn, cur.thing))
			{
				workingShields.Add(cur);
			}
		}
		if (workingShields.Count == 0)
		{
			return;
		}
		if (workingShields.TryRandomElementByWeight((ThingStuffPair p) => p.Commonality * p.Price, out var result))
		{
			Apparel apparel = (Apparel)ThingMaker.MakeThing(result.thing, result.stuff);
			if (inventory.CanFitInInventory(apparel, out var _, ignoreEquipment: false, useApparelCalculations: true))
			{
				pawn.apparel.Wear(apparel);
			}
		}
		workingShields.Clear();
	}
}
