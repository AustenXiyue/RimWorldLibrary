using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_GiveAmmo : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return GetActor().CanReserveAndReach(base.TargetA, PathEndMode.ClosestTouch, Danger.Deadly);
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);
		yield return Toils_General.Do(delegate
		{
			Pawn pawn = (Pawn)base.TargetA.Thing;
			CompAmmoGiver compAmmoGiver = pawn.TryGetComp<CompAmmoGiver>();
			Thing thing = ThingMaker.MakeThing(base.TargetB.Thing.def);
			thing.HitPoints = base.TargetB.Thing.HitPoints;
			thing.stackCount = compAmmoGiver.ammoAmountToGive;
			base.TargetB.Thing.stackCount -= compAmmoGiver.ammoAmountToGive;
			if (base.TargetB.Thing.stackCount <= 0)
			{
				base.TargetB.Thing.Destroy();
			}
			pawn.inventory.TryAddItemNotForSale(thing);
		});
	}
}
