using AncotLibrary;
using UnityEngine;
using Verse;

namespace Milira;

public class RailGunBullet : Projectile_Custom
{
	public override bool AnimalsFleeImpact => true;

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		Find.CameraDriver.shaker.DoShake(2f);
		Map map = base.Map;
		IntVec3 position = launcher.Position;
		IntVec3 position2 = base.Position;
		Vector3 v = (position2 - position).ToVector3();
		v.Normalize();
		float num = v.ToAngleFlat();
		if (hitThing != null)
		{
			position2 = hitThing.Position;
		}
		IntVec3 center = position2;
		DamageDef milira_KineticBomb = MiliraDefOf.Milira_KineticBomb;
		Thing instigator = launcher;
		int damAmount = DamageAmount / 2;
		float armorPenetration = ArmorPenetration;
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
		float screenShakeFactor = def.projectile.screenShakeFactor;
		GenExplosion.DoExplosion(center, map, 1.5f, milira_KineticBomb, instigator, damAmount, armorPenetration, null, weapon, projectile, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef2, preExplosionSpawnChance2, preExplosionSpawnThingCount2, explosionChanceToStartFire, explosionDamageFalloff, direction, null, null, doVisualEffects: false, 1f, 0f, doSoundEffects: false, postExplosionSpawnThingDefWater, screenShakeFactor);
		if (num - 15f < -180f || num + 15f > 180f)
		{
			if (num < 0f)
			{
				IntVec3 center2 = position2;
				DamageDef milira_KineticBomb2 = MiliraDefOf.Milira_KineticBomb;
				Thing instigator2 = launcher;
				int damAmount2 = DamageAmount / 2;
				float armorPenetration2 = ArmorPenetration;
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
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, screenShakeFactor: def.projectile.screenShakeFactor, center: center2, map: map, radius: 6f, damType: milira_KineticBomb2, instigator: instigator2, damAmount: damAmount2, armorPenetration: armorPenetration2, explosionSound: null, weapon: weapon2, projectile: projectile2, intendedTarget: thing2, postExplosionSpawnThingDef: postExplosionSpawnThingDef2, postExplosionSpawnChance: postExplosionSpawnChance2, postExplosionSpawnThingCount: postExplosionSpawnThingCount2, postExplosionGasType: postExplosionGasType2, affectedAngle: new FloatRange(-180f, num + 15f), doVisualEffects: false, propagationSpeed: 1f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
				IntVec3 center3 = position2;
				DamageDef milira_KineticBomb3 = MiliraDefOf.Milira_KineticBomb;
				Thing instigator3 = launcher;
				int damAmount3 = DamageAmount / 2;
				float armorPenetration3 = ArmorPenetration;
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
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, screenShakeFactor: def.projectile.screenShakeFactor, center: center3, map: map, radius: 6f, damType: milira_KineticBomb3, instigator: instigator3, damAmount: damAmount3, armorPenetration: armorPenetration3, explosionSound: null, weapon: weapon3, projectile: projectile3, intendedTarget: thing3, postExplosionSpawnThingDef: postExplosionSpawnThingDef3, postExplosionSpawnChance: postExplosionSpawnChance3, postExplosionSpawnThingCount: postExplosionSpawnThingCount3, postExplosionGasType: postExplosionGasType3, affectedAngle: new FloatRange(360f + num - 15f, 180f), doVisualEffects: false, propagationSpeed: 1f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: postExplosionSpawnThingDefWater);
			}
			else
			{
				IntVec3 center4 = position2;
				float explosionRadius = def.projectile.explosionRadius;
				DamageDef milira_KineticBomb4 = MiliraDefOf.Milira_KineticBomb;
				Thing instigator4 = launcher;
				int damAmount4 = DamageAmount / 2;
				float armorPenetration4 = ArmorPenetration;
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
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, screenShakeFactor: def.projectile.screenShakeFactor, center: center4, map: map, radius: explosionRadius, damType: milira_KineticBomb4, instigator: instigator4, damAmount: damAmount4, armorPenetration: armorPenetration4, explosionSound: null, weapon: weapon4, projectile: projectile4, intendedTarget: thing4, postExplosionSpawnThingDef: postExplosionSpawnThingDef4, postExplosionSpawnChance: postExplosionSpawnChance4, postExplosionSpawnThingCount: postExplosionSpawnThingCount4, postExplosionGasType: postExplosionGasType4, affectedAngle: new FloatRange(num - 15f, 180f), doVisualEffects: false, propagationSpeed: 3f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
				IntVec3 center5 = position2;
				DamageDef milira_KineticBomb5 = MiliraDefOf.Milira_KineticBomb;
				Thing instigator5 = launcher;
				int damAmount5 = DamageAmount / 2;
				float armorPenetration5 = ArmorPenetration;
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
				GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, screenShakeFactor: def.projectile.screenShakeFactor, center: center5, map: map, radius: 6f, damType: milira_KineticBomb5, instigator: instigator5, damAmount: damAmount5, armorPenetration: armorPenetration5, explosionSound: null, weapon: weapon5, projectile: projectile5, intendedTarget: thing5, postExplosionSpawnThingDef: postExplosionSpawnThingDef5, postExplosionSpawnChance: postExplosionSpawnChance5, postExplosionSpawnThingCount: postExplosionSpawnThingCount5, postExplosionGasType: postExplosionGasType5, affectedAngle: new FloatRange(-180f, -360f + num + 15f), doVisualEffects: false, propagationSpeed: 1f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: postExplosionSpawnThingDefWater);
			}
		}
		else
		{
			IntVec3 center6 = position2;
			DamageDef milira_KineticBomb6 = MiliraDefOf.Milira_KineticBomb;
			Thing instigator6 = launcher;
			int damAmount6 = DamageAmount / 2;
			float armorPenetration6 = ArmorPenetration;
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
			GenExplosion.DoExplosion(applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: postExplosionSpawnThingDefWater, preExplosionSpawnChance: screenShakeFactor, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, screenShakeFactor: def.projectile.screenShakeFactor, center: center6, map: map, radius: 6f, damType: milira_KineticBomb6, instigator: instigator6, damAmount: damAmount6, armorPenetration: armorPenetration6, explosionSound: null, weapon: weapon6, projectile: projectile6, intendedTarget: thing6, postExplosionSpawnThingDef: postExplosionSpawnThingDef6, postExplosionSpawnChance: postExplosionSpawnChance6, postExplosionSpawnThingCount: postExplosionSpawnThingCount6, postExplosionGasType: postExplosionGasType6, affectedAngle: new FloatRange(num - 15f, num + 15f), doVisualEffects: false, propagationSpeed: 1f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: preExplosionSpawnThingDef);
		}
		base.Impact(hitThing, blockedByShield);
	}
}
