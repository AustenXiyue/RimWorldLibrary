using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_Hunt : JobDriver
{
	private const TargetIndex VictimInd = TargetIndex.A;

	private const TargetIndex StoreCellInd = TargetIndex.B;

	private const int MaxHuntTicks = 5000;

	private int jobStartTick = -1;

	public Pawn Victim
	{
		get
		{
			Corpse corpse = Corpse;
			return (corpse != null) ? corpse.InnerPawn : ((Pawn)job.GetTarget(TargetIndex.A).Thing);
		}
	}

	private Corpse Corpse => job.GetTarget(TargetIndex.A).Thing as Corpse;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Victim, job);
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(delegate
		{
			if (!job.ignoreDesignations)
			{
				Pawn victim = Victim;
				if (victim != null && !victim.Dead && base.Map.designationManager.DesignationOn(victim, DesignationDefOf.Hunt) == null)
				{
					return true;
				}
			}
			return false;
		});
		yield return Toils_Reserve.Reserve(TargetIndex.A);
		yield return new Toil
		{
			initAction = delegate
			{
				jobStartTick = Find.TickManager.TicksGame;
			}
		};
		yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
		CompAmmoUser comp = ((pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.TryGetComp<CompAmmoUser>() : null);
		Toil startCollectCorpse = StartCollectCorpseToil();
		Toil gotoCastPos = GotoCastPosition(TargetIndex.A, closeIfDowned: true).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
		yield return gotoCastPos;
		Toil moveIfCannotHit = Toils_Jump.JumpIf(gotoCastPos, delegate
		{
			Verb verbToUse = pawn.CurJob.verbToUse;
			float optimalHuntRange = GetOptimalHuntRange(pawn, Victim);
			return pawn.Position.DistanceTo(Victim.Position) > optimalHuntRange || !verbToUse.CanHitTarget(Victim);
		});
		yield return moveIfCannotHit;
		yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
		Toil startExecuteDowned = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse);
		yield return Toils_Jump.JumpIf(startExecuteDowned, () => Victim.Downed && Victim.RaceProps.executionRange <= 2);
		if (pawn?.equipment?.PrimaryEq?.PrimaryVerb?.IsMeleeAttack == true)
		{
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
			{
				pawn.meleeVerbs.TryMeleeAttack(Victim);
			}).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse);
		}
		else
		{
			yield return Toils_Combat.CastVerb(TargetIndex.A, canHitNonTargetPawns: false).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => (Find.TickManager.TicksGame > jobStartTick + 5000 || (comp != null && !comp.HasAndUsesAmmoOrMagazine)) ? true : false);
			yield return Toils_Jump.Jump(moveIfCannotHit);
		}
		yield return Toils_Jump.Jump(moveIfCannotHit);
		yield return startExecuteDowned;
		yield return Toils_General.WaitWith(TargetIndex.A, 180, useProgressBar: true).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse);
		yield return new Toil
		{
			initAction = delegate
			{
				ExecutionUtility.DoExecutionByCut(pawn, Victim);
			},
			defaultCompleteMode = ToilCompleteMode.Instant
		};
		yield return startCollectCorpse;
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
		yield return carryToCell;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref jobStartTick, "jobStartTick", 0);
	}

	public override string GetReport()
	{
		return pawn.CurJob.def.reportString.Replace("TargetA", Victim.LabelShort);
	}

	private Toil GotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false)
	{
		Toil toil = new Toil();
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.CurJob;
			Thing thing = curJob.GetTarget(targetInd).Thing;
			Pawn victim = thing as Pawn;
			CastPositionRequest newReq = default(CastPositionRequest);
			newReq.caster = toil.actor;
			newReq.target = thing;
			newReq.verb = curJob.verbToUse;
			newReq.maxRangeFromTarget = GetOptimalHuntRange(actor, victim);
			newReq.wantCoverFromTarget = false;
			if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
			{
				toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else
			{
				toil.actor.pather.StartPath(dest, PathEndMode.OnCell);
				actor.Map.pawnDestinationReservationManager.Reserve(actor, job, dest);
			}
		};
		toil.FailOnDespawnedOrNull(targetInd);
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	public static float GetOptimalHuntRange(Pawn hunter, Pawn victim)
	{
		Job curJob = hunter.CurJob;
		RaceProperties raceProps = victim.RaceProps;
		if (victim.Downed)
		{
			return Mathf.Min(curJob.verbToUse.verbProps.range, raceProps.executionRange);
		}
		float num = HuntRangePerBodysize(raceProps.baseBodySize, raceProps.executionRange, curJob.verbToUse.verbProps.range);
		if (raceProps.manhunterOnDamageChance > 0f)
		{
			float num2 = Mathf.Clamp01(raceProps.baseBodySize);
			float num3 = Mathf.Clamp01((float)hunter.skills.GetSkill(SkillDefOf.Shooting).Level / 5f);
			float range = curJob.verbToUse.verbProps.range;
			float a = range * num2 * num3;
			return Mathf.Max(a, num);
		}
		return num;
		static float HuntRangePerBodysize(float x, float executionRange, float gunRange)
		{
			return Mathf.Min(Mathf.Clamp(1f + 20f * (1f - Mathf.Exp(-0.65f * x)), executionRange, 20f), gunRange);
		}
	}

	private Toil StartCollectCorpseToil()
	{
		Toil toil = new Toil();
		toil.initAction = delegate
		{
			if (Victim == null)
			{
				toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else
			{
				TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, Victim);
				Corpse corpse = Victim.Corpse;
				if (corpse == null || !pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly))
				{
					pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					corpse.SetForbidden(value: false);
					if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, pawn, base.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell))
					{
						pawn.Reserve(corpse, job);
						pawn.Reserve(foundCell, job);
						job.SetTarget(TargetIndex.B, foundCell);
						job.SetTarget(TargetIndex.A, corpse);
						job.count = 1;
						job.haulMode = HaulMode.ToCellStorage;
					}
					else
					{
						pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			}
		};
		return toil;
	}
}
