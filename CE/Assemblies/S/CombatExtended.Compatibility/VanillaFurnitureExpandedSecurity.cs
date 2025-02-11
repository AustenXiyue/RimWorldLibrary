using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;
using VFESecurity;

namespace CombatExtended.Compatibility;

public class VanillaFurnitureExpandedSecurity : IPatch
{
	private static int lastCacheTick;

	private static Map lastCacheMap;

	private static HashSet<Building> shields;

	private const string VFES_ModName = "Vanilla Furniture Expanded - Security";

	private static FastInvokeHandler CanFunctionPropertyGetter;

	public bool CanInstall()
	{
		if (!ModLister.HasActiveModWithName("Vanilla Furniture Expanded - Security"))
		{
			return false;
		}
		return true;
	}

	public void Install()
	{
		Type typeFromHandle = typeof(Building_Shield);
		if (typeFromHandle != null)
		{
			CanFunctionPropertyGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(typeof(Building_Shield), "CanFunction"), false);
		}
		BlockerRegistry.RegisterCheckForCollisionCallback(CheckCollision);
		BlockerRegistry.RegisterImpactSomethingCallback(ImpactSomething);
	}

	private static bool CheckCollision(ProjectileCE projectile, IntVec3 cell, Thing launcher)
	{
		if (projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		Map map = projectile.Map;
		Vector3 exactPosition = projectile.ExactPosition;
		refreshShields(map);
		foreach (Building shield in shields)
		{
			Building_Shield val = (Building_Shield)(object)((shield is Building_Shield) ? shield : null);
			if (!ShieldInterceptsProjectile((Building)(object)val, projectile, launcher))
			{
				continue;
			}
			exactPosition = BlockerRegistry.GetExactPosition(projectile.OriginIV3.ToVector3(), exactPosition, new Vector3(((Thing)(object)val).Position.x, 0f, ((Thing)(object)val).Position.z), val.ShieldRadius * val.ShieldRadius);
			if (!(projectile is ProjectileCE_Explosive))
			{
				val.AbsorbDamage((float)projectile.def.projectile.GetDamageAmount(launcher), projectile.def.projectile.damageDef, projectile.ExactRotation.eulerAngles.y);
			}
			projectile.InterceptProjectile(val, exactPosition, destroyCompletely: true);
			return true;
		}
		return false;
	}

	private static bool ImpactSomething(ProjectileCE projectile, Thing launcher)
	{
		Map map = projectile.Map;
		Vector3 exactPosition = projectile.ExactPosition;
		refreshShields(map);
		foreach (Building shield in shields)
		{
			Building_Shield val = (Building_Shield)(object)((shield is Building_Shield) ? shield : null);
			if (val != null && ShieldInterceptsProjectile((Building)(object)val, projectile, launcher))
			{
				projectile.InterceptProjectile(val, exactPosition, destroyCompletely: true);
				return true;
			}
		}
		return false;
	}

	private static bool ShieldInterceptsProjectile(Building building, ProjectileCE projectile, Thing launcher)
	{
		Building_Shield val = (Building_Shield)(object)((building is Building_Shield) ? building : null);
		if (!val.active || !(bool)CanFunctionPropertyGetter.Invoke((object)val, Array.Empty<object>()) || val.Energy == 0f)
		{
			return false;
		}
		if (val.coveredCells.Contains(launcher.Position))
		{
			return false;
		}
		if (!val.coveredCells.Contains(projectile.Position))
		{
			return false;
		}
		return true;
	}

	private static void refreshShields(Map map)
	{
		int ticksAbs = Find.TickManager.TicksAbs;
		if (lastCacheTick != ticksAbs || lastCacheMap != map)
		{
			IEnumerable<Building> listerShieldGens = (IEnumerable<Building>)map.GetComponent<ListerThingsExtended>().listerShieldGens;
			shields = listerShieldGens.ToHashSet();
			lastCacheTick = ticksAbs;
			lastCacheMap = map;
		}
	}
}
