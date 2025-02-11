using RimWorld;
using Verse;

namespace AncotLibrary;

public class Projectile_Custom : Bullet
{
	public Projectile_Custom_Extension Props => def.GetModExtension<Projectile_Custom_Extension>();

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		base.Impact(hitThing, blockedByShield);
		if (Props.impactEffecter != null)
		{
			Props.impactEffecter.Spawn().Trigger(new TargetInfo(ExactPosition.ToIntVec3(), launcher.Map), launcher);
		}
	}
}
