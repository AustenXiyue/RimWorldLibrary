using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class CompExplosiveCE : ThingComp
{
	private CompProperties_ExplosiveCE Props => props as CompProperties_ExplosiveCE;

	public virtual void Explode(Thing instigator, Vector3 pos, Map map, float scaleFactor = 1f, float? direction = null, List<Thing> ignoredThings = null)
	{
		IntVec3 intVec = pos.ToIntVec3();
		if (map == null)
		{
			Log.Warning("Tried to do explodeCE in a null map.");
			return;
		}
		if (!intVec.InBounds(map))
		{
			Log.Warning("Tried to explodeCE out of bounds");
			return;
		}
		foreach (CompFragments comp in parent.GetComps<CompFragments>())
		{
			comp.Throw(pos, map, instigator);
		}
		if (Props.explosiveRadius > 0f && parent.def != null)
		{
			float explosiveRadius = Props.explosiveRadius;
			DamageDef explosiveDamageType = Props.explosiveDamageType;
			int damAmount = GenMath.RoundRandom(Props.damageAmountBase);
			float explosionArmorPenetration = Props.GetExplosionArmorPenetration();
			SoundDef explosionSound = Props.explosionSound;
			ThingDef def = parent.def;
			ThingDef postExplosionSpawnThingDef = Props.postExplosionSpawnThingDef;
			float postExplosionSpawnChance = Props.postExplosionSpawnChance;
			int postExplosionSpawnThingCount = Props.postExplosionSpawnThingCount;
			GasType? postExplosionGasType = Props.postExplosionGasType;
			bool applyDamageToExplosionCellsNeighbors = Props.applyDamageToExplosionCellsNeighbors;
			ThingDef preExplosionSpawnThingDef = Props.preExplosionSpawnThingDef;
			float preExplosionSpawnChance = Props.preExplosionSpawnChance;
			int preExplosionSpawnThingCount = Props.preExplosionSpawnThingCount;
			float chanceToStartFire = Props.chanceToStartFire;
			bool damageFalloff = Props.damageFalloff;
			float y = pos.y;
			GenExplosionCE.DoExplosion(intVec, map, explosiveRadius, explosiveDamageType, instigator, damAmount, explosionArmorPenetration, explosionSound, null, def, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings, null, doVisualEffects: true, 1f, 0f, doSoundEffects: true, null, 1f, null, null, y, scaleFactor);
		}
	}
}
