using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace CombatExtended.WorldObjects;

public static class WorldCompUtility
{
	public static IEnumerable<IWorldCompCE> GetCompsCE(this WorldObject worldObject)
	{
		for (int i = 0; i < worldObject.comps.Count; i++)
		{
			WorldObjectComp worldObjectComp = worldObject.comps[i];
			if (worldObjectComp is IWorldCompCE throttleable)
			{
				yield return throttleable;
			}
		}
	}

	public static WorldObjectDamageWorker GetWorldObjectDamageWorker(this ThingDef shellDef)
	{
		return (shellDef?.GetProjectile()?.projectile as ProjectilePropertiesCE)?.shellingProps?.Worker ?? new WorldObjectDamageWorker();
	}
}
