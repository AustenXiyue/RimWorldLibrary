using Verse;
using Verse.AI;

namespace CombatExtended;

public static class Toils_Ammo
{
	public static Toil Drop(ThingDef def, int count)
	{
		Toil toil = ToilMaker.MakeToil("DropAmmo");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			actor.inventory.DropCount(def, count);
		};
		return toil;
	}

	public static Toil TryUnloadAmmo(CompAmmoUser ammoUser)
	{
		Toil toil = ToilMaker.MakeToil("TryUnloadAmmo");
		toil.initAction = delegate
		{
			ammoUser?.TryUnload(forceUnload: true);
		};
		return toil;
	}

	public static Toil DropUnusedAmmo(CompMechAmmo mechAmmo)
	{
		Toil toil = ToilMaker.MakeToil("DropUnusedAmmo");
		toil.initAction = delegate
		{
			mechAmmo.DropUnusedAmmo();
		};
		return toil;
	}
}
