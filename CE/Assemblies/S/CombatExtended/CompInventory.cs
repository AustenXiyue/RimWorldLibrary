using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class CompInventory : ThingComp
{
	private int age = 0;

	private Pawn parentPawnInt = null;

	private const int CLEANUPTICKINTERVAL = 2100;

	private float currentWeightCached;

	private float currentBulkCached;

	private List<Thing> ammoListCached = new List<Thing>();

	private List<ThingWithComps> meleeWeaponListCached = new List<ThingWithComps>();

	private List<ThingWithComps> rangedWeaponListCached = new List<ThingWithComps>();

	public CompProperties_Inventory Props => (CompProperties_Inventory)props;

	public float currentWeight => currentWeightCached;

	public float currentBulk => currentBulkCached;

	private float availableWeight => capacityWeight - currentWeight;

	private float availableBulk => capacityBulk - currentBulk;

	public float capacityBulk => parentPawn.GetStatValue(CE_StatDefOf.CarryBulk);

	public float capacityWeight => parentPawn.GetStatValue(CE_StatDefOf.CarryWeight);

	private Pawn parentPawn
	{
		get
		{
			if (parentPawnInt == null)
			{
				parentPawnInt = parent as Pawn;
			}
			return parentPawnInt;
		}
	}

	public float moveSpeedFactor => MassBulkUtility.MoveSpeedFactor(currentWeight, capacityWeight);

	public float dodgeChanceFactorWeight => MassBulkUtility.DodgeWeightFactor(currentWeight, capacityWeight);

	public float workSpeedFactor => MassBulkUtility.WorkSpeedFactor(currentBulk, capacityBulk);

	public float encumberPenalty => MassBulkUtility.EncumberPenalty(currentWeight, capacityWeight);

	public IEnumerable<ThingWithComps> weapons
	{
		get
		{
			if (meleeWeaponList != null)
			{
				foreach (ThingWithComps meleeWeapon in meleeWeaponList)
				{
					yield return meleeWeapon;
				}
			}
			if (rangedWeaponList == null)
			{
				yield break;
			}
			foreach (ThingWithComps rangedWeapon in rangedWeaponList)
			{
				yield return rangedWeapon;
			}
		}
	}

	public ThingOwner container
	{
		get
		{
			if (parentPawn.inventory != null)
			{
				return parentPawn.inventory.innerContainer;
			}
			return null;
		}
	}

	public List<Thing> ammoList => ammoListCached;

	public List<ThingWithComps> meleeWeaponList => meleeWeaponListCached;

	public List<ThingWithComps> rangedWeaponList => rangedWeaponListCached;

	public float GetAvailableWeight(bool updateInventory = true)
	{
		if (updateInventory)
		{
			UpdateInventory();
		}
		return availableWeight;
	}

	public float GetAvailableBulk(bool updateInventory = true)
	{
		if (updateInventory)
		{
			UpdateInventory();
		}
		return availableBulk;
	}

	public int AmmoCountOfDef(AmmoDef def)
	{
		return ammoListCached.Where((Thing t) => t.def == def).Sum((Thing t) => t.stackCount);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		UpdateInventory();
	}

	public void UpdateInventory()
	{
		if (parentPawn == null)
		{
			Log.Error("CompInventory on non-pawn " + parent.ToString());
			return;
		}
		float bulk = 0f;
		float weight = 0f;
		if (parentPawn.equipment != null && parentPawn.equipment.Primary != null)
		{
			GetEquipmentStats(parentPawn.equipment.Primary, out weight, out bulk);
		}
		if (parentPawn.apparel != null && parentPawn.apparel.WornApparelCount > 0)
		{
			foreach (Apparel item in parentPawn.apparel.WornApparel)
			{
				float statValue = item.GetStatValue(CE_StatDefOf.WornBulk);
				float statValue2 = item.GetStatValue(StatDefOf.Mass);
				bulk += statValue;
				weight += statValue2;
				if (age <= 2100 || !(statValue > 0f))
				{
					continue;
				}
				Pawn pawn = parentPawn;
				if (pawn != null && pawn.Spawned)
				{
					Faction factionInt = ((Thing)parentPawn).factionInt;
					if (factionInt != null && factionInt.IsPlayer)
					{
						LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_WornBulk, OpportunityType.GoodToKnow);
					}
				}
			}
		}
		if (parentPawn.inventory != null && parentPawn.inventory.innerContainer != null)
		{
			ammoListCached.Clear();
			meleeWeaponListCached.Clear();
			rangedWeaponListCached.Clear();
			List<HoldRecord> holdRecords = LoadoutManager.GetHoldRecords(parentPawn);
			foreach (Thing thing in parentPawn.inventory.innerContainer)
			{
				ThingWithComps thingWithComps = thing as ThingWithComps;
				CompEquippable compEquippable = thing.TryGetComp<CompEquippable>();
				if (thingWithComps != null && compEquippable != null)
				{
					if (thingWithComps.def.IsRangedWeapon)
					{
						rangedWeaponListCached.Add(thingWithComps);
					}
					else
					{
						meleeWeaponListCached.Add(thingWithComps);
					}
					GetEquipmentStats(thingWithComps, out var weight2, out var bulk2);
					weight += weight2 * (float)thing.stackCount;
					bulk += bulk2 * (float)thing.stackCount;
				}
				else
				{
					bulk += thing.GetStatValue(CE_StatDefOf.Bulk) * (float)thing.stackCount;
					weight += thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount;
				}
				if (thing.def is AmmoDef)
				{
					ammoListCached.Add(thing);
				}
				if (holdRecords != null)
				{
					HoldRecord holdRecord = holdRecords.FirstOrDefault((HoldRecord hr) => hr.thingDef == thing.def);
					if (holdRecord != null && !holdRecord.pickedUp)
					{
						holdRecord.pickedUp = true;
					}
				}
			}
		}
		currentBulkCached = bulk;
		currentWeightCached = weight;
	}

	public bool CanFitInInventory(ThingDef thingDef, out int count, bool ignoreEquipment = false, bool useApparelCalculations = false)
	{
		float statValueAbstract;
		float statValueAbstract2;
		if (useApparelCalculations)
		{
			statValueAbstract = thingDef.GetStatValueAbstract(StatDefOf.Mass);
			statValueAbstract2 = thingDef.GetStatValueAbstract(CE_StatDefOf.WornBulk);
			if (statValueAbstract <= 0f && statValueAbstract2 <= 0f)
			{
				count = 1;
				return true;
			}
			statValueAbstract -= thingDef.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryWeight);
			statValueAbstract2 -= thingDef.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryBulk);
		}
		else
		{
			statValueAbstract = thingDef.GetStatValueAbstract(StatDefOf.Mass);
			statValueAbstract2 = thingDef.GetStatValueAbstract(CE_StatDefOf.Bulk);
		}
		float bulk = 0f;
		float weight = 0f;
		if (ignoreEquipment && parentPawn.equipment != null && parentPawn.equipment.Primary != null)
		{
			ThingWithComps primary = parentPawn.equipment.Primary;
			GetEquipmentStats(primary, out weight, out bulk);
		}
		float num = ((statValueAbstract <= 0f) ? 1f : ((availableWeight + weight) / statValueAbstract));
		float num2 = ((statValueAbstract2 <= 0f) ? 1f : ((availableBulk + bulk) / statValueAbstract2));
		count = Mathf.FloorToInt(Mathf.Min(num2, num, 1f));
		return count > 0;
	}

	public bool CanFitInInventory(Thing thing, out int count, bool ignoreEquipment = false, bool useApparelCalculations = false)
	{
		float statValue;
		float statValue2;
		if (useApparelCalculations)
		{
			statValue = thing.GetStatValue(StatDefOf.Mass);
			statValue2 = thing.GetStatValue(CE_StatDefOf.WornBulk);
			if (statValue <= 0f && statValue2 <= 0f)
			{
				count = 1;
				return true;
			}
			statValue -= thing.def.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryWeight);
			statValue2 -= thing.def.equippedStatOffsets.GetStatOffsetFromList(CE_StatDefOf.CarryBulk);
		}
		else
		{
			statValue = thing.GetStatValue(StatDefOf.Mass);
			statValue2 = thing.GetStatValue(CE_StatDefOf.Bulk);
		}
		float bulk = 0f;
		float weight = 0f;
		if (ignoreEquipment && parentPawn.equipment != null && parentPawn.equipment.Primary != null)
		{
			ThingWithComps primary = parentPawn.equipment.Primary;
			GetEquipmentStats(primary, out weight, out bulk);
		}
		float num = ((statValue <= 0f) ? ((float)thing.stackCount) : ((availableWeight + weight) / statValue));
		float num2 = ((statValue2 <= 0f) ? ((float)thing.stackCount) : ((availableBulk + bulk) / statValue2));
		count = Mathf.FloorToInt(Mathf.Min(num2, num, thing.stackCount));
		return count > 0;
	}

	public static void GetEquipmentStats(ThingWithComps eq, out float weight, out float bulk)
	{
		weight = eq.GetStatValue(StatDefOf.Mass);
		bulk = eq.GetStatValue(CE_StatDefOf.Bulk);
	}

	public bool SwitchToNextViableWeapon(bool useFists = false, bool useAOE = false, bool stopJob = true, Func<ThingWithComps, CompAmmoUser, bool> predicate = null)
	{
		if (parentPawn.equipment?.Primary?.def.weaponTags?.Contains("NoSwitch") == true)
		{
			return false;
		}
		ThingWithComps thingWithComps = null;
		if (parentPawn.jobs != null && stopJob)
		{
			parentPawn.jobs.StopAll();
		}
		foreach (ThingWithComps item in rangedWeaponListCached)
		{
			if (parentPawn.equipment == null || parentPawn.equipment.Primary == item)
			{
				continue;
			}
			CompAmmoUser compAmmoUser = item.TryGetComp<CompAmmoUser>();
			if ((useAOE || !item.def.IsAOEWeapon()) && !item.def.IsIlluminationDevice())
			{
				Func<ThingWithComps, CompAmmoUser, bool> func = predicate;
				if (((func == null || func(item, compAmmoUser)) && compAmmoUser == null) || compAmmoUser.HasAndUsesAmmoOrMagazine)
				{
					thingWithComps = item;
					break;
				}
			}
		}
		if (thingWithComps == null)
		{
			IEnumerable<ThingWithComps> source;
			if (predicate != null)
			{
				source = meleeWeaponListCached.Where((ThingWithComps w) => predicate(w, null));
			}
			else
			{
				IEnumerable<ThingWithComps> enumerable = meleeWeaponListCached;
				source = enumerable;
			}
			thingWithComps = source.FirstOrDefault();
		}
		if (thingWithComps != null)
		{
			if (!stopJob)
			{
				parentPawn.jobs.StartJob(JobMaker.MakeJob(CE_JobDefOf.EquipFromInventory, thingWithComps), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			}
			else
			{
				TrySwitchToWeapon(thingWithComps, stopJob);
			}
			return true;
		}
		if (useFists)
		{
			ThingWithComps thingWithComps2 = parentPawn.equipment?.Primary;
			if (thingWithComps2 != null && !parentPawn.equipment.TryTransferEquipmentToContainer(thingWithComps2, container))
			{
				if (parentPawn.Position.InBounds(parentPawn.Map))
				{
					parentPawn.equipment.TryDropEquipment(thingWithComps2, out var _, parentPawn.Position);
				}
				else if (!thingWithComps2.Destroyed)
				{
					thingWithComps2.Destroy();
				}
			}
			return true;
		}
		return false;
	}

	public bool TryFindRandomAOEWeapon(out ThingWithComps weapon, Func<ThingWithComps, bool> predicate = null, bool checkAmmo = false)
	{
		weapon = null;
		foreach (ThingWithComps item in rangedWeaponListCached.InRandomOrder())
		{
			if (checkAmmo)
			{
				CompAmmoUser compAmmoUser = item.TryGetComp<CompAmmoUser>();
				if (compAmmoUser != null && !compAmmoUser.HasAmmoOrMagazine)
				{
					continue;
				}
			}
			if (parentPawn.equipment != null && parentPawn.equipment.Primary != item && item.def.IsAOEWeapon() && (predicate == null || predicate(item)))
			{
				weapon = item;
				return true;
			}
		}
		return false;
	}

	public bool TryFindSmokeWeapon(out ThingWithComps grenade)
	{
		grenade = (ThingWithComps)container.FirstOrFallback((Thing t) => t.def.weaponTags?.Contains("GrenadeSmoke") ?? false);
		if (grenade == null)
		{
			return false;
		}
		CompAmmoUser compAmmoUser = grenade.TryGetComp<CompAmmoUser>();
		if (compAmmoUser != null)
		{
			if (compAmmoUser.CurAmmoProjectile?.projectile?.damageDef != DamageDefOf.Smoke)
			{
				return false;
			}
			ThingDef curAmmoProjectile = compAmmoUser.CurAmmoProjectile;
			if (curAmmoProjectile == null || curAmmoProjectile.projectile?.postExplosionGasType != GasType.BlindSmoke)
			{
				return false;
			}
		}
		return true;
	}

	public bool TryFindViableWeapon(out ThingWithComps weapon, bool useAOE = false, Func<ThingWithComps, CompAmmoUser, bool> predicate = null)
	{
		weapon = null;
		foreach (ThingWithComps item in rangedWeaponListCached)
		{
			if (parentPawn.equipment == null || parentPawn.equipment.Primary == item)
			{
				continue;
			}
			CompAmmoUser compAmmoUser = item.TryGetComp<CompAmmoUser>();
			if ((useAOE || !item.def.IsAOEWeapon()) && !item.def.IsIlluminationDevice())
			{
				Func<ThingWithComps, CompAmmoUser, bool> func = predicate;
				if (((func == null || func(item, compAmmoUser)) && compAmmoUser == null) || compAmmoUser.HasAndUsesAmmoOrMagazine)
				{
					weapon = item;
					break;
				}
			}
		}
		if (weapon == null)
		{
			IEnumerable<ThingWithComps> source;
			if (predicate != null)
			{
				source = meleeWeaponListCached.Where((ThingWithComps w) => predicate(w, null));
			}
			else
			{
				IEnumerable<ThingWithComps> enumerable = meleeWeaponListCached;
				source = enumerable;
			}
			weapon = source.FirstOrDefault();
		}
		return weapon != null;
	}

	public bool TryFindFlare(out ThingWithComps flareGun, bool checkAmmo = false)
	{
		foreach (ThingWithComps rangedWeapon in rangedWeaponList)
		{
			if (checkAmmo)
			{
				CompAmmoUser compAmmoUser = rangedWeapon.TryGetComp<CompAmmoUser>();
				if (compAmmoUser != null && !compAmmoUser.HasAmmoOrMagazine)
				{
					continue;
				}
			}
			if (rangedWeapon.def.IsIlluminationDevice())
			{
				CompAmmoUser compAmmoUser2 = rangedWeapon.TryGetComp<CompAmmoUser>();
				if (compAmmoUser2 == null || compAmmoUser2.HasAmmoOrMagazine)
				{
					flareGun = rangedWeapon;
					return true;
				}
			}
		}
		flareGun = null;
		return false;
	}

	public void TrySwitchToWeapon(ThingWithComps newEq, bool stopJob = true)
	{
		if (newEq == null || parentPawn.equipment == null || !container.Contains(newEq))
		{
			return;
		}
		if (parentPawn.jobs != null && stopJob)
		{
			parentPawn.jobs.StopAll();
		}
		if (parentPawn.equipment.Primary != null)
		{
			if (CanFitInInventory(parentPawn.equipment.Primary, out var _, ignoreEquipment: true))
			{
				parentPawn.equipment.TryTransferEquipmentToContainer(parentPawn.equipment.Primary, container);
			}
			else
			{
				parentPawn.equipment.MakeRoomFor(newEq);
			}
		}
		parentPawn.equipment.AddEquipment((ThingWithComps)container.Take(newEq, 1));
		if (newEq.def.soundInteract != null)
		{
			newEq.def.soundInteract.PlayOneShot(new TargetInfo(parent.Position, parent.MapHeld));
		}
	}

	public override void CompTick()
	{
		age++;
		if ((parentPawn.thingIDNumber + GenTicks.TicksGame) % 2100 == 0)
		{
			parentPawn.HoldTrackerCleanUp();
		}
		base.CompTick();
		if (Controller.settings.DebugEnableInventoryValidation)
		{
			ValidateCache();
		}
	}

	private void ValidateCache()
	{
		float num = currentWeight;
		float num2 = currentBulk;
		UpdateInventory();
		if (num != currentWeight || num2 != currentBulk)
		{
			Log.Error("Combat Extended :: CompInventory :: " + parent.ToString() + " failed inventory validation");
		}
	}
}
