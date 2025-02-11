using System.Linq;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ProjectileImpactFX;

public static class ImpactFleckThrower
{
	public static void ThrowFleck(Vector3 loc, IntVec3 position, Map map, ProjectilePropertiesCE projProps, ThingDef def, Thing hitThing = null, float direction = 0f)
	{
		if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
		{
			return;
		}
		EffectProjectileExtension effectProjectileExtension = (def.HasModExtension<EffectProjectileExtension>() ? def.GetModExtension<EffectProjectileExtension>() : new EffectProjectileExtension());
		FleckDef fleckDef = effectProjectileExtension.explosionFleckDef;
		FleckDef fleckDef2 = effectProjectileExtension.ImpactFleckDef;
		FleckDef fleckDef3 = effectProjectileExtension.ImpactGlowFleckDef;
		SoundDef impactSoundDef = effectProjectileExtension.ImpactSoundDef;
		float num = effectProjectileExtension.explosionFleckSizeRange?.RandomInRange ?? effectProjectileExtension.explosionFleckSize;
		float num2 = effectProjectileExtension.ImpactFleckSizeRange?.RandomInRange ?? effectProjectileExtension.ImpactFleckSize;
		float scale = effectProjectileExtension.ImpactGlowFleckSizeRange?.RandomInRange ?? effectProjectileExtension.ImpactGlowFleckSize;
		Rand.PushState();
		float rotationRate = Rand.Range(-30f, 30f);
		float num3 = Rand.Range(0.5f, 1f);
		int num4 = 1;
		float value = 5f;
		float[] array = new float[3] { 0f, -60f, 60f };
		DamageDef damageDef = projProps.damageDef;
		if (effectProjectileExtension.AutoAssign)
		{
			if (!projProps.secondaryDamage.NullOrEmpty())
			{
				SecondaryDamage secondaryDamage = projProps.secondaryDamage.FirstOrDefault();
				if (secondaryDamage != null && (secondaryDamage.def == CE_DamageDefOf.Flame_Secondary || secondaryDamage.def == CE_DamageDefOf.PrometheumFlame))
				{
					fleckDef = FleckDefOf.MicroSparksFast;
					num = ScaleToRange(0.2f, 2f, 1f, 100f, secondaryDamage.amount);
					fleckDef3 = CE_FleckDefOf.Fleck_HeatGlow_API;
					scale = num * 4f;
				}
				else if (secondaryDamage != null && secondaryDamage.def == CE_DamageDefOf.Bomb_Secondary)
				{
					fleckDef = CE_FleckDefOf.BlastFlame;
					num = ScaleToRange(0.2f, 1.8f, 1f, 100f, secondaryDamage.amount);
					fleckDef3 = FleckDefOf.ExplosionFlash;
					scale = num * 7f;
				}
				else if (secondaryDamage.def == DamageDefOf.EMP)
				{
					fleckDef = CE_FleckDefOf.ElectricalSpark;
					num = ScaleToRange(0.5f, 4f, 6f, 30f, secondaryDamage.amount);
					fleckDef3 = CE_FleckDefOf.Fleck_ElectricGlow_EMP;
					scale = num2 * 3f;
				}
			}
			else if (damageDef == DamageDefOf.EMP && projProps.explosionRadius == 0f)
			{
				fleckDef = CE_FleckDefOf.ElectricalSpark;
				num = ScaleToRange(0.5f, 4f, 6f, 30f, ((ProjectileProperties)projProps).damageAmountBase);
				fleckDef3 = CE_FleckDefOf.Fleck_ElectricGlow_EMP;
				scale = num2 * 3f;
			}
		}
		if (effectProjectileExtension.CreateTerrainEffects)
		{
			if (hitThing == null)
			{
				TerrainDef terrain = position.GetTerrain(map);
				num2 = ScaleToRange(1f, 3f, 10f, 50f, ((ProjectileProperties)projProps).damageAmountBase);
				if (((ProjectileProperties)projProps).damageAmountBase > 30)
				{
					num4 = 3;
					num3 = Rand.Range(1.5f, 2f);
					num2 /= 2f;
				}
				if (terrain.takeFootprints)
				{
					fleckDef2 = FleckDefOf.DustPuffThick;
					num3 /= 2f;
					num2 = ScaleToRange(2f, 4f, 10f, 50f, ((ProjectileProperties)projProps).damageAmountBase);
				}
				else if (terrain.holdSnow && (damageDef == DamageDefOf.Bullet || damageDef == DamageDefOf.Cut))
				{
					fleckDef2 = FleckDefOf.AirPuff;
					TriggerBulletHole(loc, map, (((ProjectileProperties)projProps).damageAmountBase < 50) ? ScaleToRange(0.1f, 0.7f, 1f, 50f, ((ProjectileProperties)projProps).damageAmountBase) : 0.7f);
					TriggerScatteredSparks(loc, map, (((ProjectileProperties)projProps).damageAmountBase >= 10 && Rand.Chance(0.5f)) ? ((int)ScaleToRange(3f, 15f, 10f, 100f, ((ProjectileProperties)projProps).damageAmountBase)) : 2, (direction > 0f) ? (360f - direction) : (0f - direction));
				}
			}
			if (hitThing is Building)
			{
				float num5 = 0.3f * hitThing.DrawSize.x;
				float num6 = 0.3f * hitThing.DrawSize.y;
				TriggerBulletHole(hitThing.DrawPos + new Vector3(Rand.Range(0f - num5, num5), 0f, Rand.Range(0f - num6, num6)), map, (((ProjectileProperties)projProps).damageAmountBase < 50) ? ScaleToRange(0.1f, 0.7f, 1f, 50f, ((ProjectileProperties)projProps).damageAmountBase) : 0.7f);
			}
		}
		if (effectProjectileExtension.explosionEffecter != null)
		{
			TriggerEffect(effectProjectileExtension.explosionEffecter, loc, map);
		}
		if (fleckDef2 != null)
		{
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, fleckDef2);
			for (int i = 0; i < num4; i++)
			{
				dataStatic.rotation = Rand.Range(0, 360);
				dataStatic.velocityAngle = ((i < 3) ? array[i] : ((float)Rand.Range(-90, 90)));
				dataStatic.velocitySpeed = num3;
				dataStatic.scale = num2;
				dataStatic.spawnPosition = loc;
				dataStatic.spawnPosition.y += 3f;
				dataStatic.airTimeLeft = value;
				map.flecks.CreateFleck(dataStatic);
			}
		}
		if (fleckDef3 != null)
		{
			FleckCreationData dataStatic2 = FleckMaker.GetDataStatic(loc, map, fleckDef3);
			dataStatic2.scale = scale;
			map.flecks.CreateFleck(dataStatic2);
		}
		if (fleckDef != null)
		{
			FleckCreationData dataStatic3 = FleckMaker.GetDataStatic(loc, map, fleckDef);
			dataStatic3.scale = num;
			dataStatic3.rotationRate = rotationRate;
			dataStatic3.spawnPosition = loc;
			dataStatic3.instanceColor = projProps.damageDef.explosionColorCenter;
			map.flecks.CreateFleck(dataStatic3);
		}
		impactSoundDef?.PlayOneShot(new TargetInfo(loc.ToIntVec3(), map));
		Rand.PopState();
	}

	private static void TriggerBulletHole(Vector3 loc, Map map, float size)
	{
		FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, CE_FleckDefOf.Fleck_BulletHole);
		dataStatic.rotation = Rand.Range(0, 360);
		dataStatic.scale = size;
		dataStatic.spawnPosition = loc;
		map.flecks.CreateFleck(dataStatic);
	}

	private static void TriggerScatteredSparks(Vector3 loc, Map map, int count, float direction)
	{
		FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, CE_FleckDefOf.Fleck_SparkThrownFast);
		for (int i = 0; i < count; i++)
		{
			dataStatic.velocityAngle = direction + (float)Rand.Range(-45, 45);
			dataStatic.scale = 0.2f;
			dataStatic.velocitySpeed = Rand.Range(5, 10);
			dataStatic.spawnPosition = loc;
			dataStatic.airTimeLeft = 0.2f;
			map.flecks.CreateFleck(dataStatic);
		}
	}

	private static float ScaleToRange(float minNew, float maxNew, float minOld, float maxOld, float value)
	{
		return minNew + (value - minOld) / (maxOld - minOld) * (maxNew - minNew);
	}

	public static void TriggerEffect(EffecterDef effect, Vector3 position, Map map, Thing hitThing = null)
	{
		TriggerEffect(effect, IntVec3.FromVector3(position), map);
	}

	public static void TriggerEffect(EffecterDef effect, IntVec3 dest, Map map)
	{
		if (effect != null)
		{
			TargetInfo targetInfo = new TargetInfo(dest, map);
			Effecter effecter = effect.Spawn();
			effecter.Trigger(targetInfo, targetInfo);
			effecter.Cleanup();
		}
	}
}
