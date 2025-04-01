using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_ForceJobOnTargetBase : AbilityExtension_AbilityMod
{
	public JobDef jobDef;

	public StatDef durationMultiplier;

	public FleckDef fleckOnTarget;

	protected void ForceJob(GlobalTargetInfo target, Ability ability)
	{
		if (target.Thing is Pawn pawn)
		{
			Job job = JobMaker.MakeJob(jobDef, ability.pawn);
			float num = 1f;
			if (durationMultiplier != null)
			{
				num = pawn.GetStatValue(durationMultiplier);
			}
			job.expiryInterval = (int)((float)ability.GetDurationForPawn() * num);
			job.mote = MoteMaker.MakeThoughtBubble(pawn, ability.def.iconPath, maintain: true);
			pawn.jobs.StopAll();
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			if (fleckOnTarget != null)
			{
				Ability.MakeStaticFleck(pawn.DrawPos, pawn.Map, fleckOnTarget, 1f, 0f);
			}
		}
	}
}
