using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class WorkGiver_ModifyWeapon : WorkGiver_Scanner
{
	private const int SCAN_COOLDOWN = 300;

	private const int MAX_SCAN_RADIUS = 150;

	private static Dictionary<int, int> _throttleByPawn;

	static WorkGiver_ModifyWeapon()
	{
		_throttleByPawn = new Dictionary<int, int>();
		CacheClearComponent.AddClearCacheAction(delegate
		{
			_throttleByPawn.Clear();
		});
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		if (!(t.Thing is WeaponPlatform weaponPlatform))
		{
			return 0f;
		}
		if (weaponPlatform == pawn.equipment?.Primary)
		{
			return 35f;
		}
		return 15f + 15f * Mathf.Clamp01(1f - pawn.Position.DistanceTo(t.Cell) / 150f);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (_throttleByPawn.TryGetValue(pawn.thingIDNumber, out var value) && GenTicks.TicksGame - value < 300)
		{
			return true;
		}
		_throttleByPawn[pawn.thingIDNumber] = GenTicks.TicksGame;
		if (!pawn.RaceProps.Humanlike)
		{
			return true;
		}
		if ((!pawn.IsColonist && !pawn.IsSlaveOfColony) || pawn.IsPrisoner)
		{
			return true;
		}
		if (ShouldSkipMap(pawn.Map))
		{
			return true;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Crafting))
		{
			return true;
		}
		if (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) != true)
		{
			return true;
		}
		return base.ShouldSkip(pawn, forced);
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		ThingWithComps thingWithComps = pawn.equipment?.Primary;
		WeaponPlatform primary = thingWithComps as WeaponPlatform;
		if (primary != null && !primary.ConfigApplied)
		{
			yield return primary;
		}
		foreach (ThingWithComps item in from t in pawn.Position.WeaponsInRange(pawn.Map, 150f)
			where ShouldYield(t, pawn)
			select t)
		{
			yield return item;
		}
		static bool ShouldYield(Thing thing, Pawn pawn)
		{
			return thing is WeaponPlatform { ConfigApplied: false } weaponPlatform && !weaponPlatform.IsForbidden(((Thing)pawn).factionInt) && pawn.CanReserve(weaponPlatform, 1, 1) && pawn.CanReach(weaponPlatform, PathEndMode.OnCell, Danger.Deadly);
		}
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return TryGetModifyWeaponJob(pawn, t as WeaponPlatform);
	}

	public static Job TryGetModifyWeaponJob(Pawn pawn, WeaponPlatform platform)
	{
		if (!platform.Spawned && pawn.equipment?.Primary != platform)
		{
			return null;
		}
		if (platform.Spawned && (!pawn.CanReserve(platform, 1, 1) || !pawn.CanReach(platform, PathEndMode.OnCell, Danger.Deadly)))
		{
			return null;
		}
		Building building = pawn.Map.listerBuildings.AllBuildingsColonistOfDef(CE_ThingDefOf.GunsmithingBench).FirstOrFallback((Building b) => pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.Deadly, 1, 1));
		if (building == null)
		{
			JobFailReason.Is("CE_MissinGunsmithingBench".Translate());
			return null;
		}
		List<ThingCount> chosenIngThings = new List<ThingCount>();
		IBillGiver billGiver = building as IBillGiver;
		AttachmentDef attachmentDef;
		if (platform.RemovalList.Count != 0)
		{
			attachmentDef = platform.RemovalList.RandomElement();
		}
		else if (!TryFindTargetAndIngredients(pawn, building, platform, out attachmentDef, out chosenIngThings))
		{
			JobFailReason.Is("MissingMaterials".Translate());
			return null;
		}
		Job haulOffJob = null;
		Job job = TryCreateModifyJob(pawn, platform, attachmentDef, building, billGiver, chosenIngThings, out haulOffJob);
		if (haulOffJob != null)
		{
			pawn.jobs.jobQueue.EnqueueFirst(job, null);
			return haulOffJob;
		}
		return job;
	}

	private static bool ShouldSkipMap(Map map)
	{
		return map != null && map.listerThings.ThingsOfDef(CE_ThingDefOf.GunsmithingBench).Count == 0;
	}

	private static Job TryCreateModifyJob(Pawn pawn, WeaponPlatform weapon, AttachmentDef attachmentDef, Thing bench, IBillGiver billGiver, List<ThingCount> chosenIngThings, out Job haulOffJob)
	{
		haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, weapon);
		JobCE jobCE = new JobCE();
		jobCE.def = CE_JobDefOf.ModifyWeapon;
		jobCE.targetA = bench;
		jobCE.targetThingDefs.Add(attachmentDef);
		jobCE.targetQueueA = new List<LocalTargetInfo> { weapon };
		jobCE.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
		jobCE.countQueue = new List<int>(chosenIngThings.Count);
		if (weapon.Spawned && !(bench as IBillGiver).IngredientStackCells.Contains(weapon.Position))
		{
			jobCE.targetQueueB.Add(weapon);
			jobCE.countQueue.Add(1);
		}
		for (int i = 0; i < chosenIngThings.Count; i++)
		{
			jobCE.targetQueueB.Add(chosenIngThings[i].Thing);
			jobCE.countQueue.Add(chosenIngThings[i].Count);
		}
		jobCE.haulMode = HaulMode.ToCellNonStorage;
		return jobCE;
	}

	private static bool TryFindTargetAndIngredients(Pawn pawn, Building bench, WeaponPlatform platform, out AttachmentDef attachmentDef, out List<ThingCount> chosenIngThings)
	{
		chosenIngThings = new List<ThingCount>();
		attachmentDef = null;
		foreach (AttachmentDef addition in platform.AdditionList)
		{
			chosenIngThings.Clear();
			if (TryFindIngredientsFor(addition, pawn, bench, chosenIngThings))
			{
				attachmentDef = addition;
				return true;
			}
		}
		return false;
	}

	private static bool TryFindIngredientsFor(AttachmentDef attachmentDef, Pawn pawn, Building bench, List<ThingCount> chosenIngThings)
	{
		int remainingTotalCost = 0;
		Dictionary<int, int> remainingCost = new Dictionary<int, int>();
		foreach (ThingDefCountClass cost in attachmentDef.costList)
		{
			remainingTotalCost += cost.count;
			remainingCost[cost.thingDef.index] = cost.count;
		}
		TraverseParms traverseParams = TraverseParms.For(pawn);
		Region region = bench.Position.GetRegion(pawn.Map);
		RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
		RegionProcessor regionProcessor = delegate(Region r)
		{
			List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
			for (int i = 0; i < list.Count; i++)
			{
				if (remainingTotalCost <= 0)
				{
					break;
				}
				Thing thing = list[i];
				if (remainingCost.TryGetValue(thing.def.index, out var value) && value > 0 && !chosenIngThings.Any((ThingCount t) => t.Thing.thingIDNumber == thing.thingIDNumber) && !thing.IsForbidden(pawn) && pawn.CanReserve(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.InteractionCell, pawn))
				{
					int num = Math.Min(thing.stackCount, value);
					chosenIngThings.Add(new ThingCount(thing, num));
					remainingCost[thing.def.index] = value - num;
					remainingTotalCost -= num;
				}
			}
			return remainingTotalCost == 0;
		};
		RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 200);
		if (remainingTotalCost != 0)
		{
			chosenIngThings.Clear();
			return false;
		}
		return true;
	}
}
