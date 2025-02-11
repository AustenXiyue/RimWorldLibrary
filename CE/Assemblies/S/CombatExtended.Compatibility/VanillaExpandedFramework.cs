using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFECore;

namespace CombatExtended.Compatibility;

public class VanillaExpandedFramework : IPatch
{
	private const string ModName = "Vanilla Expanded Framework";

	bool IPatch.CanInstall()
	{
		return ModLister.HasActiveModWithName("Vanilla Expanded Framework");
	}

	void IPatch.Install()
	{
		BlockerRegistry.RegisterCheckForCollisionCallback(CheckIntercept);
		BlockerRegistry.RegisterShieldZonesCallback(ShieldZonesCallback);
	}

	private IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing pawnToSuppress)
	{
		IEnumerable<CompShieldField> enumerable = CompShieldField.ListerShieldGensActiveIn(pawnToSuppress.Map).ToList();
		List<IEnumerable<IntVec3>> list = new List<IEnumerable<IntVec3>>();
		if (!enumerable.Any())
		{
			return list;
		}
		foreach (CompShieldField item in enumerable)
		{
			if (item.CanFunction)
			{
				list.Add(GenRadial.RadialCellsAround(item.HostThing.Position, item.ShieldRadius, useCenter: true));
			}
		}
		return list;
	}

	private static bool CheckIntercept(ProjectileCE projectile, IntVec3 cell, Thing launcher)
	{
		if (projectile.def.projectile.flyOverhead)
		{
			return false;
		}
		IEnumerable<CompShieldField> enumerable = CompShieldField.ListerShieldGensActiveIn(projectile.Map).ToList();
		if (!enumerable.Any())
		{
			return false;
		}
		ThingDef def = projectile.def;
		Vector3 p = projectile.LastPos.Yto0();
		Vector3 p2 = projectile.ExactPosition.Yto0();
		foreach (CompShieldField item in enumerable)
		{
			if (!item.CanFunction)
			{
				continue;
			}
			Vector3 center = item.HostThing.Position.ToVector3Shifted().Yto0();
			float shieldRadius = item.ShieldRadius;
			if (!CE_Utility.IntersectionPoint(p, p2, center, shieldRadius, out var sect, catchOutbound: false))
			{
				continue;
			}
			projectile.ExactPosition = sect.OrderBy((Vector3 x) => (projectile.OriginIV3.ToVector3() - x).sqrMagnitude).First();
			projectile.landed = true;
			projectile.InterceptProjectile(item.HostThing, projectile.ExactPosition, destroyCompletely: true);
			item.AbsorbDamage(projectile.DamageAmount, projectile.def.projectile.damageDef, launcher);
			return true;
		}
		return false;
	}
}
