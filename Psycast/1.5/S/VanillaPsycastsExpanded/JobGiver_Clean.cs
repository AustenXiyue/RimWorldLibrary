using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaPsycastsExpanded;

public class JobGiver_Clean : ThinkNode_JobGiver
{
	private int MinTicksSinceThickened = 600;

	public PathEndMode PathEndMode => PathEndMode.Touch;

	public IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
	}

	public bool ShouldSkip(Pawn pawn)
	{
		return pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count == 0;
	}

	public bool HasJobOnThing(Pawn pawn, Thing t)
	{
		if (!(t is Filth filth))
		{
			return false;
		}
		if (!filth.Map.areaManager.Home[filth.Position])
		{
			return false;
		}
		if (!pawn.CanReserve(t))
		{
			return false;
		}
		if (filth.TicksSinceThickened < MinTicksSinceThickened)
		{
			return false;
		}
		return true;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (ShouldSkip(pawn))
		{
			return null;
		}
		Predicate<Thing> validator = (Thing x) => x.def.category == ThingCategory.Filth && HasJobOnThing(pawn, x);
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Filth), PathEndMode, TraverseParms.For(pawn, Danger.Some), 100f, validator, PotentialWorkThingsGlobal(pawn));
		if (thing == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Clean);
		job.AddQueuedTarget(TargetIndex.A, thing);
		int num = 15;
		Map map = thing.Map;
		Room room = thing.GetRoom();
		for (int i = 0; i < 100; i++)
		{
			IntVec3 c2 = thing.Position + GenRadial.RadialPattern[i];
			if (!ShouldClean(c2))
			{
				continue;
			}
			List<Thing> thingList = c2.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing2 = thingList[j];
				if (HasJobOnThing(pawn, thing2) && thing2 != thing)
				{
					job.AddQueuedTarget(TargetIndex.A, thing2);
				}
			}
			if (job.GetTargetQueue(TargetIndex.A).Count >= num)
			{
				break;
			}
		}
		if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
		{
			job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
		}
		return job;
		bool ShouldClean(IntVec3 c)
		{
			if (!c.InBounds(map))
			{
				return false;
			}
			Room room2 = c.GetRoom(map);
			if (room == room2)
			{
				return true;
			}
			Region region = c.GetDoor(map)?.GetRegion(RegionType.Portal);
			if (region != null && !region.links.NullOrEmpty())
			{
				for (int k = 0; k < region.links.Count; k++)
				{
					RegionLink regionLink = region.links[k];
					for (int l = 0; l < 2; l++)
					{
						if (regionLink.regions[l] != null && regionLink.regions[l] != region && regionLink.regions[l].valid && regionLink.regions[l].Room == room)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
