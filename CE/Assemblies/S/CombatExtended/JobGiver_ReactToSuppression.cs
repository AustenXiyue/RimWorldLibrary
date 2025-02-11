using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_ReactToSuppression : ThinkNode_JobGiver
{
	public override Job TryGiveJob(Pawn pawn)
	{
		CompSuppressable compSuppressable = pawn.TryGetComp<CompSuppressable>();
		if (compSuppressable == null)
		{
			return null;
		}
		Job runForCoverJob = SuppressionUtility.GetRunForCoverJob(pawn);
		if (runForCoverJob == null && compSuppressable.IsHunkering)
		{
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Hunkering, pawn, OpportunityType.Critical);
			runForCoverJob = JobMaker.MakeJob(CE_JobDefOf.HunkerDown, pawn);
			runForCoverJob.checkOverrideOnExpire = true;
			return runForCoverJob;
		}
		LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_SuppressionReaction, pawn, OpportunityType.Critical);
		return runForCoverJob;
	}
}
