using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Dance : ThinkNode_JobGiver
{
	public IntRange ticksRange = new IntRange(300, 600);

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_Dance obj = (JobGiver_Dance)base.DeepCopy(resolve);
		obj.ticksRange = ticksRange;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Dance);
		job.expiryInterval = ticksRange.RandomInRange;
		return job;
	}
}
