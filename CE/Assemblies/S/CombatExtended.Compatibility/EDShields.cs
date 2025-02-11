using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jaxxa.EnhancedDevelopment.Shields.Shields;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended.Compatibility;

internal class EDShields : IPatch
{
	public static SoundDef HitSoundDef;

	public static List<Building> shields;

	public static int lastCacheTick;

	public static Map lastCacheMap;

	public bool CanInstall()
	{
		if (!ModLister.HasActiveModWithName("ED-Shields"))
		{
			return false;
		}
		return true;
	}

	public void Install()
	{
		BlockerRegistry.RegisterCheckForCollisionBetweenCallback(CheckForCollisionBetweenCallback);
		BlockerRegistry.RegisterImpactSomethingCallback(ImpactSomethingCallback);
		BlockerRegistry.RegisterShieldZonesCallback(ShieldZonesCallback);
		Type type = Type.GetType("Jaxxa.EnhancedDevelopment.Shields.Shields.ShieldManagerMapComp, ED-Shields");
		HitSoundDef = (SoundDef)type.GetField("HitSoundDef", BindingFlags.Static | BindingFlags.Public).GetValue(null);
	}

	public static bool CheckForCollisionBetweenCallback(ProjectileCE projectile, Vector3 from, Vector3 to)
	{
		if (projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		Thing launcher = projectile.launcher;
		Map map = projectile.Map;
		Vector3 exactPosition = projectile.ExactPosition;
		IntVec3 originIV = projectile.OriginIV3;
		Quaternion exactRotation = projectile.ExactRotation;
		getShields(map);
		foreach (Building shield in shields)
		{
			Building_Shield val = (Building_Shield)(object)((shield is Building_Shield) ? shield : null);
			Comp_ShieldGenerator comp = ((ThingWithComps)(object)val).GetComp<Comp_ShieldGenerator>();
			if (!comp.IsActive() || !comp.BlockDirect_Active())
			{
				continue;
			}
			int num = comp.FieldRadius_Active();
			Vector3 vector = new Vector3(((Thing)(object)val).Position.x, 0f, ((Thing)(object)val).Position.z);
			if (CE_Utility.IntersectionPoint(from, to, vector, num, out var sect, catchOutbound: false, spherical: false, map))
			{
				Vector3 vector2 = sect.OrderBy((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude).First();
				Quaternion b = Quaternion.LookRotation(from - vector);
				if (Quaternion.Angle(exactRotation, b) > 90f)
				{
					HitSoundDef.PlayOneShot(new TargetInfo(((Thing)(object)val).Position, map));
					int damageAmount = projectile.def.projectile.GetDamageAmount(launcher);
					comp.FieldIntegrity_Current -= damageAmount;
					exactPosition = vector2;
					FleckMakerCE.ThrowLightningGlow(exactPosition, map, 0.5f);
					projectile.InterceptProjectile(val, exactPosition);
					return true;
				}
			}
		}
		return false;
	}

	public static bool ImpactSomethingCallback(ProjectileCE projectile, Thing launcher)
	{
		if (!projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		Map map = projectile.Map;
		getShields(map);
		Vector3 exactPosition = projectile.ExactPosition;
		foreach (Building shield in shields)
		{
			Building_Shield val = (Building_Shield)(object)((shield is Building_Shield) ? shield : null);
			Comp_ShieldGenerator comp = ((ThingWithComps)(object)val).GetComp<Comp_ShieldGenerator>();
			if (comp.IsActive() && comp.BlockIndirect_Active())
			{
				int num = comp.FieldRadius_Active();
				int num2 = num * num;
				float num3 = projectile.Position.DistanceToSquared(((Thing)(object)val).Position) - num2;
				if (!(num3 > 0f))
				{
					HitSoundDef.PlayOneShot(new TargetInfo(((Thing)(object)val).Position, map));
					FleckMakerCE.ThrowLightningGlow(exactPosition, map, 0.5f);
					int damageAmount = projectile.def.projectile.GetDamageAmount(launcher);
					comp.FieldIntegrity_Current -= damageAmount;
					projectile.InterceptProjectile(val, projectile.ExactPosition, destroyCompletely: true);
					return true;
				}
			}
		}
		return false;
	}

	public static void getShields(Map map)
	{
		int ticksAbs = Find.TickManager.TicksAbs;
		if (lastCacheTick != ticksAbs || lastCacheMap != map)
		{
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			shields = allBuildingsColonist.Where((Building b) => b is Building_Shield).ToList();
			lastCacheTick = ticksAbs;
			lastCacheMap = map;
		}
	}

	private static IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing pawnToSuppress)
	{
		Map map = pawnToSuppress.Map;
		getShields(map);
		List<IEnumerable<IntVec3>> list = new List<IEnumerable<IntVec3>>();
		foreach (Building shield in shields)
		{
			Building_Shield val = (Building_Shield)(object)((shield is Building_Shield) ? shield : null);
			Comp_ShieldGenerator comp = ((ThingWithComps)(object)val).GetComp<Comp_ShieldGenerator>();
			if (comp.IsActive() && comp.BlockDirect_Active())
			{
				int num = comp.FieldRadius_Active();
				list.Add(GenRadial.RadialCellsAround(((Thing)(object)val).Position, num, useCenter: true));
			}
		}
		return list;
	}
}
