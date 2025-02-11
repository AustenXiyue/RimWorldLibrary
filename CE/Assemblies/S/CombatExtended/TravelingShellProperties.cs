using System;
using Verse;

namespace CombatExtended;

public class TravelingShellProperties
{
	public float tilesPerTick = 0.2f;

	public float range = 30f;

	public float damage;

	public string iconPath;

	public Type workerClass = typeof(WorldObjectDamageWorker);

	[Unsaved(false)]
	private WorldObjectDamageWorker workerInt;

	public WorldObjectDamageWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (WorldObjectDamageWorker)Activator.CreateInstance(workerClass);
			}
			return workerInt;
		}
	}
}
