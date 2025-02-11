using CombatExtended.Compatibility;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_ManTurretsNearSelfCE : JobGiver_ManTurretsNearSelf
{
	public override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = pawn.Map.GetComponent<TurretTracker>().ClosestTurret(GetRoot(pawn), PathEndMode.InteractionCell, TraverseParms.For(pawn), maxDistFromPoint, (Thing t) => t.def.hasInteractionCell && t.def.HasComp(typeof(CompMannable)) && pawn.CanReserve(t) && FindAmmoForTurret(pawn, t) != null);
		if (thing == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
		job.expiryInterval = 2000;
		job.checkOverrideOnExpire = true;
		return job;
	}

	private static Thing FindAmmoForTurret(Pawn pawn, Thing turret)
	{
		CompAmmoUser compAmmo = (turret as Building_Turret)?.GetAmmo();
		if (compAmmo == null || !compAmmo.UseAmmo)
		{
			return null;
		}
		return GenClosest.ClosestThingReachable(turret.Position, turret.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1) && compAmmo.Props.ammoSet.ammoTypes.Any((AmmoLink l) => l.ammo == t.def));
	}
}
