using RimWorld;
using Verse;
using Verse.AI;

namespace Quality_Recasting_Table;

public class CompTargetEffect_QualityRecasting : CompTargetEffect
{
	public CompProperties_QualityRecasting Props => (CompProperties_QualityRecasting)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (user.IsColonistPlayerControlled)
		{
			Job job = JobMaker.MakeJob(RecastingDefOf.DoRecasting, target, parent);
			job.count = 1;
			job.playerForced = true;
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}
}
