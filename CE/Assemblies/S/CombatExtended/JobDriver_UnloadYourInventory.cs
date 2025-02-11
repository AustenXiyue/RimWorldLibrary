using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_UnloadYourInventory : JobDriver
{
	private const TargetIndex ItemToHaulInd = TargetIndex.A;

	private const TargetIndex StoreCellInd = TargetIndex.B;

	private const int UnloadDuration = 10;

	private int amountToDrop;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref amountToDrop, "amountToDrop", -1);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	[DebuggerHidden]
	public override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_General.Wait(10);
		yield return new Toil
		{
			initAction = delegate
			{
				IntVec3 storeCell;
				if (!pawn.inventory.UnloadEverything || !pawn.GetAnythingForDrop(out var resultingThing, out var dropCount))
				{
					EndJobWith(JobCondition.Succeeded);
				}
				else if (!StoreUtility.TryFindStoreCellNearColonyDesperate(resultingThing, pawn, out storeCell))
				{
					pawn.inventory.innerContainer.TryDrop(resultingThing, pawn.Position, pawn.Map, ThingPlaceMode.Near, dropCount, out resultingThing);
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					pawn.CurJob.SetTarget(TargetIndex.A, resultingThing);
					pawn.CurJob.SetTarget(TargetIndex.B, storeCell);
					amountToDrop = dropCount;
				}
			}
		};
		yield return Toils_Reserve.Reserve(TargetIndex.B);
		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
		yield return new Toil
		{
			initAction = delegate
			{
				Thing resultingTransferredItem = pawn.CurJob.GetTarget(TargetIndex.A).Thing;
				if (resultingTransferredItem == null || !pawn.inventory.innerContainer.Contains(resultingTransferredItem))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !resultingTransferredItem.def.EverStorable(willMinifyIfPossible: true))
					{
						pawn.inventory.innerContainer.TryDrop(resultingTransferredItem, pawn.Position, pawn.Map, ThingPlaceMode.Near, amountToDrop, out resultingTransferredItem);
						EndJobWith(JobCondition.Succeeded);
					}
					else
					{
						pawn.inventory.innerContainer.TryTransferToContainer(resultingTransferredItem, pawn.carryTracker.innerContainer, amountToDrop, out resultingTransferredItem);
						pawn.CurJob.count = amountToDrop;
						pawn.CurJob.SetTarget(TargetIndex.A, resultingTransferredItem);
					}
					resultingTransferredItem.SetForbidden(value: false, warnOnFail: false);
					if (!pawn.HasAnythingForDrop())
					{
						pawn.inventory.UnloadEverything = false;
					}
				}
			}
		};
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
		yield return carryToCell;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
	}
}
