using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ConstructFinishFrame : JobDriver
{
	private const int JobEndInterval = 5000;

	private Frame Frame => (Frame)job.GetTarget(TargetIndex.A).Thing;

	private bool IsBuildingAttachment
	{
		get
		{
			ThingDef thingDef = GenConstruct.BuiltDefOf(Frame.def) as ThingDef;
			if (thingDef?.building != null)
			{
				return thingDef.building.isAttachment;
			}
			return false;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		PathEndMode pathEndMode = (IsBuildingAttachment ? PathEndMode.OnCell : PathEndMode.Touch);
		yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		Toil build = ToilMaker.MakeToil("MakeNewToils");
		build.initAction = delegate
		{
			GenClamor.DoClamor(build.actor, 15f, ClamorDefOf.Construction);
		};
		build.tickAction = delegate
		{
			Pawn actor = build.actor;
			Frame frame = Frame;
			if (frame.resourceContainer.Count > 0 && actor.skills != null)
			{
				actor.skills.Learn(SkillDefOf.Construction, 0.25f);
			}
			if (IsBuildingAttachment)
			{
				actor.rotationTracker.FaceTarget(GenConstruct.GetWallAttachedTo(frame));
			}
			else
			{
				actor.rotationTracker.FaceTarget(frame);
			}
			float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
			if (frame.Stuff != null)
			{
				num *= frame.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
			}
			float workToBuild = frame.WorkToBuild;
			if (actor.Faction == Faction.OfPlayer)
			{
				float statValue = actor.GetStatValue(StatDefOf.ConstructSuccessChance);
				if (!TutorSystem.TutorialMode && Rand.Value < 1f - Mathf.Pow(statValue, num / workToBuild))
				{
					frame.FailConstruction(actor);
					ReadyForNextToil();
					return;
				}
			}
			if (frame.def.entityDefToBuild is TerrainDef)
			{
				base.Map.snowGrid.SetDepth(frame.Position, 0f);
			}
			frame.workDone += num;
			if (frame.workDone >= workToBuild)
			{
				frame.CompleteConstruction(actor);
				ReadyForNextToil();
			}
		};
		build.WithEffect(() => ((Frame)build.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).ConstructionEffect, TargetIndex.A, null);
		build.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		build.FailOnCannotTouch(TargetIndex.A, pathEndMode);
		build.FailOn(() => !GenConstruct.CanConstruct(Frame, pawn));
		build.defaultCompleteMode = ToilCompleteMode.Delay;
		build.defaultDuration = 5000;
		build.activeSkill = () => SkillDefOf.Construction;
		build.handlingFacing = true;
		yield return build;
	}
}
