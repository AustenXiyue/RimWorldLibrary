using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.Utilities;

public static class GenClosest
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ThingsTracker GetThingTracker(this Map map)
	{
		return ThingsTracker.GetTracker(map);
	}

	public static IEnumerable<Thing> SimilarInRange(this Thing thing, float range)
	{
		ThingsTracker thingTracker = thing.Map.GetThingTracker();
		return thingTracker.SimilarInRangeOf(thing, range);
	}

	public static IEnumerable<Thing> ThingsByDefInRange(this IntVec3 cell, Map map, ThingDef thingDef, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return thingTracker.ThingsInRangeOf(thingDef, cell, range);
	}

	public static IEnumerable<Pawn> PawnsInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Pawns, cell, range)
			select t as Pawn;
	}

	public static IEnumerable<Pawn> HostilesInRange(this IntVec3 cell, Map map, Faction faction, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Pawns, cell, range)
			where t.Faction?.HostileTo(faction) ?? false
			select t as Pawn;
	}

	public static IEnumerable<AmmoThing> AmmoInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Ammo, cell, range)
			select t as AmmoThing;
	}

	public static IEnumerable<ThingWithComps> WeaponsInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Weapons, cell, range)
			select t as ThingWithComps;
	}

	public static IEnumerable<Thing> AttachmentsInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Attachments, cell, range)
			select (t);
	}

	public static IEnumerable<ThingWithComps> FlaresInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Flares, cell, range)
			select t as ThingWithComps;
	}

	public static IEnumerable<ThingWithComps> MedicineInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Medicine, cell, range)
			select t as ThingWithComps;
	}

	public static IEnumerable<Apparel> ApparelInRange(this IntVec3 cell, Map map, float range)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Apparel, cell, range)
			select t as Apparel;
	}

	public static IEnumerable<Thing> SimilarInRange(this Thing thing, float range, PathEndMode pathEndMode = PathEndMode.None, TraverseMode traverseMode = TraverseMode.ByPawn, Danger danger = Danger.Unspecified)
	{
		ThingsTracker thingTracker = thing.Map.GetThingTracker();
		return ThingsReachableFrom(thing.Map, thingTracker.SimilarInRangeOf(thing, range), thing, pathEndMode, traverseMode, danger);
	}

	public static IEnumerable<Thing> PawnsNearSegment(this IntVec3 origin, IntVec3 destination, Map map, float range, bool behind = false, bool infront = true)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in thingTracker.ThingsNearSegment(TrackedThingsRequestCategory.Pawns, origin, destination, range, behind, infront)
			select t as Pawn;
	}

	public static IEnumerable<Thing> ThingsByDefInRange(this IntVec3 cell, Map map, ThingDef thingDef, float range, PathEndMode pathEndMode = PathEndMode.None, TraverseMode traverseMode = TraverseMode.ByPawn, Danger danger = Danger.Unspecified)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return ThingsReachableFrom(map, thingTracker.ThingsInRangeOf(thingDef, cell, range), cell, pathEndMode, traverseMode, danger);
	}

	public static IEnumerable<Pawn> PawnsInRange(this IntVec3 cell, Map map, float range, PathEndMode pathEndMode = PathEndMode.None, TraverseMode traverseMode = TraverseMode.ByPawn, Danger danger = Danger.Unspecified)
	{
		ThingsTracker thingTracker = map.GetThingTracker();
		return from t in ThingsReachableFrom(map, thingTracker.ThingsInRangeOf(TrackedThingsRequestCategory.Pawns, cell, range), cell, pathEndMode, traverseMode, danger)
			select t as Pawn;
	}

	private static IEnumerable<Thing> ThingsReachableFrom(Map map, IEnumerable<Thing> things, IntVec3 position, PathEndMode pathEndMode, TraverseMode traverseMode, Danger danger = Danger.Unspecified)
	{
		return things.Where((Thing t) => map.reachability.CanReach(position, t, pathEndMode, traverseMode, danger));
	}

	private static IEnumerable<Thing> ThingsReachableFrom(Map map, IEnumerable<Thing> things, Thing thing, PathEndMode pathEndMode, TraverseMode traverseMode, Danger danger = Danger.Unspecified)
	{
		IntVec3 position = thing.Position;
		return things.Where((Thing t) => map.reachability.CanReach(position, t, pathEndMode, traverseMode, danger));
	}
}
