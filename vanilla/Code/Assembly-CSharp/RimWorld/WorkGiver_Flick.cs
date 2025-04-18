using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Flick : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.designationsByDef[DesignationDefOf.Flick])
		{
			yield return item.target.Thing;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Flick);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Flick) == null)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.Flick, t);
	}
}
