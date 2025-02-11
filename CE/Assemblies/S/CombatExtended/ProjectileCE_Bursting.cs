using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

internal class ProjectileCE_Bursting : ProjectileCE
{
	private int ticksToBurst;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToBurst, "ticksToBurst", -1);
	}

	public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0f, float shotSpeed = -1f, Thing equipment = null, float distance = -1f)
	{
		int num = 0;
		if (def.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			num = projectilePropertiesCE.armingDelay;
			castShadow = projectilePropertiesCE.castShadow;
		}
		if (distance > 0f)
		{
			float num2 = (float)Math.Cos(shotAngle);
			float num3 = distance / (shotSpeed / 60f * num2);
			ticksToBurst = (int)num3;
		}
		if (ticksToBurst < num)
		{
			ticksToBurst = num;
		}
		base.shotAngle = shotAngle;
		base.shotHeight = shotHeight;
		base.shotRotation = shotRotation;
		base.shotSpeed = Math.Max(shotSpeed, def.projectile.speed);
		ticksToTruePosition = (def.projectile as ProjectilePropertiesCE).TickToTruePos;
		Launch(launcher, origin, equipment);
	}

	public override void Tick()
	{
		base.Tick();
		if (--ticksToBurst == 0)
		{
			base.Impact(null);
		}
	}
}
