using System.Collections.Generic;
using System.Linq;
using Rimatomics;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.Compatibility;

internal class Rimatomics : IPatch
{
	public static SoundDef HitSoundDef;

	public static List<ThingComp> shields;

	public static bool found;

	public static int lastCacheTick;

	public static Map lastCacheMap;

	public bool CanInstall()
	{
		return ModLister.HasActiveModWithName("Dubs Rimatomics");
	}

	public void Install()
	{
		BlockerRegistry.RegisterCheckForCollisionBetweenCallback(CheckForCollisionBetweenCallback);
		BlockerRegistry.RegisterImpactSomethingCallback(ImpactSomethingCallback);
		BlockerRegistry.RegisterShieldZonesCallback(ShieldZonesCallback);
	}

	public static bool CheckForCollisionBetweenCallback(ProjectileCE projectile, Vector3 from, Vector3 to)
	{
		Map map = projectile.Map;
		getShields(map);
		if (!found)
		{
			return false;
		}
		Vector3 exactPosition = projectile.ExactPosition;
		IntVec3 originIV = projectile.OriginIV3;
		Quaternion exactRotation = projectile.ExactRotation;
		if (projectile.launcher == null)
		{
			return false;
		}
		foreach (ThingComp shield in shields)
		{
			CompRimatomicsShield val = (CompRimatomicsShield)(object)((shield is CompRimatomicsShield) ? shield : null);
			if (!val.Active || val.ShieldState != 0 || (!projectile.launcher.HostileTo(((ThingComp)(object)val).parent) && !val.debugInterceptNonHostileProjectiles && !val.Props.interceptNonHostileProjectiles))
			{
				continue;
			}
			bool interceptOutgoingProjectiles = val.Props.interceptOutgoingProjectiles;
			int num = (int)val.Radius;
			Vector3 vector = new Vector3(((ThingComp)(object)val).parent.Position.x, 0f, ((ThingComp)(object)val).parent.Position.z);
			Vector3 p = from;
			Vector3 center = vector;
			float radius = num;
			Map map2 = map;
			if (!CE_Utility.IntersectionPoint(p, to, center, radius, out var sect, interceptOutgoingProjectiles, spherical: false, map2))
			{
				continue;
			}
			Vector3 vector2 = sect.OrderBy((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude).First();
			Quaternion b = Quaternion.LookRotation(new Vector3(from.x - vector.x, 0f, from.z - vector.z));
			float num2 = Quaternion.Angle(exactRotation, b);
			if (!(num2 > 90f || interceptOutgoingProjectiles))
			{
				continue;
			}
			int damageAmount = projectile.def.projectile.GetDamageAmount(projectile.launcher);
			exactPosition = BlockerRegistry.GetExactPosition(originIV.ToVector3(), projectile.ExactPosition, ((ThingComp)(object)val).parent.Position.ToVector3(), (num - 1) * (num - 1));
			FleckMakerCE.ThrowLightningGlow(exactPosition, map, 0.5f);
			projectile.ExactPosition = exactPosition;
			Effecter effecter = new Effecter(val.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
			effecter.Trigger(new TargetInfo(exactPosition.ToIntVec3(), ((ThingComp)(object)val).parent.Map), TargetInfo.Invalid);
			effecter.Cleanup();
			val.energy -= (float)damageAmount * val.EnergyLossPerDamage;
			if (val.energy < 0f)
			{
				DamageInfo damageInfo = new DamageInfo(projectile.def.projectile.damageDef, damageAmount);
				val.BreakShield(damageInfo);
			}
			projectile.InterceptProjectile(val, exactPosition, destroyCompletely: true);
			return true;
		}
		return false;
	}

	public static bool ImpactSomethingCallback(ProjectileCE projectile, Thing launcher)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		if (!projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		Map map = projectile.Map;
		getShields(map);
		Vector3 exactPosition = projectile.ExactPosition;
		foreach (CompRimatomicsShield shield in shields)
		{
			CompRimatomicsShield val = shield;
			if (!val.Active || val.ShieldState != 0 || (!projectile.launcher.HostileTo(((ThingComp)(object)val).parent) && !val.debugInterceptNonHostileProjectiles && !val.Props.interceptNonHostileProjectiles))
			{
				continue;
			}
			int num = (int)val.Radius;
			int num2 = num * num;
			float num3 = projectile.Position.DistanceToSquared(((ThingComp)(object)val).parent.Position) - num2;
			if (num3 > 0f)
			{
				continue;
			}
			int damageAmount = projectile.def.projectile.GetDamageAmount(launcher);
			Effecter effecter = new Effecter(val.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile);
			effecter.Trigger(new TargetInfo(projectile.Position, ((ThingComp)(object)val).parent.Map), TargetInfo.Invalid);
			effecter.Cleanup();
			val.energy -= (float)damageAmount * val.EnergyLossPerDamage;
			if (val.energy < 0f)
			{
				DamageInfo damageInfo = new DamageInfo(projectile.def.projectile.damageDef, damageAmount);
				val.BreakShield(damageInfo);
			}
			projectile.InterceptProjectile(val, projectile.ExactPosition, destroyCompletely: true);
			return true;
		}
		return false;
	}

	private static IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing pawnToSuppress)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		Map map = pawnToSuppress.Map;
		getShields(map);
		List<IEnumerable<IntVec3>> list = new List<IEnumerable<IntVec3>>();
		foreach (CompRimatomicsShield shield in shields)
		{
			CompRimatomicsShield val = shield;
			if (val.Active && val.ShieldState == ShieldState.Active && (!pawnToSuppress.HostileTo(((ThingComp)(object)val).parent) || val.debugInterceptNonHostileProjectiles || val.Props.interceptNonHostileProjectiles))
			{
				int num = (int)val.Radius;
				list.Add(GenRadial.RadialCellsAround(((ThingComp)(object)val).parent.Position, num, useCenter: true));
			}
		}
		return list;
	}

	public static void getShields(Map map)
	{
		int ticksAbs = Find.TickManager.TicksAbs;
		if (lastCacheTick == ticksAbs && lastCacheMap == map)
		{
			return;
		}
		found = false;
		IEnumerable<Building> enumerable = map.listerBuildings.allBuildingsColonist.Where((Building b) => b is Building_ShieldArray);
		shields = new List<ThingComp>();
		foreach (Building item in enumerable)
		{
			found = true;
			shields.Add((ThingComp)(object)((Building_ShieldArray)((item is Building_ShieldArray) ? item : null)).CompShield);
		}
		lastCacheTick = ticksAbs;
		lastCacheMap = map;
	}
}
