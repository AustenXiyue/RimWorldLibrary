using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class TurretTracker : MapComponent
{
	public HashSet<Building_Turret> Turrets = new HashSet<Building_Turret>();

	public TurretTracker(Map map)
		: base(map)
	{
	}

	public void Register(Building_Turret t)
	{
		if (!Turrets.Contains(t))
		{
			Turrets.Add(t);
		}
	}

	public void Unregister(Building_Turret t)
	{
		if (Turrets.Contains(t))
		{
			Turrets.Remove(t);
		}
	}

	public Thing ClosestTurret(IntVec3 position, PathEndMode pathEndMode, TraverseParms parms, float maxDist, Predicate<Thing> validator = null)
	{
		return GenClosest.ClosestThingReachable(position, map, ThingRequest.ForUndefined(), pathEndMode, parms, maxDist, validator, Turrets);
	}
}
