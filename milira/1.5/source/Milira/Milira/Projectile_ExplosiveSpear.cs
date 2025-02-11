using UnityEngine;
using Verse;

namespace Milira;

public class Projectile_ExplosiveSpear : Projectile
{
	private int ticksToDetonation;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
	}

	public override void Tick()
	{
		base.Tick();
		if (ticksToDetonation > 0)
		{
			ticksToDetonation--;
			if (ticksToDetonation <= 0)
			{
				Explode();
			}
		}
	}

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		if (blockedByShield || def.projectile.explosionDelay == 0)
		{
			Explode();
			return;
		}
		landed = true;
		ticksToDetonation = def.projectile.explosionDelay;
		GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction, launcher);
	}

	protected virtual void Explode()
	{
		Map map = base.Map;
		Destroy();
		if (def.projectile.explosionEffect != null)
		{
			Effecter effecter = def.projectile.explosionEffect.Spawn();
			effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
			effecter.Cleanup();
		}
		IntVec3 position = launcher.Position;
		IntVec3 position2 = base.Position;
		Vector3 v = (position2 - position).ToVector3();
		v.Normalize();
		float num = v.ToAngleFlat();
		IntVec3 position3 = base.Position;
		DamageDef damageDef = def.projectile.damageDef;
		Thing instigator = launcher;
		int damageAmount = DamageAmount;
		float armorPenetration = ArmorPenetration;
		SoundDef soundExplode = def.projectile.soundExplode;
		ThingDef weapon = equipmentDef;
		ThingDef projectile = def;
		Thing thing = intendedTarget.Thing;
		ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
		ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
		float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
		int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
		GasType? postExplosionGasType = def.projectile.postExplosionGasType;
		ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
		float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
		int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
		bool applyDamageToExplosionCellsNeighbors = def.projectile.applyDamageToExplosionCellsNeighbors;
		ThingDef preExplosionSpawnThingDef2 = preExplosionSpawnThingDef;
		float preExplosionSpawnChance2 = preExplosionSpawnChance;
		int preExplosionSpawnThingCount2 = preExplosionSpawnThingCount;
		float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
		bool explosionDamageFalloff = def.projectile.explosionDamageFalloff;
		float? direction = origin.AngleToFlat(destination);
		float expolosionPropagationSpeed = def.projectile.damageDef.expolosionPropagationSpeed;
		float screenShakeFactor = def.projectile.screenShakeFactor;
		GenExplosion.DoExplosion(position3, map, 1.5f, damageDef, instigator, damageAmount, armorPenetration, soundExplode, weapon, projectile, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef2, preExplosionSpawnChance2, preExplosionSpawnThingCount2, explosionChanceToStartFire, explosionDamageFalloff, direction, null, null, doVisualEffects: true, expolosionPropagationSpeed, 0f, doSoundEffects: true, postExplosionSpawnThingDefWater, screenShakeFactor);
		if (num - 30f < -180f || num + 30f > 180f)
		{
			if (num < 0f)
			{
				IntVec3 position4 = base.Position;
				float explosionRadius = def.projectile.explosionRadius;
				DamageDef damageDef2 = def.projectile.damageDef;
				Thing instigator2 = launcher;
				int damageAmount2 = DamageAmount;
				float armorPenetration2 = ArmorPenetration;
				SoundDef soundExplode2 = def.projectile.soundExplode;
				ThingDef weapon2 = equipmentDef;
				ThingDef projectile2 = def;
				Thing thing2 = intendedTarget.Thing;
				ThingDef postExplosionSpawnThingDef2 = def.projectile.postExplosionSpawnThingDef;
				preExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDefWater;
				float postExplosionSpawnChance2 = def.projectile.postExplosionSpawnChance;
				int postExplosionSpawnThingCount2 = def.projectile.postExplosionSpawnThingCount;
				GasType? postExplosionGasType2 = def.projectile.postExplosionGasType;
				postExplosionSpawnThingDefWater = def.projectile.preExplosionSpawnThingDef;
				screenShakeFactor = def.projectile.preExplosionSpawnChance;
				preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, screenShakeFactor: def.projectile.screenShakeFactor, center: position4, map: map, radius: explosionRadius, damType: damageDef2, instigator: instigator2, damAmount: damageAmount2, armorPenetration: armorPenetration2, explosionSound: soundExplode2, weapon: weapon2, projectile: projectile2, intendedTarget: thing2, postExplosionSpawnThingDef: postExplosionSpawnThingDef2, postExplosionSpawnChance: postExplosionSpawnChance2, postExplosionSpawnThingCount: postExplosionSpawnThingCount2, postExplosionGasType: postExplosionGasType2, affectedAngle: new FloatRange(-180f, num + 30f), doVisualEffects: true, excludeRadius: 0f, doSoundEffects: true, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
				IntVec3 position5 = base.Position;
				float explosionRadius2 = def.projectile.explosionRadius;
				DamageDef damageDef3 = def.projectile.damageDef;
				Thing instigator3 = launcher;
				int damageAmount3 = DamageAmount;
				float armorPenetration3 = ArmorPenetration;
				SoundDef soundExplode3 = def.projectile.soundExplode;
				ThingDef weapon3 = equipmentDef;
				ThingDef projectile3 = def;
				Thing thing3 = intendedTarget.Thing;
				ThingDef postExplosionSpawnThingDef3 = def.projectile.postExplosionSpawnThingDef;
				postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
				float postExplosionSpawnChance3 = def.projectile.postExplosionSpawnChance;
				int postExplosionSpawnThingCount3 = def.projectile.postExplosionSpawnThingCount;
				GasType? postExplosionGasType3 = def.projectile.postExplosionGasType;
				preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
				preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
				preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, screenShakeFactor: def.projectile.screenShakeFactor, center: position5, map: map, radius: explosionRadius2, damType: damageDef3, instigator: instigator3, damAmount: damageAmount3, armorPenetration: armorPenetration3, explosionSound: soundExplode3, weapon: weapon3, projectile: projectile3, intendedTarget: thing3, postExplosionSpawnThingDef: postExplosionSpawnThingDef3, postExplosionSpawnChance: postExplosionSpawnChance3, postExplosionSpawnThingCount: postExplosionSpawnThingCount3, postExplosionGasType: postExplosionGasType3, affectedAngle: new FloatRange(360f + num - 30f, 180f), doVisualEffects: true, excludeRadius: 0f, doSoundEffects: true, postExplosionSpawnThingDefWater: postExplosionSpawnThingDefWater);
			}
			else
			{
				IntVec3 position6 = base.Position;
				float explosionRadius3 = def.projectile.explosionRadius;
				DamageDef damageDef4 = def.projectile.damageDef;
				Thing instigator4 = launcher;
				int damageAmount4 = DamageAmount;
				float armorPenetration4 = ArmorPenetration;
				SoundDef soundExplode4 = def.projectile.soundExplode;
				ThingDef weapon4 = equipmentDef;
				ThingDef projectile4 = def;
				Thing thing4 = intendedTarget.Thing;
				ThingDef postExplosionSpawnThingDef4 = def.projectile.postExplosionSpawnThingDef;
				preExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDefWater;
				float postExplosionSpawnChance4 = def.projectile.postExplosionSpawnChance;
				int postExplosionSpawnThingCount4 = def.projectile.postExplosionSpawnThingCount;
				GasType? postExplosionGasType4 = def.projectile.postExplosionGasType;
				postExplosionSpawnThingDefWater = def.projectile.preExplosionSpawnThingDef;
				screenShakeFactor = def.projectile.preExplosionSpawnChance;
				preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, screenShakeFactor: def.projectile.screenShakeFactor, center: position6, map: map, radius: explosionRadius3, damType: damageDef4, instigator: instigator4, damAmount: damageAmount4, armorPenetration: armorPenetration4, explosionSound: soundExplode4, weapon: weapon4, projectile: projectile4, intendedTarget: thing4, postExplosionSpawnThingDef: postExplosionSpawnThingDef4, postExplosionSpawnChance: postExplosionSpawnChance4, postExplosionSpawnThingCount: postExplosionSpawnThingCount4, postExplosionGasType: postExplosionGasType4, affectedAngle: new FloatRange(num - 30f, 180f), doVisualEffects: true, excludeRadius: 0f, doSoundEffects: true, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
				IntVec3 position7 = base.Position;
				float explosionRadius4 = def.projectile.explosionRadius;
				DamageDef damageDef5 = def.projectile.damageDef;
				Thing instigator5 = launcher;
				int damageAmount5 = DamageAmount;
				float armorPenetration5 = ArmorPenetration;
				SoundDef soundExplode5 = def.projectile.soundExplode;
				ThingDef weapon5 = equipmentDef;
				ThingDef projectile5 = def;
				Thing thing5 = intendedTarget.Thing;
				ThingDef postExplosionSpawnThingDef5 = def.projectile.postExplosionSpawnThingDef;
				postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
				float postExplosionSpawnChance5 = def.projectile.postExplosionSpawnChance;
				int postExplosionSpawnThingCount5 = def.projectile.postExplosionSpawnThingCount;
				GasType? postExplosionGasType5 = def.projectile.postExplosionGasType;
				preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
				preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
				preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, screenShakeFactor: def.projectile.screenShakeFactor, center: position7, map: map, radius: explosionRadius4, damType: damageDef5, instigator: instigator5, damAmount: damageAmount5, armorPenetration: armorPenetration5, explosionSound: soundExplode5, weapon: weapon5, projectile: projectile5, intendedTarget: thing5, postExplosionSpawnThingDef: postExplosionSpawnThingDef5, postExplosionSpawnChance: postExplosionSpawnChance5, postExplosionSpawnThingCount: postExplosionSpawnThingCount5, postExplosionGasType: postExplosionGasType5, affectedAngle: new FloatRange(-180f, -360f + num + 30f), doVisualEffects: true, excludeRadius: 0f, doSoundEffects: true, postExplosionSpawnThingDefWater: postExplosionSpawnThingDefWater);
			}
		}
		else
		{
			IntVec3 position8 = base.Position;
			float explosionRadius5 = def.projectile.explosionRadius;
			DamageDef damageDef6 = def.projectile.damageDef;
			Thing instigator6 = launcher;
			int damageAmount6 = DamageAmount;
			float armorPenetration6 = ArmorPenetration;
			SoundDef soundExplode6 = def.projectile.soundExplode;
			ThingDef weapon6 = equipmentDef;
			ThingDef projectile6 = def;
			Thing thing6 = intendedTarget.Thing;
			ThingDef postExplosionSpawnThingDef6 = def.projectile.postExplosionSpawnThingDef;
			preExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDefWater;
			float postExplosionSpawnChance6 = def.projectile.postExplosionSpawnChance;
			int postExplosionSpawnThingCount6 = def.projectile.postExplosionSpawnThingCount;
			GasType? postExplosionGasType6 = def.projectile.postExplosionGasType;
			postExplosionSpawnThingDefWater = def.projectile.preExplosionSpawnThingDef;
			screenShakeFactor = def.projectile.preExplosionSpawnChance;
			preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
			GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, screenShakeFactor: def.projectile.screenShakeFactor, center: position8, map: map, radius: explosionRadius5, damType: damageDef6, instigator: instigator6, damAmount: damageAmount6, armorPenetration: armorPenetration6, explosionSound: soundExplode6, weapon: weapon6, projectile: projectile6, intendedTarget: thing6, postExplosionSpawnThingDef: postExplosionSpawnThingDef6, postExplosionSpawnChance: postExplosionSpawnChance6, postExplosionSpawnThingCount: postExplosionSpawnThingCount6, postExplosionGasType: postExplosionGasType6, affectedAngle: new FloatRange(num - 30f, num + 30f), doVisualEffects: true, excludeRadius: 0f, doSoundEffects: true, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
		}
	}
}
