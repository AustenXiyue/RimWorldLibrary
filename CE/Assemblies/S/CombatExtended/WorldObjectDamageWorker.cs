using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.WorldObjects;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class WorldObjectDamageWorker
{
	protected const float fragDamageMultipler = 0.04f;

	protected static Map map;

	protected HashSet<IntVec3> addedCellsAffectedOnlyByDamage = new HashSet<IntVec3>();

	public virtual float ApplyDamage(HealthComp healthComp, ThingDef shellDef)
	{
		float num = CalculateDamage(shellDef.GetProjectile(), healthComp.parent.Faction) / healthComp.ArmorDamageMultiplier;
		healthComp.Health -= num;
		return num;
	}

	public virtual float CalculateDamage(ThingDef projectile, Faction faction)
	{
		float modifier = 0.03f;
		if (faction != null)
		{
			if ((int)faction.def.techLevel > 3)
			{
				modifier = Mathf.Pow(3.3f, (int)faction.def.techLevel) / 1800f / ((float)(int)faction.def.techLevel - 2f);
			}
			if (faction.def.pawnGroupMakers.SelectMany((PawnGroupMaker x) => x.options).All((PawnGenOption k) => k.kind.RaceProps.IsMechanoid))
			{
				modifier = 1f;
			}
		}
		float num = FragmentsPotentialDamage(projectile) + FirePotentialDamage(projectile) + EMPPotentialDamage(projectile, modifier) + OtherPotentialDamage(projectile);
		num /= 3500f;
		if (projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE && projectilePropertiesCE.shellingProps.damage > 0f)
		{
			num = projectilePropertiesCE.shellingProps.damage;
		}
		return num * Rand.Range(0.4f, 1.5f);
	}

	protected virtual float FragmentsPotentialDamage(ThingDef projectile)
	{
		float num = 0f;
		CompProperties_Fragments compProperties = projectile.GetCompProperties<CompProperties_Fragments>();
		if (projectile.projectile is ProjectilePropertiesCE && compProperties != null)
		{
			foreach (ThingDefCountClass fragment in compProperties.fragments)
			{
				num += (float)(fragment.count * fragment.thingDef.projectile.damageAmountBase) * 0.04f;
			}
		}
		return num;
	}

	protected virtual float EMPPotentialDamage(ThingDef projectile, float modifier = 0.03f)
	{
		float num = 0f;
		if (projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE && projectilePropertiesCE.damageDef == DamageDefOf.EMP)
		{
			num += (float)((ProjectileProperties)projectilePropertiesCE).damageAmountBase * modifier;
			for (int i = 1; (float)i < projectilePropertiesCE.explosionRadius; i++)
			{
				num += modifier * DamageAtRadius(projectile, i) * Mathf.Pow(2f, i);
			}
		}
		return num;
	}

	protected virtual float FirePotentialDamage(ThingDef projectile)
	{
		float num = 0f;
		if (projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE && projectilePropertiesCE.damageDef == CE_DamageDefOf.PrometheumFlame)
		{
			num += (float)((ProjectileProperties)projectilePropertiesCE).damageAmountBase;
			for (int i = 1; (float)i < projectilePropertiesCE.explosionRadius; i++)
			{
				num += DamageAtRadius(projectile, i) * Mathf.Pow(2f, i);
			}
			if (projectilePropertiesCE.preExplosionSpawnThingDef == CE_ThingDefOf.FilthPrometheum)
			{
				num += projectilePropertiesCE.preExplosionSpawnChance * ((float)Math.PI * projectilePropertiesCE.explosionRadius * projectilePropertiesCE.explosionRadius) * 3f;
			}
		}
		return num;
	}

	protected virtual float OtherPotentialDamage(ThingDef projectile)
	{
		float num = 0f;
		if (projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			if (projectilePropertiesCE.damageDef == DamageDefOf.EMP || projectilePropertiesCE.damageDef == CE_DamageDefOf.PrometheumFlame)
			{
				return 0f;
			}
			num += (float)((ProjectileProperties)projectilePropertiesCE).damageAmountBase;
			for (int i = 1; (float)i < projectilePropertiesCE.explosionRadius; i++)
			{
				num += DamageAtRadius(projectile, i) * Mathf.Pow(2f, i);
			}
			DamageDefExtensionCE modExtension = projectilePropertiesCE.damageDef.GetModExtension<DamageDefExtensionCE>();
			if (modExtension != null && modExtension.worldDamageMultiplier >= 0f)
			{
				num *= modExtension.worldDamageMultiplier;
			}
		}
		return num;
	}

	public static float DamageAtRadius(ThingDef projectile, int radius)
	{
		if (!projectile.projectile.explosionDamageFalloff)
		{
			return projectile.projectile.damageAmountBase;
		}
		float t = (float)radius / projectile.projectile.explosionRadius;
		return Mathf.Max(GenMath.RoundRandom(Mathf.Lerp(projectile.projectile.damageAmountBase, (float)projectile.projectile.damageAmountBase * 0.2f, t)), 1);
	}

	public static void BeginAttrition(Map map)
	{
		WorldObjectDamageWorker.map = map;
	}

	public static void EndAttrition()
	{
		map = null;
	}

	public virtual void OnExploded(IntVec3 cell, ThingDef shell)
	{
	}

	public virtual void ProcessShell(ThingDef shell)
	{
		int num = (int)shell.projectile.explosionRadius;
		int damageAmountBase = shell.projectile.damageAmountBase;
		int num2 = 0;
		IntVec3 intVec;
		RoofDef roof;
		do
		{
			intVec = new IntVec3((int)CE_Utility.RandomGaussian(1f, map.Size.x - 1), 0, (int)CE_Utility.RandomGaussian(1f, map.Size.z - 1));
			roof = intVec.GetRoof(map);
			num2++;
		}
		while (num2 <= 7 && roof == RoofDefOf.RoofRockThick);
		if (roof != RoofDefOf.RoofRockThick && TryExplode(intVec, shell))
		{
			OnExploded(intVec, shell);
		}
	}

	protected virtual bool TryExplode(IntVec3 centerCell, ThingDef shellDef)
	{
		int num = (int)shellDef.GetProjectile().projectile.explosionRadius;
		if (!centerCell.InBounds(map))
		{
			return false;
		}
		ProcessFragmentsComp(shellDef);
		DamageToPawns(shellDef);
		IEnumerable<IntVec3> enumerable = ExplosionCellsToHit(centerCell, map, num, null, null, null);
		foreach (IntVec3 item in enumerable)
		{
			List<Thing> list = item.GetThingList(map).Except(map.mapPawns.AllPawns).ToList();
			if (Controller.settings.DebugDisplayAttritionInfo)
			{
				map.debugDrawer.FlashCell(item, 0f, null, 1000);
			}
			bool flag = false;
			int num2 = (int)DamageAtRadius(shellDef, (int)centerCell.DistanceTo(item));
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.def.useHitPoints)
				{
					continue;
				}
				thing.hitPointsInt -= num2 * ((!thing.IsPlant()) ? 1 : 3);
				if (thing.hitPointsInt > 0)
				{
					if (!flag && Rand.Chance(0.5f))
					{
						ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_RubbleBuilding);
						flag = true;
					}
					if (Rand.Chance(0.1f))
					{
						FireUtility.TryStartFireIn(item, map, Rand.Range(0.5f, 1.5f), null);
					}
					continue;
				}
				thing.DeSpawn();
				thing.Destroy();
				if (thing.def.MakeFog)
				{
					map.fogGrid.Notify_FogBlockerRemoved(thing);
				}
				ThingDef filthDef = thing.def.filthLeaving ?? (Rand.Chance(0.5f) ? ThingDefOf.Filth_Ash : ThingDefOf.Filth_RubbleBuilding);
				if (!flag && FilthMaker.TryMakeFilth(item, map, filthDef, Rand.Range(1, 3), FilthSourceFlags.Any))
				{
					flag = true;
				}
			}
			map.snowGrid.SetDepth(item, 0f);
			map.roofGrid.SetRoof(item, null);
			if (Rand.Chance(0.33f) && map.terrainGrid.CanRemoveTopLayerAt(item))
			{
				map.terrainGrid.RemoveTopLayer(item, doLeavings: false);
			}
		}
		return true;
	}

	protected virtual void DamageToPawns(ThingDef shellDef)
	{
		ThingDef projectile = shellDef.GetProjectile();
		if (!Rand.Chance(0.05f))
		{
			return;
		}
		int num = Rand.Range(1, Math.Min(map.mapPawns.AllPawnsSpawnedCount, (int)projectile.projectile.explosionRadius));
		for (int i = 0; i < num; i++)
		{
			if (map.mapPawns.AllPawnsSpawned.Where((Pawn x) => !x.Faction.IsPlayerSafe()).ToList().TryRandomElementByWeight((Pawn x) => x.Faction.HostileTo(Faction.OfPlayer) ? 1f : 0.2f, out var result))
			{
				DamagePawn(result, projectile);
			}
		}
	}

	protected virtual void DamagePawn(Pawn pawn, ThingDef projDef)
	{
		BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, CE_RulePackDefOf.DamageEvent_ShellingExplosion);
		Find.BattleLog.Add(battleLogEntry_DamageTaken);
		float num = DamageAtRadius(projDef, Rand.Range(0, (int)projDef.projectile.explosionRadius));
		DamageInfo dinfo = new DamageInfo(projDef.projectile.damageDef, num);
		dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
		pawn.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
		ResetVisualDamageEffects(pawn);
	}

	public virtual IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius, IntVec3? needLOSToCell1 = null, IntVec3? needLOSToCell2 = null, FloatRange? affectedAngle = null)
	{
		bool flag = center.Roofed(map);
		bool flag2 = false;
		List<IntVec3> list = new List<IntVec3>();
		List<IntVec3> list2 = new List<IntVec3>();
		int num = GenRadial.NumCellsInRadius(radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[i];
			if (!intVec.InBounds(map))
			{
				continue;
			}
			if (flag2)
			{
				if ((!flag && GenSight.LineOfSight(center, intVec, map, skipFirstCell: false, null, 0, 0)) || !intVec.Roofed(map))
				{
					list.Add(intVec);
				}
			}
			else
			{
				if (!GenSight.LineOfSight(center, intVec, map, skipFirstCell: true))
				{
					continue;
				}
				if (needLOSToCell1.HasValue || needLOSToCell2.HasValue)
				{
					bool flag3 = needLOSToCell1.HasValue && GenSight.LineOfSight(needLOSToCell1.Value, intVec, map, skipFirstCell: false, null, 0, 0);
					bool flag4 = needLOSToCell2.HasValue && GenSight.LineOfSight(needLOSToCell2.Value, intVec, map, skipFirstCell: false, null, 0, 0);
					if (!flag3 && !flag4)
					{
						continue;
					}
				}
				list.Add(intVec);
			}
		}
		foreach (IntVec3 item in list)
		{
			if (!item.Walkable(map))
			{
				continue;
			}
			for (int j = 0; j < 4; j++)
			{
				IntVec3 intVec2 = item + GenAdj.CardinalDirections[j];
				if (intVec2.InHorDistOf(center, radius) && intVec2.InBounds(map) && !intVec2.Standable(map) && intVec2.GetEdifice(map) != null && !list.Contains(intVec2) && list2.Contains(intVec2))
				{
					list2.Add(intVec2);
				}
			}
		}
		return list.Concat(list2);
	}

	private void ProcessFragmentsComp(ThingDef shellDef)
	{
		ThingDef projectile = shellDef.GetProjectile();
		if (!projectile.HasComp(typeof(CompFragments)))
		{
			return;
		}
		CompProperties_Fragments compProperties = projectile.GetCompProperties<CompProperties_Fragments>();
		if (!Rand.Chance(0.33f))
		{
			return;
		}
		int num = Rand.Range(1, Math.Min(map.mapPawns.AllPawnsSpawnedCount, 5));
		for (int i = 0; i < num; i++)
		{
			if (!map.mapPawns.AllPawnsSpawned.Where((Pawn x) => !x.Faction.IsPlayerSafe()).ToList().TryRandomElementByWeight((Pawn x) => x.Faction.HostileTo(Faction.OfPlayer) ? 1f : 0.2f, out var result))
			{
				continue;
			}
			int num2 = Rand.Range(3, 9);
			for (int j = 0; j < num2; j++)
			{
				if (result.Map == null)
				{
					break;
				}
				ProjectileCE projectileCE = GenSpawn.Spawn(compProperties.fragments.RandomElementByWeight((ThingDefCountClass x) => x.count).thingDef, result.Position, result.Map) as ProjectileCE;
				projectileCE.Impact(result);
			}
			ResetVisualDamageEffects(result);
		}
	}

	public static void ResetVisualDamageEffects(Pawn pawn)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Pawn_DrawTracker drawer = pawn.drawer;
		(new Traverse((object)drawer).Field("jitterer").GetValue() as JitterHandler)?.ProcessPostTickVisuals(10000);
	}
}
