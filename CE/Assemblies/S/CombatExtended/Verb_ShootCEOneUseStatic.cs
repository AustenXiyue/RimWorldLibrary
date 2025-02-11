using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class Verb_ShootCEOneUseStatic : Verb_ShootCEOneUse
{
	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStaticReserve, target);
		job.verbToUse = this;
		CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}
}
