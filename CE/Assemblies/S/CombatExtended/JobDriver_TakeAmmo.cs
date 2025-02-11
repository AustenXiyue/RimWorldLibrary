using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_TakeAmmo : JobDriver
{
	public int amount;

	private Thing Ammo => job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Ammo, job, 1, -1, null, errorOnFailed);
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnForbidden(TargetIndex.A);
		CompAmmoUser ammoUser = pawn?.equipment?.Primary?.GetComp<CompAmmoUser>();
		if (ammoUser != null)
		{
			if (job.count > 0)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
				yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
			}
			else if (job.count < 0)
			{
				yield return Toils_Ammo.Drop(Ammo.def, -job.count);
			}
			else
			{
				Log.Error("[MechTakeAmmoCE] Trying to start job that no need ammo");
			}
		}
	}
}
