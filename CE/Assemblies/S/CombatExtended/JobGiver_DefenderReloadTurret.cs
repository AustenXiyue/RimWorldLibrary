using System;
using CombatExtended.CombatExtended.Jobs.Utils;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_DefenderReloadTurret : ThinkNode_JobGiver
{
	public const float AmmoReloadThreshold = 0.5f;

	public override Job TryGiveJob(Pawn pawn)
	{
		Building_TurretGunCE building_TurretGunCE = TryFindTurretWhichNeedsReloading(pawn);
		if (building_TurretGunCE == null)
		{
			return null;
		}
		return JobGiverUtils_Reload.MakeReloadJob(pawn, building_TurretGunCE);
	}

	private Building_TurretGunCE TryFindTurretWhichNeedsReloading(Pawn pawn)
	{
		Predicate<Thing> validator = (Thing t) => t is Building_Turret building_Turret && building_Turret.ShouldReload() && JobGiverUtils_Reload.CanReload(pawn, building_Turret, forced: false, emergency: true);
		Thing thing = pawn.Map.GetComponent<TurretTracker>().ClosestTurret(pawn.Position, PathEndMode.Touch, TraverseParms.For(pawn), 100f, validator);
		return thing as Building_TurretGunCE;
	}
}
