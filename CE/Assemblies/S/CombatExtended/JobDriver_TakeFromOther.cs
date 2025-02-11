using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_TakeFromOther : JobDriver
{
	private TargetIndex thingInd = TargetIndex.A;

	private TargetIndex sourceInd = TargetIndex.B;

	private TargetIndex flagInd = TargetIndex.C;

	private Thing targetItem => job.GetTarget(thingInd).Thing;

	private Pawn takePawn => (Pawn)job.GetTarget(sourceInd).Thing;

	private bool doEquip => job.GetTarget(flagInd).HasThing;

	public override string GetReport()
	{
		string reportString = CE_JobDefOf.TakeFromOther.reportString;
		reportString = reportString.Replace("FlagC", doEquip ? "CE_TakeFromOther_Equipping".Translate() : "CE_TakeFromOther_Taking".Translate());
		reportString = reportString.Replace("TargetA", targetItem.Label);
		return reportString.Replace("TargetB", takePawn.LabelShort);
	}

	private bool DeadTakePawn()
	{
		return takePawn.Dead;
	}

	private Pawn RootHolder(IThingHolder container)
	{
		IThingHolder thingHolder = container;
		while (thingHolder != null && !(thingHolder is Pawn))
		{
			thingHolder = thingHolder.ParentHolder;
		}
		return thingHolder as Pawn;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(sourceInd);
		this.FailOnDestroyedOrNull(thingInd);
		this.FailOn(DeadTakePawn);
		yield return Toils_Reserve.Reserve(sourceInd, int.MaxValue, 0);
		yield return Toils_Goto.GotoThing(sourceInd, PathEndMode.Touch);
		yield return Toils_General.Wait(10);
		yield return new Toil
		{
			initAction = delegate
			{
				if (takePawn == RootHolder(targetItem.holdingOwner.Owner))
				{
					int count = ((targetItem.stackCount < pawn.CurJob.count) ? targetItem.stackCount : pawn.CurJob.count);
					takePawn.inventory.innerContainer.TryTransferToContainer(targetItem, pawn.inventory.innerContainer, count);
					if (doEquip)
					{
						pawn.TryGetComp<CompInventory>()?.TrySwitchToWeapon((ThingWithComps)targetItem);
					}
				}
				else
				{
					EndJobWith(JobCondition.Incompletable);
				}
			}
		};
		yield return Toils_Reserve.Release(sourceInd);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}
}
