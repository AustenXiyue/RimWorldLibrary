using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

internal class JobGiver_HunkerDown : ThinkNode_JobGiver
{
	public override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.Position.Standable(pawn.Map) && !pawn.Position.ContainsStaticFire(pawn.Map))
		{
			return null;
		}
		return JobMaker.MakeJob(CE_JobDefOf.HunkerDown, pawn);
	}
}
