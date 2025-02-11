using System.Collections.Generic;
using Verse.AI;

namespace AncotLibrary;

public class JobDriver_GainHediffSelf_Sharpen : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(base.TargetThingA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil = Toils_General.Wait(60);
		toil.WithProgressBarToilDelay(TargetIndex.A);
		toil.FailOnDespawnedOrNull(TargetIndex.A);
		yield return toil;
		yield return Toils_General.Do(GainHediff);
	}

	private void GainHediff()
	{
		pawn.health.AddHediff(AncotDefOf.Ancot_Sharpen, null, null);
	}
}
