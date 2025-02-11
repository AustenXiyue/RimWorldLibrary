using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class Verb_LaunchProjectileStaticCE : Verb_LaunchProjectileCE
{
	public override bool MultiSelect => true;

	public override Texture2D UIIcon => TexCommand.Attack;

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		return base.ValidateTarget(target, showMessages) && ReloadableUtility.CanUseConsideringQueuedJobs(CasterPawn, base.EquipmentSource);
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
		job.verbToUse = this;
		CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}
}
