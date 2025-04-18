using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_Scarify : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord == null || !(lord.LordJob is LordJob_Ritual_Mutilation lordJob_Ritual_Mutilation))
		{
			return null;
		}
		Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
		if (lordJob_Ritual_Mutilation.mutilatedPawns.Contains(pawn2) || !pawn.CanReserveAndReach(pawn2, PathEndMode.ClosestTouch, Danger.None) || !JobDriver_Scarify.AvailableOnNow(pawn2))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.Scarify, pawn2, pawn.mindState.duty.focus);
	}
}
