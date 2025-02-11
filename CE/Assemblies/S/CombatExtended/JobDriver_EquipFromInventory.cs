using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_EquipFromInventory : JobDriver
{
	public ThingWithComps Weapon => (ThingWithComps)job.targetA.Thing;

	public CompInventory CompInventory => pawn.TryGetComp<CompInventory>();

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		yield return Toils_General.Wait(5).FailOn(delegate(Toil toil)
		{
			Pawn_InventoryTracker inventory = toil.actor.inventory;
			return inventory == null || !inventory.Contains(Weapon);
		});
		yield return Toils_General.Do(delegate
		{
			CompInventory.TrySwitchToWeapon(Weapon, stopJob: false);
			if (pawn.equipment.Contains(Weapon))
			{
				EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				EndJobWith(JobCondition.Incompletable);
			}
		});
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}
}
