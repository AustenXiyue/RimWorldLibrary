using System.Collections.Generic;
using System.Linq;
using CombatExtended.AI;
using CombatExtended.Compatibility;
using CombatExtended.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public static class SuppressionUtility
{
	private const float maxCoverDist = 10f;

	public const float linearDangerAmountFactor = 0.2f;

	public const float squaredDangerAmountFactor = 0.01f;

	private const float pathCostFactor = 2f;

	private const float obstaclePathCostFactor = 2f;

	private const float distanceFromThreatFactor = 0.5f;

	private static LightingTracker lightingTracker;

	private static DangerTracker dangerTracker;

	public static bool TryRequestHelp(Pawn pawn)
	{
		if (pawn != null)
		{
			return false;
		}
		Map map = pawn.Map;
		float currentSuppression = pawn.TryGetComp<CompSuppressable>().CurrentSuppression;
		ThingWithComps grenade = null;
		foreach (Pawn item in pawn.Position.PawnsInRange(map, 8f))
		{
			if (item.Faction != pawn.Faction || item.jobs?.curDriver is IJobDriver_Tactical)
			{
				continue;
			}
			CompInventory compInventory = item.TryGetComp<CompInventory>();
			if (compInventory != null && compInventory.TryFindSmokeWeapon(out grenade))
			{
				CompSuppressable compSuppressable = item.TryGetComp<CompSuppressable>();
				if (compSuppressable != null && !compSuppressable.isSuppressed && !(compSuppressable.CurrentSuppression > currentSuppression) && GenSight.LineOfSight(pawn.Position, item.Position, map))
				{
					Job job = JobMaker.MakeJob(CE_JobDefOf.OpportunisticAttack, grenade, pawn.Position);
					job.maxNumStaticAttacks = 1;
					item.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
					return true;
				}
			}
		}
		return false;
	}

	public static Job GetRunForCoverJob(Pawn pawn)
	{
		CompSuppressable compSuppressable = pawn.TryGetComp<CompSuppressable>();
		if (compSuppressable == null)
		{
			return null;
		}
		if (!GetCoverPositionFrom(pawn, compSuppressable.SuppressorLoc, 10f, out var coverPosition))
		{
			return null;
		}
		if (pawn.Position.Equals(coverPosition))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(CE_JobDefOf.RunForCover, coverPosition);
		job.locomotionUrgency = LocomotionUrgency.Sprint;
		job.playerForced = true;
		return job;
	}

	private static bool GetCoverPositionFrom(Pawn pawn, IntVec3 fromPosition, float maxDist, out IntVec3 coverPosition)
	{
		List<IntVec3> source = new List<IntVec3>(GenRadial.RadialCellsAround(pawn.Position, maxDist, useCenter: true));
		IntVec3 intVec = pawn.Position;
		lightingTracker = pawn.Map.GetLightingTracker();
		dangerTracker = pawn.Map.GetDangerTracker();
		float num = GetCellCoverRatingForPawn(pawn, pawn.Position, fromPosition);
		if (num <= 0f)
		{
			Region region = pawn.Position.GetRegion(pawn.Map);
			List<Region> list = region.Neighbors.ToList();
			list.Add(region);
			List<Region> list2 = list.ListFullCopy();
			foreach (Region item in list2)
			{
				list.AddRange(item.Neighbors.Except(list));
			}
			foreach (IntVec3 item2 in source.Where((IntVec3 x) => x.InBounds(pawn.Map)))
			{
				if (item2.InBounds(pawn.Map) && list.Contains(item2.GetRegion(pawn.Map)))
				{
					float cellCoverRatingForPawn = GetCellCoverRatingForPawn(pawn, item2, fromPosition);
					if (cellCoverRatingForPawn > num)
					{
						num = cellCoverRatingForPawn;
						intVec = item2;
					}
				}
			}
		}
		coverPosition = intVec;
		lightingTracker = null;
		return num >= -999f && pawn.Position != coverPosition;
	}

	private static float GetCellCoverRatingForPawn(Pawn pawn, IntVec3 cell, IntVec3 shooterPos)
	{
		if (!cell.IsValid || !cell.Standable(pawn.Map) || !pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly) || cell.ContainsStaticFire(pawn.Map))
		{
			return -1000f;
		}
		foreach (IntVec3 item in GenSight.PointsOnLineOfSight(pawn.Position, cell))
		{
			if (!item.Walkable(pawn.Map))
			{
				return -1000f;
			}
		}
		float num = 0f;
		float num2 = 1f;
		float y = CE_Utility.GetCollisionBodyFactors(pawn).y;
		float num3 = y * 0.14999998f + 0.01f;
		float num4 = y * 0.45f + num3;
		float num5 = num4;
		Vector3 normalized = (shooterPos - cell).ToVector3().normalized;
		if (!GenSight.LineOfSight(shooterPos, cell, pawn.Map))
		{
			num = 20f;
		}
		else
		{
			IntVec3 c = (cell.ToVector3Shifted() + normalized).ToIntVec3();
			Thing cover = c.GetCover(pawn.Map);
			if (cover == null || cover is Plant || cover is Gas)
			{
				c = (cell.ToVector3Shifted() + normalized.ToVec3Gridified()).ToIntVec3();
				cover = c.GetCover(pawn.Map);
			}
			if (cover != null && !(cover is Plant) && !(cover is Gas))
			{
				num5 = Mathf.Clamp(cover.def.fillPercent + num3, num4, y);
				float a = 1f - (num5 - cover.def.fillPercent) / y;
				num = Mathf.Min(a, 1f) * 10f;
			}
		}
		foreach (IntVec3 item2 in pawn.Map.PartialLineOfSights(cell, shooterPos))
		{
			Thing cover2 = item2.GetCover(pawn.Map);
			if (cover2 == null)
			{
				continue;
			}
			if (!(cover2 is Gas) && !(cover2 is Plant))
			{
				float num6 = cover2.def.fillPercent / num5;
				if (num6 * 10f > num)
				{
					num = Mathf.Min(num6, 1f) * 10f;
				}
			}
			else
			{
				num2 *= 1f - GetCoverRating(cover2);
			}
		}
		num += 10f - num2 * 10f;
		num += (float)CalculateShieldRating(cell, pawn);
		float num7 = dangerTracker.DangerAt(cell) * 300f;
		num -= (num7 * 0.2f + num7 * num7 * 0.01f) / (num + 1f);
		if (!pawn.Position.Equals(cell))
		{
			num -= lightingTracker.CombatGlowAtFor(shooterPos, cell) * 5f;
			float num8 = (pawn.Position - cell).LengthHorizontal;
			foreach (IntVec3 item3 in GenSight.PointsOnLineOfSight(pawn.Position, cell))
			{
				if (!item3.Standable(pawn.Map) || item3.GetDoor(pawn.Map) != null)
				{
					num8 *= 2f;
					break;
				}
			}
			num -= num8 * 2f;
			num += ((cell - shooterPos).LengthHorizontal - (pawn.Position - shooterPos).LengthHorizontal) * 0.5f;
		}
		if (Controller.settings.DebugDisplayCellCoverRating)
		{
			pawn.Map.debugDrawer.FlashCell(cell, num, $"{num}");
		}
		return num;
	}

	private static int CalculateShieldRating(IntVec3 cell, Pawn pawn)
	{
		int num = 0;
		foreach (IEnumerable<IntVec3> item in InterceptorZonesFor(pawn))
		{
			foreach (IntVec3 item2 in item)
			{
				if (item2 == cell)
				{
					if (!IsOccupiedByEnemies(item, pawn))
					{
						num += 15;
					}
					break;
				}
			}
		}
		return num;
	}

	public static IEnumerable<IEnumerable<IntVec3>> InterceptorZonesFor(Pawn pawn)
	{
		foreach (Thing interceptor in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor))
		{
			CompProjectileInterceptor comp = interceptor.TryGetComp<CompProjectileInterceptor>();
			if (comp.Active && (comp.Props.interceptNonHostileProjectiles || !interceptor.HostileTo(pawn)))
			{
				yield return GenRadial.RadialCellsAround(interceptor.Position, comp.Props.radius, useCenter: true);
			}
		}
		foreach (IEnumerable<IntVec3> item in BlockerRegistry.ShieldZonesCallback(pawn))
		{
			yield return item;
		}
	}

	private static bool IsOccupiedByEnemies(IEnumerable<IntVec3> cells, Pawn pawn)
	{
		foreach (IntVec3 cell in cells)
		{
			List<Thing> list = pawn.Map.thingGrid.ThingsListAt(cell);
			foreach (Thing item in list)
			{
				if (item.HostileTo(pawn))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static float GetCoverRating(Thing cover)
	{
		if (cover == null)
		{
			return 0f;
		}
		if (cover is Gas)
		{
			return 0.2f;
		}
		if (cover.def.category == ThingCategory.Plant)
		{
			return 0.45f * cover.def.fillPercent;
		}
		return 0f;
	}

	public static bool TryGetSmokeScreeningJob(Pawn pawn, IntVec3 suppressorLoc, out Job job)
	{
		job = null;
		ThingWithComps grenade = null;
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null || !compInventory.TryFindSmokeWeapon(out grenade))
		{
			return false;
		}
		int num = 5;
		IntVec3 intVec = pawn.Position;
		foreach (IntVec3 item in pawn.PartialLineOfSights(suppressorLoc))
		{
			if (item.DistanceTo(pawn.Position) >= (float)num)
			{
				break;
			}
			intVec = item;
		}
		if (!intVec.IsValid)
		{
			intVec = pawn.Position;
		}
		job = JobMaker.MakeJob(CE_JobDefOf.OpportunisticAttack, grenade, intVec);
		job.maxNumStaticAttacks = 1;
		return true;
	}

	public static List<MentalStateDef> GetPossibleBreaks(Pawn pawn)
	{
		List<MentalStateDef> list = new List<MentalStateDef>();
		TraitSet traits = pawn.story.traits;
		if (!traits.HasTrait(TraitDefOf.Bloodlust) && traits.DegreeOfTrait(CE_TraitDefOf.Bravery) <= 1)
		{
			list.Add(pawn.IsColonist ? MentalStateDefOf.Wander_OwnRoom : MentalStateDefOf.PanicFlee);
			list.Add(CE_MentalStateDefOf.ShellShock);
		}
		if (!pawn.WorkTagIsDisabled(WorkTags.Violent) && traits.DegreeOfTrait(CE_TraitDefOf.Bravery) >= 0)
		{
			list.Add(CE_MentalStateDefOf.CombatFrenzy);
		}
		return list;
	}
}
