using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

internal class JobDriver_HunkerDown : JobDriver
{
	private const int GetUpCheckInterval = 60;

	public override void SetInitialPosture()
	{
		pawn.jobs.posture = PawnPosture.LayingOnGroundNormal;
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil toilWait = new Toil();
		toilWait.initAction = delegate
		{
			toilWait.actor.pather.StopDead();
		};
		Toil toilNothing = new Toil
		{
			defaultCompleteMode = ToilCompleteMode.Delay,
			defaultDuration = 60
		};
		yield return toilWait;
		yield return toilNothing;
		yield return Toils_Jump.JumpIf(toilNothing, delegate
		{
			CompSuppressable compSuppressable = pawn.TryGetComp<CompSuppressable>();
			if (compSuppressable == null)
			{
				return false;
			}
			return compSuppressable.CanReactToSuppression && compSuppressable.IsHunkering;
		});
	}
}
