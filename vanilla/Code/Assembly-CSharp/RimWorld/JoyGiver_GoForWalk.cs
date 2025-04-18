using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_GoForWalk : JoyGiver
{
	public override Job TryGiveJob(Pawn pawn)
	{
		if (!JoyUtility.EnjoyableOutsideNow(pawn))
		{
			return null;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return null;
		}
		Predicate<IntVec3> cellValidator = (IntVec3 x) => !PawnUtility.KnownDangerAt(x, pawn.Map, pawn) && !x.GetTerrain(pawn.Map).avoidWander && x.Standable(pawn.Map) && !x.Roofed(pawn.Map);
		IntVec3 result;
		Predicate<Region> validator = (Region x) => x.Room.PsychologicallyOutdoors && !x.IsForbiddenEntirely(pawn) && x.TryFindRandomCellInRegionUnforbidden(pawn, cellValidator, out result);
		if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), validator, 100, out var result2))
		{
			return null;
		}
		if (!result2.TryFindRandomCellInRegionUnforbidden(pawn, cellValidator, out var result3))
		{
			return null;
		}
		if (!WalkPathFinder.TryFindWalkPath(pawn, result3, out var result4))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(def.jobDef, result4[0]);
		job.targetQueueA = new List<LocalTargetInfo>();
		for (int i = 1; i < result4.Count; i++)
		{
			job.targetQueueA.Add(result4[i]);
		}
		job.locomotionUrgency = LocomotionUrgency.Walk;
		return job;
	}
}
