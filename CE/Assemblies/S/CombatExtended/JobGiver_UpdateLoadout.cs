using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_UpdateLoadout : ThinkNode_JobGiver
{
	private enum ItemPriority : byte
	{
		None,
		Low,
		LowStock,
		Proximity
	}

	private const int TicksThrottleCooldown = 1800;

	private const int ProximitySearchRadius = 20;

	private const int MaximumSearchRadius = 80;

	private const int TicksBeforeDropRaw = 40000;

	private static Dictionary<int, int> _throttle;

	static JobGiver_UpdateLoadout()
	{
		_throttle = new Dictionary<int, int>();
		CacheClearComponent.AddClearCacheAction(delegate
		{
			_throttle.Clear();
		});
	}

	public override float GetPriority(Pawn pawn)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null)
		{
			return 0f;
		}
		if (!_throttle.TryGetValue(pawn.thingIDNumber, out var value) || GenTicks.TicksGame - value > 1800)
		{
			return 30f;
		}
		if (pawn.HasExcessThing())
		{
			return 9.2f;
		}
		ItemPriority itemPriority;
		Thing closestThing;
		int count;
		Pawn carriedBy;
		LoadoutSlot prioritySlot = GetPrioritySlot(pawn, out itemPriority, out closestThing, out count, out carriedBy);
		if (prioritySlot == null)
		{
			return 0f;
		}
		if (itemPriority == ItemPriority.Low)
		{
			return 1f;
		}
		TimeAssignmentDef timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
		if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
		{
			return 1f;
		}
		return 2.8f;
	}

	private static LoadoutSlot GetPrioritySlot(Pawn pawn, out ItemPriority priority, out Thing closestThing, out int count, out Pawn carriedBy)
	{
		priority = ItemPriority.None;
		LoadoutSlot result = null;
		closestThing = null;
		count = 0;
		carriedBy = null;
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null && compInventory.container != null)
		{
			Loadout loadout = pawn.GetLoadout();
			if (loadout != null && !loadout.Slots.NullOrEmpty())
			{
				Dictionary<ThingDef, Integer> storageByThingDef = pawn.GetStorageByThingDef();
				foreach (LoadoutSlot curSlot in loadout.Slots.Where((LoadoutSlot s) => s.countType != LoadoutCountType.dropExcess))
				{
					Thing curThing = null;
					ItemPriority curPriority = ItemPriority.None;
					Pawn curCarrier = null;
					int num = curSlot.count;
					if (curSlot.thingDef != null && storageByThingDef.ContainsKey(curSlot.thingDef))
					{
						int num2 = ((storageByThingDef[curSlot.thingDef].value >= num) ? num : storageByThingDef[curSlot.thingDef].value);
						storageByThingDef[curSlot.thingDef].value -= num2;
						num -= num2;
						if (storageByThingDef[curSlot.thingDef].value <= 0)
						{
							storageByThingDef.Remove(curSlot.thingDef);
						}
					}
					if (curSlot.genericDef != null)
					{
						List<ThingDef> list = new List<ThingDef>();
						foreach (ThingDef item in storageByThingDef.Keys.Where((ThingDef td) => curSlot.genericDef.lambda(td)))
						{
							int num3 = ((storageByThingDef[item].value >= num) ? num : storageByThingDef[item].value);
							storageByThingDef[item].value -= num3;
							num -= num3;
							if (storageByThingDef[item].value <= 0)
							{
								list.Add(item);
							}
							if (num <= 0)
							{
								break;
							}
						}
						foreach (ThingDef item2 in list)
						{
							storageByThingDef.Remove(item2);
						}
					}
					if (num <= 0)
					{
						continue;
					}
					FindPickup(pawn, curSlot, num, out curPriority, out curThing, out curCarrier);
					if ((int)curPriority > (int)priority && curThing != null && compInventory.CanFitInInventory(curThing, out count))
					{
						priority = curPriority;
						result = curSlot;
						count = ((count >= num) ? num : count);
						closestThing = curThing;
						if (curCarrier != null)
						{
							carriedBy = curCarrier;
						}
					}
					if ((int)priority >= 2)
					{
						break;
					}
				}
			}
		}
		return result;
	}

	private static void FindPickup(Pawn pawn, LoadoutSlot curSlot, int findCount, out ItemPriority curPriority, out Thing curThing, out Pawn curCarrier)
	{
		curPriority = ItemPriority.None;
		curThing = null;
		curCarrier = null;
		Predicate<Thing> isFoodInPrison = delegate(Thing t)
		{
			Room room = t.GetRoom();
			return room != null && room.IsPrisonCell && t.def.IsNutritionGivingIngestible && pawn.Faction.IsPlayer;
		};
		ThingRequest thingReq = ((curSlot.genericDef == null) ? (curSlot.thingDef.Minifiable ? ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing) : ThingRequest.ForDef(curSlot.thingDef)) : ThingRequest.ForGroup(curSlot.genericDef.thingRequestGroup));
		Predicate<Thing> findItem;
		if (curSlot.genericDef != null)
		{
			findItem = (Thing t) => curSlot.genericDef.lambda(t.GetInnerIfMinified().def);
		}
		else
		{
			findItem = (Thing t) => t.GetInnerIfMinified().def == curSlot.thingDef;
		}
		Predicate<Thing> validator = (Thing t) => findItem(t) && !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1) && !isFoodInPrison(t) && AllowedByBiocode(t, pawn) && AllowedByFoodRestriction(t, pawn);
		curThing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, thingReq, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.None), 20f, validator);
		if (curThing != null)
		{
			curPriority = ItemPriority.Proximity;
			return;
		}
		curThing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, thingReq, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.None), 80f, validator);
		if (curThing == null && pawn.Map != null)
		{
			List<Pawn> list = pawn.Map.mapPawns.AllPawns.Where((Pawn p) => p.inventory?.innerContainer?.InnerListForReading?.Any() == true && ((p.RaceProps.packAnimal && p.Faction == pawn.Faction) || (p.IsPrisoner && p.HostFaction == pawn.Faction)) && pawn.CanReserveAndReach(p, PathEndMode.ClosestTouch, Danger.Deadly, int.MaxValue, 0)).ToList();
			foreach (Pawn item in list)
			{
				Thing thing = item.inventory.innerContainer.FirstOrDefault((Thing t) => findItem(t));
				if (thing != null)
				{
					curThing = thing;
					curCarrier = item;
					break;
				}
			}
		}
		if (curThing != null)
		{
			if (!curThing.def.IsNutritionGivingIngestible && (float)findCount / (float)curSlot.count >= 0.5f)
			{
				curPriority = ItemPriority.LowStock;
			}
			else
			{
				curPriority = ItemPriority.Low;
			}
		}
	}

	private static bool AllowedByBiocode(Thing thing, Pawn pawn)
	{
		CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
		return compBiocodable == null || !compBiocodable.Biocoded || compBiocodable.CodedPawn == pawn;
	}

	private static bool AllowedByFoodRestriction(Thing thing, Pawn pawn)
	{
		if (thing != null && thing.def.IsNutritionGivingIngestible)
		{
			return pawn.foodRestriction.GetCurrentRespectedRestriction(pawn)?.Allows(thing) ?? true;
		}
		return true;
	}

	public static Job GetUpdateLoadoutJob(Pawn pawn)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null)
		{
			return null;
		}
		if (pawn.equipment?.Primary is WeaponPlatform platform)
		{
			platform.TrySyncPlatformLoadout(pawn);
		}
		Loadout loadout = pawn.GetLoadout();
		if (loadout != null)
		{
			if (pawn.GetExcessEquipment(out var dropEquipment) && pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var resultingEq, pawn.Position, forbid: false))
			{
				if (resultingEq != null)
				{
					return HaulAIUtility.HaulToStorageJob(pawn, resultingEq);
				}
				Log.Error(string.Concat(pawn, " tried dropping ", dropEquipment, " from loadout but resulting thing is null"));
			}
			if (pawn.GetExcessThing(out var dropThing, out var dropCount) && compInventory.container.TryDrop(dropThing, pawn.Position, pawn.Map, ThingPlaceMode.Near, dropCount, out var resultingThing))
			{
				if (resultingThing != null)
				{
					return HaulAIUtility.HaulToStorageJob(pawn, resultingThing);
				}
				Log.Error(string.Concat(pawn, " tried dropping ", dropThing, " from loadout but resulting thing is null"));
			}
			bool flag = false;
			GetPrioritySlot(pawn, out var _, out var closestThing, out var count, out var carriedBy);
			if (closestThing != null)
			{
				if (closestThing.TryGetComp<CompEquippable>() != null && (pawn.story == null || !pawn.WorkTagIsDisabled(WorkTags.Violent)) && pawn.health != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !pawn.IsItemQuestLocked(pawn.equipment?.Primary) && (pawn.equipment == null || pawn.equipment.Primary == null || !loadout.Slots.Any((LoadoutSlot s) => s.thingDef == pawn.equipment.Primary.def || (s.genericDef != null && s.countType == LoadoutCountType.pickupDrop && s.genericDef.lambda(pawn.equipment.Primary.def)))))
				{
					flag = true;
				}
				if (carriedBy == null)
				{
					if (flag)
					{
						return JobMaker.MakeJob(JobDefOf.Equip, closestThing);
					}
					Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, closestThing);
					job.count = Mathf.Min(closestThing.stackCount, count);
					job.MakeDriver(pawn);
					return job;
				}
				Job job2 = JobMaker.MakeJob(CE_JobDefOf.TakeFromOther, closestThing, carriedBy, flag ? pawn : null);
				job2.MakeDriver(pawn);
				job2.count = (flag ? 1 : Mathf.Min(closestThing.stackCount, count));
				return job2;
			}
		}
		return ((ThinkNode_JobGiver)pawn.thinker?.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>())?.TryGiveJob(pawn);
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		Job updateLoadoutJob = GetUpdateLoadoutJob(pawn);
		_throttle[pawn.thingIDNumber] = ((updateLoadoutJob == null) ? GenTicks.TicksGame : (GenTicks.TicksGame - 1800 - 1));
		return updateLoadoutJob;
	}
}
