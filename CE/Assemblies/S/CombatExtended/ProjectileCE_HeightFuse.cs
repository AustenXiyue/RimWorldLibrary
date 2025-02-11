using Verse;

namespace CombatExtended;

internal class ProjectileCE_HeightFuse : ProjectileCE
{
	private bool armed;

	private float detonationHeight => (def.projectile as ProjectilePropertiesCE).aimHeightOffset;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref armed, "armed", defaultValue: false);
	}

	public override void Tick()
	{
		base.Tick();
		if (!armed && LastPos.y > detonationHeight)
		{
			armed = true;
		}
		if (armed && ExactPosition.y <= detonationHeight)
		{
			HeightFuseAirBurst();
		}
	}

	public override void Impact(Thing hitThing)
	{
		if (armed && ExactPosition.y <= detonationHeight)
		{
			HeightFuseAirBurst();
		}
		else
		{
			base.Impact(hitThing);
		}
	}

	private void HeightFuseAirBurst()
	{
		float num = (LastPos.y - detonationHeight) / (LastPos.y - ExactPosition.y);
		ExactPosition += num * (LastPos - ExactPosition);
		if (!ExactPosition.ToIntVec3().IsValid)
		{
			Destroy();
		}
		else
		{
			base.Impact(null);
		}
	}
}
