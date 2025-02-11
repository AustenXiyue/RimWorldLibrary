using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_SetUpBipod : JobDriver
{
	private const TargetIndex guntosetup = TargetIndex.A;

	private ThingWithComps weapon => base.TargetThingA as ThingWithComps;

	public BipodComp Bipod => weapon.TryGetComp<BipodComp>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job);
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		Pawn Pawn = GetActor();
		float pawnSkill = Pawn.GetStatValue(StatDefOf.ShootingAccuracyPawn) * 0.5f;
		if (pawnSkill == 0f)
		{
			pawnSkill = 0.25f;
		}
		int timeToSetUpTrue = Mathf.Clamp((int)((float)Bipod.Props.ticksToSetUp / pawnSkill), 1, Bipod.Props.ticksToSetUp * 3);
		Pawn.CellsAdjacent8WayAndInside();
		yield return Toils_General.Wait(timeToSetUpTrue).WithProgressBarToilDelay(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			Bipod.SetUpEnd(weapon);
		});
	}
}
