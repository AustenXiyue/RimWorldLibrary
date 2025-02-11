using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ProjectileCE_BunkerBuster : ProjectileCE_Explosive
{
	public override void Impact(Thing hitThing)
	{
		if (hitThing is Building)
		{
			ProjectilePropertiesCE projectilePropertiesCE = (ProjectilePropertiesCE)def.projectile;
			float num = projectilePropertiesCE.fuze_delay;
			if (projectilePropertiesCE.HP_penetration)
			{
				num /= (float)hitThing.HitPoints / projectilePropertiesCE.HP_penetration_ratio;
			}
			Vector3 vector = hitThing.Position.ToVector3() - new Vector3(origin.x, 0f, origin.y);
			IntVec3 intVec = (new Vector3(hitThing.Position.x, 0f, hitThing.Position.z) + vector.normalized * Math.Max(num, 1f)).ToIntVec3();
			GenExplosionCE.DoExplosion(intVec, base.Map, projectilePropertiesCE.explosionRadius, projectilePropertiesCE.damageDef, this, Mathf.FloorToInt(DamageAmount), DamageAmount / 5f, null, null, null, null, null, 0f, 1, damageFalloff: projectilePropertiesCE.explosionDamageFalloff, applyDamageToExplosionCellsNeighbors: projectilePropertiesCE.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: projectilePropertiesCE.preExplosionSpawnThingDef, preExplosionSpawnChance: projectilePropertiesCE.preExplosionSpawnChance, preExplosionSpawnThingCount: projectilePropertiesCE.preExplosionSpawnThingCount, postExplosionGasType: projectilePropertiesCE.postExplosionGasType, chanceToStartFire: 0f, direction: null, ignoredThings: null, affectedAngle: null);
			foreach (CompFragments comp in GetComps<CompFragments>())
			{
				comp.Throw(intVec.ToVector3(), base.Map, this);
			}
			Destroy();
		}
		else
		{
			base.Impact(hitThing);
		}
	}
}
