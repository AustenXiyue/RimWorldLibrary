using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_TakeAndEquip : ThinkNode_JobGiver
{
	private enum WorkPriority
	{
		None,
		Unloading,
		LowAmmo,
		Weapon,
		Ammo
	}

	private const float ammoFractionOfNonAmmoInventory = 0.666f;

	private WorkPriority GetPriorityWork(Pawn pawn)
	{
		if (pawn.kindDef.trader)
		{
			return WorkPriority.None;
		}
		bool flag = pawn.equipment != null && pawn.equipment.Primary != null;
		CompAmmoUser compAmmoUser = (flag ? pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : (hasWeaponInInventory(pawn) ? weaponInInventory(pawn) : null));
		if (pawn.Faction.IsPlayer && compAmmoUser != null)
		{
			Loadout loadout = pawn.GetLoadout();
			if (loadout != null && loadout.SlotCount > 0)
			{
				return WorkPriority.None;
			}
		}
		if (!flag)
		{
			if (Unload(pawn))
			{
				return WorkPriority.Unloading;
			}
			if (!hasWeaponInInventory(pawn))
			{
				return WorkPriority.Weapon;
			}
		}
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compAmmoUser != null && compAmmoUser.UseAmmo)
		{
			FloatRange floatRange = new FloatRange(1f, 2f);
			LoadoutPropertiesExtension loadoutPropertiesExtension = (LoadoutPropertiesExtension)(pawn.kindDef.modExtensions?.FirstOrDefault((DefModExtension x) => x is LoadoutPropertiesExtension));
			List<string> weaponTags = pawn.kindDef.weaponTags;
			if (weaponTags != null && weaponTags.Any() && compAmmoUser.parent.def.weaponTags.Any(pawn.kindDef.weaponTags.Contains) && loadoutPropertiesExtension != null && loadoutPropertiesExtension.primaryMagazineCount != FloatRange.Zero)
			{
				floatRange.min = loadoutPropertiesExtension.primaryMagazineCount.min;
				floatRange.max = loadoutPropertiesExtension.primaryMagazineCount.max;
			}
			floatRange.min *= compAmmoUser.MagSize;
			floatRange.max *= compAmmoUser.MagSize;
			int num = 0;
			float num2 = 0f;
			foreach (AmmoLink ammoType in compAmmoUser.Props.ammoSet.ammoTypes)
			{
				int num3 = compInventory.AmmoCountOfDef(ammoType.ammo);
				num += num3;
				num2 += (float)num3 * ammoType.ammo.GetStatValueAbstract(CE_StatDefOf.Bulk);
			}
			float num4 = 0.666f * (compInventory.capacityBulk - compInventory.currentBulk + num2);
			if (num2 < num4)
			{
				if (compAmmoUser.MagSize == 0 || (float)num < floatRange.min)
				{
					return Unload(pawn) ? WorkPriority.Unloading : WorkPriority.LowAmmo;
				}
				if ((float)num < floatRange.max && !PawnUtility.EnemiesAreNearby(pawn, 20, passDoors: true))
				{
					return Unload(pawn) ? WorkPriority.Unloading : WorkPriority.Ammo;
				}
			}
		}
		return WorkPriority.None;
	}

	public override float GetPriority(Pawn pawn)
	{
		if ((!Controller.settings.AutoTakeAmmo && pawn.IsColonist) || !Controller.settings.EnableAmmoSystem)
		{
			return 0f;
		}
		if (pawn.Faction == null)
		{
			return 0f;
		}
		switch (GetPriorityWork(pawn))
		{
		case WorkPriority.Unloading:
			return 9.2f;
		case WorkPriority.LowAmmo:
			return 9f;
		case WorkPriority.Weapon:
			return 8f;
		case WorkPriority.Ammo:
			return 6f;
		case WorkPriority.None:
			return 0f;
		default:
		{
			TimeAssignmentDef timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
			if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
			{
				return 0f;
			}
			if (pawn.health == null || pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return 0f;
			}
			return 0f;
		}
		}
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!Controller.settings.EnableAmmoSystem || !Controller.settings.AutoTakeAmmo)
		{
			return null;
		}
		if (pawn.Faction == null)
		{
			return null;
		}
		if (!pawn.RaceProps.Humanlike || (pawn.story != null && pawn.WorkTagIsDisabled(WorkTags.Violent)))
		{
			return null;
		}
		if (pawn.Faction.IsPlayer && pawn.Drafted)
		{
			return null;
		}
		if (!Rand.MTBEventOccurs(60f, 5f, 30f))
		{
			return null;
		}
		if (!pawn.Faction.IsPlayer && FindBattleWorthyEnemyPawnsCount(pawn.Map, pawn) > 25)
		{
			return null;
		}
		if (pawn.IsPrisoner && (pawn.HostFaction != Faction.OfPlayer || pawn.guest.interactionMode == PrisonerInteractionModeDefOf.Release))
		{
			return null;
		}
		bool flag = pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler);
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		bool hasPrimary = pawn.equipment != null && pawn.equipment.Primary != null;
		CompAmmoUser primaryAmmoUser = (hasPrimary ? pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : null);
		CompAmmoUser compAmmoUser = (hasPrimary ? pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : (hasWeaponInInventory(pawn) ? weaponInInventory(pawn) : null));
		if (compInventory != null)
		{
			if (!pawn.Faction.IsPlayer && hasPrimary && pawn.equipment.Primary.def.IsMeleeWeapon && !flag && (pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6))
			{
				ThingWithComps thingWithComps = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine);
				if (thingWithComps != null)
				{
					compInventory.TrySwitchToWeapon(thingWithComps);
				}
			}
			if (!pawn.Faction.IsPlayer && !hasPrimary)
			{
				if ((pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= pawn.skills.GetSkill(SkillDefOf.Melee).Level || pawn.skills.GetSkill(SkillDefOf.Shooting).Level >= 6) && !flag)
				{
					ThingWithComps thingWithComps2 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine);
					if (thingWithComps2 != null)
					{
						compInventory.TrySwitchToWeapon(thingWithComps2);
					}
				}
				else
				{
					ThingWithComps thingWithComps3 = compInventory.meleeWeaponList.Find((ThingWithComps thing) => thing.def.IsMeleeWeapon);
					if (thingWithComps3 != null)
					{
						compInventory.TrySwitchToWeapon(thingWithComps3);
					}
				}
			}
			WorkPriority priorityWork = GetPriorityWork(pawn);
			if (!pawn.Faction.IsPlayer && primaryAmmoUser != null && priorityWork == WorkPriority.Unloading && compInventory.rangedWeaponList.Count >= 1)
			{
				Thing thing2 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null && thing.def != pawn.equipment.Primary.def);
				if (thing2 != null)
				{
					Thing thing3 = null;
					if (!thing2.TryGetComp<CompAmmoUser>().HasAmmoOrMagazine)
					{
						foreach (AmmoLink link in thing2.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
						{
							if (compInventory.ammoList.Find((Thing thing) => thing.def == link.ammo) == null)
							{
								thing3 = thing2;
								break;
							}
						}
					}
					if (thing3 != null && compInventory.container.TryDrop(thing2, pawn.Position, pawn.Map, ThingPlaceMode.Near, thing2.stackCount, out var resultingThing))
					{
						pawn.jobs.EndCurrentJob(JobCondition.None);
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, resultingThing, 30, checkOverrideOnExpiry: true), JobTag.Misc);
					}
				}
			}
			if (!pawn.Faction.IsPlayer && hasPrimary && compInventory.ammoList.Count > 1 && priorityWork == WorkPriority.Unloading)
			{
				Thing thing4 = null;
				thing4 = ((primaryAmmoUser != null) ? compInventory.ammoList.Find((Thing thing) => !primaryAmmoUser.Props.ammoSet.ammoTypes.Any((AmmoLink a) => a.ammo == thing.def)) : compInventory.ammoList.RandomElement());
				if (thing4 != null)
				{
					Thing thing5 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => hasPrimary && thing.TryGetComp<CompAmmoUser>() != null && thing.def != pawn.equipment.Primary.def);
					Thing resultingThing3;
					if (thing5 != null)
					{
						Thing thing6 = null;
						using (List<AmmoLink>.Enumerator enumerator2 = thing5.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
						{
							if (enumerator2.MoveNext())
							{
								AmmoLink link2 = enumerator2.Current;
								thing6 = compInventory.ammoList.Find((Thing thing) => thing.def == link2.ammo);
							}
						}
						if (thing6 != null && thing6 != thing4 && compInventory.container.TryDrop(thing6, pawn.Position, pawn.Map, ThingPlaceMode.Near, thing6.stackCount, out var _))
						{
							pawn.jobs.EndCurrentJob(JobCondition.None);
							pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, 30, checkOverrideOnExpiry: true), JobTag.Misc);
						}
					}
					else if (compInventory.container.TryDrop(thing4, pawn.Position, pawn.Map, ThingPlaceMode.Near, thing4.stackCount, out resultingThing3))
					{
						pawn.jobs.EndCurrentJob(JobCondition.None);
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, 30, checkOverrideOnExpiry: true), JobTag.Misc);
					}
				}
			}
			if (priorityWork == WorkPriority.Weapon && !hasPrimary)
			{
				ThingWithComps thingWithComps4 = compInventory.rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null);
				if (thingWithComps4 != null)
				{
					Thing thing7 = null;
					using (List<AmmoLink>.Enumerator enumerator3 = thingWithComps4.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes.GetEnumerator())
					{
						if (enumerator3.MoveNext())
						{
							AmmoLink link3 = enumerator3.Current;
							thing7 = compInventory.ammoList.Find((Thing thing) => thing.def == link3.ammo);
						}
					}
					if (thing7 != null)
					{
						compInventory.TrySwitchToWeapon(thingWithComps4);
					}
				}
				if (!pawn.Faction.IsPlayer)
				{
					Predicate<Thing> validatorWS = (Thing w) => w.def.IsWeapon && w.MarketValue > 500f && pawn.CanReserve(w) && pawn.Position.InHorDistOf(w.Position, 25f) && pawn.CanReach(w, PathEndMode.Touch, Danger.Deadly, canBashDoors: true) && (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == Faction.OfPlayer || !pawn.Map.areaManager.Home[w.Position]);
					List<Thing> list = (from w in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
						where validatorWS(w)
						orderby w.MarketValue - (float)w.Position.DistanceToSquared(pawn.Position) * 2f descending
						select w).ToList();
					List<Thing> list2 = list.Where((Thing w) => w.def.IsRangedWeapon).ToList();
					if (!list2.NullOrEmpty())
					{
						foreach (Thing item in list2)
						{
							if (item.TryGetComp<CompAmmoUser>() == null)
							{
								int count = 0;
								if (compInventory.CanFitInInventory(item, out count))
								{
									return JobMaker.MakeJob(JobDefOf.Equip, item);
								}
								continue;
							}
							List<ThingDef> thingDefAmmoList = ((IEnumerable<AmmoLink>)item.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes).Select((Func<AmmoLink, ThingDef>)((AmmoLink g) => g.ammo)).ToList();
							Predicate<Thing> validatorA = (Thing t) => t.def.category == ThingCategory.Item && t is AmmoThing && pawn.CanReserve(t) && pawn.Position.InHorDistOf(t.Position, 25f) && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, canBashDoors: true) && (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == Faction.OfPlayer || !pawn.Map.areaManager.Home[t.Position]);
							List<Thing> list3 = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
								where validatorA(t)
								select t).ToList();
							if (list3.Count <= 0 || thingDefAmmoList.Count <= 0)
							{
								continue;
							}
							int desiredStackSize = item.TryGetComp<CompAmmoUser>().MagSize * 2;
							Thing thing8 = list3.FirstOrDefault((Thing x) => thingDefAmmoList.Contains(x.def) && x.stackCount > desiredStackSize);
							if (thing8 != null)
							{
								int count2 = 0;
								if (compInventory.CanFitInInventory(item, out count2))
								{
									return JobMaker.MakeJob(JobDefOf.Equip, item);
								}
							}
						}
					}
					if (list != null && list.Count > 0)
					{
						Thing thing9 = list.FirstOrDefault((Thing w) => !w.def.IsRangedWeapon && w.def.IsMeleeWeapon);
						if (thing9 != null)
						{
							return JobMaker.MakeJob(JobDefOf.Equip, thing9);
						}
					}
				}
			}
			if ((priorityWork == WorkPriority.Ammo || priorityWork == WorkPriority.LowAmmo) && compAmmoUser != null)
			{
				List<ThingDef> list4 = compAmmoUser.Props.ammoSet.ammoTypes.Cast<AmmoLink>().Select((Func<AmmoLink, ThingDef>)((AmmoLink g) => g.ammo)).ToList();
				if (list4.Count > 0)
				{
					Predicate<Thing> validator = (Thing t) => t is AmmoThing && pawn.CanReserve(t) && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, canBashDoors: true) && ((pawn.Faction.IsPlayer && !t.IsForbidden(pawn)) || (!pawn.Faction.IsPlayer && pawn.Position.InHorDistOf(t.Position, 35f))) && (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.Faction == Faction.OfPlayer || !pawn.Map.areaManager.Home[t.Position]);
					List<Thing> list5 = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
						where validator(t)
						select t).ToList();
					foreach (Thing item2 in list5)
					{
						foreach (ThingDef item3 in list4)
						{
							if (item3 != item2.def)
							{
								continue;
							}
							float num = item2.GetStatValue(CE_StatDefOf.Bulk) * (float)item2.stackCount;
							if (!(num > 0.5f))
							{
								continue;
							}
							if (pawn.Faction.IsPlayer)
							{
								int num2 = 0;
								num2 = ((priorityWork != WorkPriority.LowAmmo) ? 30 : 70);
								Thing thing10 = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(item2.def), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.None), num2, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x));
								if (thing10 != null)
								{
									int count3 = 0;
									if (compInventory.CanFitInInventory(item2, out count3))
									{
										Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
										int num3 = ((compAmmoUser.MagSizeOverride > 0) ? (compAmmoUser.MagSizeOverride * 4) : (compAmmoUser.MagSize * 4));
										job.count = ((num3 > 0) ? Mathf.Min(count3, num3) : count3);
										return job;
									}
								}
							}
							else
							{
								int count4 = 0;
								if (compInventory.CanFitInInventory(item2, out count4))
								{
									Job job2 = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
									job2.count = Mathf.RoundToInt((float)count4 * 0.8f);
									return job2;
								}
							}
						}
					}
				}
			}
			return null;
		}
		return null;
	}

	private static bool hasWeaponInInventory(Pawn pawn)
	{
		Thing thing2 = pawn.TryGetComp<CompInventory>().rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null);
		if (thing2 != null)
		{
			return true;
		}
		return false;
	}

	private static CompAmmoUser weaponInInventory(Pawn pawn)
	{
		return pawn.TryGetComp<CompInventory>().rangedWeaponList.Find((ThingWithComps thing) => thing.TryGetComp<CompAmmoUser>() != null).TryGetComp<CompAmmoUser>();
	}

	public static int FindBattleWorthyEnemyPawnsCount(Map map, Pawn pawn)
	{
		if (pawn == null || pawn.Faction == null)
		{
			return 0;
		}
		return (from p in map.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction)
			where p.Faction != Faction.OfPlayer && !p.Downed
			select p)?.Count() ?? 0;
	}

	private static bool Unload(Pawn pawn)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null && !pawn.Faction.IsPlayer && pawn.CurJob != null && pawn.CurJob.def != JobDefOf.Steal && (compInventory.capacityWeight - compInventory.currentWeight < 3f || compInventory.capacityBulk - compInventory.currentBulk < 4f))
		{
			return true;
		}
		return false;
	}

	private static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
	{
		if (!pawn.CanReserve(blocker))
		{
			return JobMaker.MakeJob(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10), 100, checkOverrideOnExpiry: true);
		}
		Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, blocker);
		job.ignoreDesignations = true;
		job.expiryInterval = 100;
		job.checkOverrideOnExpire = true;
		return job;
	}
}
