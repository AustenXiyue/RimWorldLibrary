using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SlaughterRandomAnimal : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.MentalState is MentalState_SlaughterThing { SlaughteredRecently: not false })
		{
			return null;
		}
		Pawn pawn2 = SlaughtererMentalStateUtility.FindAnimal(pawn);
		if (pawn2 == null || !pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Slaughter, pawn2);
		job.ignoreDesignations = true;
		return job;
	}
}
