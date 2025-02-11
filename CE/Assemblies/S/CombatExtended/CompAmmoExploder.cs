using System.Linq;
using Verse;

namespace CombatExtended;

public class CompAmmoExploder : ThingComp
{
	public DamageDef damageDef;

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		damageDef = dinfo.Def;
		base.PostPreApplyDamage(ref dinfo, out absorbed);
	}

	public void PostDamageResult(DamageWorker.DamageResult damage)
	{
		BodyPartExploderExt bodyPartExploderExt = damage?.parts?.Find((BodyPartRecord x) => x.def.HasModExtension<BodyPartExploderExt>())?.def?.GetModExtension<BodyPartExploderExt>() ?? null;
		if (bodyPartExploderExt != null && damageDef != null && Rand.Chance(bodyPartExploderExt.triggerChance) && bodyPartExploderExt.allowedDamageDefs.Contains(damageDef))
		{
			DetonateCarriedAmmo();
		}
	}

	public void DetonateCarriedAmmo()
	{
		if (((Pawn)parent)?.inventory?.innerContainer?.Where((Thing x) => x is AmmoThing).EnumerableNullOrEmpty() ?? true)
		{
			return;
		}
		foreach (AmmoThing item in ((Pawn)parent).inventory.innerContainer.Where((Thing x) => x is AmmoThing))
		{
			if (((((AmmoDef)item.def)?.AmmoSetDefs[0]?.ammoTypes[0]?.projectile ?? null)?.projectile ?? null) is ProjectilePropertiesCE { explosionRadius: >0f } projectilePropertiesCE)
			{
				IntVec3 position = parent.Position;
				Map map = parent.Map;
				float explosionRadius = projectilePropertiesCE.explosionRadius;
				DamageDef damType = projectilePropertiesCE.damageDef;
				int damageAmount = projectilePropertiesCE.GetDamageAmount(1f);
				float armorPenetrationSharp = projectilePropertiesCE.armorPenetrationSharp;
				float postExplosionSpawnChance = projectilePropertiesCE.postExplosionSpawnChance;
				int postExplosionSpawnThingCount = projectilePropertiesCE.postExplosionSpawnThingCount;
				ThingDef postExplosionSpawnThingDef = projectilePropertiesCE.postExplosionSpawnThingDef;
				float preExplosionSpawnChance = projectilePropertiesCE.preExplosionSpawnChance;
				int preExplosionSpawnThingCount = projectilePropertiesCE.preExplosionSpawnThingCount;
				ThingDef preExplosionSpawnThingDef = projectilePropertiesCE.preExplosionSpawnThingDef;
				GenExplosionCE.DoExplosion(position, map, explosionRadius, damType, null, damageAmount, armorPenetrationSharp, null, null, null, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, null, applyDamageToExplosionCellsNeighbors: false, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, 0f, damageFalloff: false, null, null, null);
			}
		}
	}
}
