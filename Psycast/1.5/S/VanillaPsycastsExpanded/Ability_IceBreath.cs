using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_IceBreath : Ability_ShootProjectile
{
	protected override Projectile ShootProjectile(GlobalTargetInfo target)
	{
		IceBreatheProjectile iceBreatheProjectile = ((Ability_ShootProjectile)this).ShootProjectile(target) as IceBreatheProjectile;
		iceBreatheProjectile.ability = (Ability)(object)this;
		return (Projectile)(object)iceBreatheProjectile;
	}
}
