using System;
using System.Collections.Generic;
using MoreBetterDeepDrill.Comp;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreBetterDeepDrill.Jobs;

public class MBDD_JobDriver_OperateDeepDrill : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		throw new NotImplementedException();
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);
		this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
		this.FailOn(() => !job.targetA.Thing.TryGetComp<MBDD_CompDeepDrill>().CanDrillNow);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil work = ToilMaker.MakeToil("MakeNewToils");
		work.tickAction = delegate
		{
			pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
			Pawn actor = work.actor;
			((Building)actor.CurJob.targetA.Thing).GetComp<MBDD_CompDeepDrill>().DrillJoinWork(actor);
			if (actor.skills != null)
			{
				actor.skills.Learn(SkillDefOf.Mining, 0.065f);
			}
		};
		work.AddFinishAction(delegate
		{
			Pawn actor2 = work.actor;
			((Building)actor2.CurJob.targetA.Thing).GetComp<MBDD_CompDeepDrill>().DrillLeaveWork(actor2);
		});
		work.defaultCompleteMode = ToilCompleteMode.Never;
		work.WithEffect(EffecterDefOf.Drill, TargetIndex.A, null);
		work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		work.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		work.activeSkill = () => SkillDefOf.Mining;
		yield return work;
	}
}
