using RimWorld;
using UnityEngine;
using Verse;

namespace ProjectileImpactFX;

public class TrailThrower
{
	public static void ThrowSmoke(Vector3 loc, float size, Map map, string defName)
	{
		if (loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
		{
			FleckDef fleckDef = DefDatabase<FleckDef>.GetNamed(defName) ?? null;
			if (fleckDef != null)
			{
				Rand.PushState();
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, fleckDef, Rand.Range(1.5f, 2.5f) * size);
				dataStatic.rotationRate = Rand.Range(-30f, 30f);
				dataStatic.velocityAngle = Rand.Range(30, 40);
				dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
				Rand.PopState();
				map.flecks.CreateFleck(dataStatic);
			}
		}
	}
}
