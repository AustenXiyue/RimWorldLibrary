using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_UnloadAmmo : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnForbidden(TargetIndex.A);
		ThingWithComps equipment = pawn?.equipment?.Primary;
		if (equipment != null)
		{
			CompMechAmmo mechAmmo = pawn?.GetComp<CompMechAmmo>();
			if (mechAmmo != null)
			{
				yield return Toils_Ammo.DropUnusedAmmo(mechAmmo);
			}
		}
	}
}
