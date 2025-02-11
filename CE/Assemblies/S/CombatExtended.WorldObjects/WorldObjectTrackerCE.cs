using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace CombatExtended.WorldObjects;

public class WorldObjectTrackerCE : WorldComponent
{
	private class TrackedObject
	{
		public readonly WorldObject worldObject;

		public readonly List<IWorldCompCE> compsCE;

		public bool IsValid => !compsCE.NullOrEmpty() && worldObject != null && !worldObject.destroyed;

		public TrackedObject(WorldObject worldObject, List<IWorldCompCE> compsCE)
		{
			this.worldObject = worldObject;
			this.compsCE = compsCE;
		}

		public void ThrottledCompsTick()
		{
			if (IsValid)
			{
				for (int i = 0; i < compsCE.Count; i++)
				{
					compsCE[i].ThrottledCompTick();
				}
			}
		}
	}

	public const int THROTTLED_TICK_INTERVAL = 15;

	private int cleanUpIndex = 0;

	private int updateIndex = 0;

	private List<TrackedObject>[] trackedObjects = new List<TrackedObject>[15];

	public IEnumerable<WorldObject> TrackedObjects
	{
		get
		{
			for (int i = 0; i < 15; i++)
			{
				List<TrackedObject> items = trackedObjects[i];
				for (int j = 0; j < items.Count; j++)
				{
					if (items[j].IsValid)
					{
						yield return items[j].worldObject;
					}
				}
			}
		}
	}

	public WorldObjectTrackerCE(World world)
		: base(world)
	{
		for (int i = 0; i < 15; i++)
		{
			trackedObjects[i] = new List<TrackedObject>();
		}
	}

	public override void WorldComponentTick()
	{
		base.WorldComponentTick();
		int ticksGame = GenTicks.TicksGame;
		if (ticksGame % 250 == 0)
		{
			trackedObjects[cleanUpIndex].RemoveAll((TrackedObject u) => !u.IsValid);
			cleanUpIndex = (cleanUpIndex + 1) % 15;
		}
		if (!trackedObjects[updateIndex].NullOrEmpty())
		{
			List<TrackedObject> list = trackedObjects[updateIndex];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsValid)
				{
					try
					{
						list[i].ThrottledCompsTick();
					}
					catch (Exception arg)
					{
						Log.Error($"CE: Error while updating WorldUpdatable {list[i]} {arg}");
					}
				}
			}
		}
		updateIndex = (updateIndex + 1) % 15;
	}

	public void TryRegister(WorldObject worldObject)
	{
		List<IWorldCompCE> list = worldObject.GetCompsCE().ToList();
		if (list.NullOrEmpty())
		{
			return;
		}
		int num = 0;
		int count = trackedObjects[0].Count;
		for (int i = 0; i < 15; i++)
		{
			if (trackedObjects[i].Any((TrackedObject u) => u.worldObject == worldObject))
			{
				return;
			}
			if (trackedObjects[i].Count < count || (trackedObjects[i].Count == count && Rand.Chance(0.5f)))
			{
				num = i;
				count = trackedObjects[i].Count;
			}
		}
		TrackedObject item = new TrackedObject(worldObject, list);
		trackedObjects[num].Add(item);
	}

	public void TryDeRegister(WorldObject worldObject)
	{
		for (int i = 0; i < 15; i++)
		{
			trackedObjects[i].RemoveAll((TrackedObject u) => !u.IsValid || u.worldObject == worldObject);
		}
	}
}
