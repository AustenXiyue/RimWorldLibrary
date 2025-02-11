using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class GenExplosionCE
{
	public const float MinExplosionScale = 0.1f;

	public const float MaxExplosionScale = 10f;

	public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, GasType? postExplosionGasType = null, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null, FloatRange? affectedAngle = null, bool doVisualEffects = true, float propagationSpeed = 1f, float excludeRadius = 0f, bool doSoundEffects = true, ThingDef postExplosionSpawnThingDefWater = null, float screenShakeFactor = 1f, SimpleCurve flammabilityChanceCurve = null, List<IntVec3> overrideCells = null, float height = 0f, float scaleFactor = 1f, bool destroyAfterwards = false, ThingWithComps explosionParentToDestroy = null)
	{
		scaleFactor = ((!(scaleFactor <= 0f)) ? Mathf.Clamp(scaleFactor, 0.1f, 10f) : 1f);
		if (map == null)
		{
			Log.Warning("CombatExtended :: Tried to do explosionCE in a null map.");
			return;
		}
		if (damAmount < 0)
		{
			damAmount = damType.defaultDamage;
			armorPenetration = damType.defaultArmorPenetration;
			if (damAmount < 0)
			{
				Log.ErrorOnce("CombatExtended :: Attempted to trigger an explosionCE without defined damage", 910948823);
				damAmount = 1;
			}
		}
		explosionSound = explosionSound ?? damType.soundExplosion;
		if (explosionSound == null)
		{
			Log.Error("CombatExtended :: SoundDef was null for DamageDef " + damType.defName + " as well as instigator " + instigator.ThingID);
		}
		damAmount = Mathf.RoundToInt((float)damAmount * scaleFactor);
		radius *= scaleFactor;
		armorPenetration *= scaleFactor;
		ExplosionCE explosionCE = GenSpawn.Spawn(CE_ThingDefOf.ExplosionCE, center, map) as ExplosionCE;
		IntVec3? needLOSToCell = null;
		IntVec3? needLOSToCell2 = null;
		if (direction.HasValue)
		{
			CalculateNeededLOSToCells(center, map, direction.Value, out needLOSToCell, out needLOSToCell2);
		}
		explosionCE.height = height;
		explosionCE.radius = radius;
		explosionCE.damType = damType;
		explosionCE.instigator = instigator;
		explosionCE.damAmount = damAmount;
		explosionCE.armorPenetration = armorPenetration;
		explosionCE.weapon = weapon;
		explosionCE.projectile = projectile;
		explosionCE.intendedTarget = intendedTarget;
		explosionCE.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
		explosionCE.preExplosionSpawnChance = preExplosionSpawnChance;
		explosionCE.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
		explosionCE.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
		explosionCE.postExplosionSpawnChance = postExplosionSpawnChance;
		explosionCE.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
		explosionCE.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
		explosionCE.chanceToStartFire = chanceToStartFire;
		explosionCE.damageFalloff = damageFalloff;
		explosionCE.needLOSToCell1 = needLOSToCell;
		explosionCE.needLOSToCell2 = needLOSToCell2;
		explosionCE.postExplosionGasType = postExplosionGasType;
		explosionCE.affectedAngle = affectedAngle;
		explosionCE.doVisualEffects = doVisualEffects;
		explosionCE.propagationSpeed = propagationSpeed;
		explosionCE.excludeRadius = excludeRadius;
		explosionCE.doSoundEffects = doSoundEffects;
		explosionCE.postExplosionSpawnThingDefWater = postExplosionSpawnThingDefWater;
		explosionCE.screenShakeFactor = screenShakeFactor;
		explosionCE.flammabilityChanceCurve = flammabilityChanceCurve;
		explosionCE.overrideCells = overrideCells;
		explosionCE.StartExplosionCE(explosionSound, ignoredThings);
		if (destroyAfterwards && !explosionParentToDestroy.Destroyed)
		{
			explosionParentToDestroy?.Kill(null);
		}
	}

	private static void CalculateNeededLOSToCells(IntVec3 position, Map map, float direction, out IntVec3? needLOSToCell1, out IntVec3? needLOSToCell2)
	{
		needLOSToCell1 = null;
		needLOSToCell2 = null;
		if (position.CanBeSeenOverFast(map))
		{
			return;
		}
		direction = GenMath.PositiveMod(direction, 360f);
		IntVec3 intVec = position;
		intVec.z++;
		IntVec3 intVec2 = position;
		intVec2.z--;
		IntVec3 intVec3 = position;
		intVec3.x--;
		IntVec3 intVec4 = position;
		intVec4.x++;
		if (direction < 90f)
		{
			if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
			{
				needLOSToCell1 = intVec3;
			}
			if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
			{
				needLOSToCell2 = intVec;
			}
		}
		else if (direction < 180f)
		{
			if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
			{
				needLOSToCell1 = intVec;
			}
			if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
			{
				needLOSToCell2 = intVec4;
			}
		}
		else if (direction < 270f)
		{
			if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
			{
				needLOSToCell1 = intVec4;
			}
			if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
			{
				needLOSToCell2 = intVec2;
			}
		}
		else
		{
			if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
			{
				needLOSToCell1 = intVec2;
			}
			if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
			{
				needLOSToCell2 = intVec3;
			}
		}
	}
}
