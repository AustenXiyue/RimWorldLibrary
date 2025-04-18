using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIResurrectTarget : ThinkNode_JobGiver
{
	private AbilityDef ability;

	private int expiryInterval = 500;

	private int maxRegions = 50;

	private static HashSet<Corpse> tmpReservedCorpses = new HashSet<Corpse>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		using (new ProfilerBlock("JobGiver_AIResurrectTarget.TryGiveJob"))
		{
			Ability ability = pawn.abilities.GetAbility(this.ability);
			if (ability == null || !ability.CanCast)
			{
				return null;
			}
			if (!pawn.Spawned)
			{
				return null;
			}
			UpdateResurrectTarget(pawn);
			if (pawn.mindState.resurrectTarget == null)
			{
				return null;
			}
			Job job = ability.GetJob(pawn.mindState.resurrectTarget.corpse, pawn.mindState.resurrectTarget.castPosition);
			job.expiryInterval = expiryInterval;
			return job;
		}
	}

	private static HashSet<Corpse> ReservedCorpsesForResurrection(Map map, Faction faction)
	{
		using (new ProfilerBlock("ReservedCorpsesForResurrection"))
		{
			tmpReservedCorpses.Clear();
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				Corpse item;
				if ((item = list[i].mindState?.resurrectTarget?.corpse) != null)
				{
					tmpReservedCorpses.Add(item);
				}
			}
			return tmpReservedCorpses;
		}
	}

	private void UpdateResurrectTarget(Pawn pawn)
	{
		pawn.mindState.resurrectTarget = null;
		Ability ability = pawn.abilities.GetAbility(this.ability);
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
		list.SortBy((Thing c) => c.Position.DistanceToSquared(pawn.Position));
		HashSet<Corpse> hashSet = ReservedCorpsesForResurrection(pawn.Map, pawn.Faction);
		for (int i = 0; i < list.Count; i++)
		{
			Corpse corpse = (Corpse)list[i];
			if (hashSet.Contains(corpse) || !ShouldResurrectCorpse(corpse, pawn) || !ability.CanApplyOn(new LocalTargetInfo(corpse)))
			{
				continue;
			}
			using (new ProfilerBlock("TryFindCastPosition"))
			{
				CastPositionRequest newReq = default(CastPositionRequest);
				newReq.caster = pawn;
				newReq.target = corpse;
				newReq.verb = ability.verb;
				newReq.maxRangeFromTarget = ability.verb.verbProps.range;
				newReq.wantCoverFromTarget = false;
				newReq.maxRegions = maxRegions;
				if (CastPositionFinder.TryFindCastPosition(newReq, out var dest))
				{
					pawn.mindState.resurrectTarget = new ResurrectCorpseData(corpse, dest);
					break;
				}
			}
		}
	}

	public static bool ShouldResurrectCorpse(Corpse corpse, Pawn pawn)
	{
		if (corpse == null || !corpse.Spawned || corpse.Map != pawn.Map || corpse.InnerPawn.Faction != pawn.Faction || !pawn.CanReserve(corpse) || corpse.InnerPawn.kindDef.abilities.NotNullAndContains(AbilityDefOf.ResurrectionMech))
		{
			return false;
		}
		return true;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIResurrectTarget obj = (JobGiver_AIResurrectTarget)base.DeepCopy(resolve);
		obj.ability = ability;
		obj.expiryInterval = expiryInterval;
		obj.maxRegions = maxRegions;
		return obj;
	}
}
