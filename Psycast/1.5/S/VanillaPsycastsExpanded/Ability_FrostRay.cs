using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_FrostRay : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		Projectile projectile = GenSpawn.Spawn(((Def)(object)base.def).GetModExtension<AbilityExtension_Projectile>().projectile, base.pawn.Position, base.pawn.Map) as Projectile;
		AbilityProjectile val = (AbilityProjectile)(object)((projectile is AbilityProjectile) ? projectile : null);
		if (val != null)
		{
			val.ability = (Ability)(object)this;
		}
		projectile?.Launch(base.pawn, base.pawn.DrawPos, (LocalTargetInfo)targets[0], (LocalTargetInfo)targets[0], ProjectileHitFlags.IntendedTarget);
		base.pawn.stances.SetStance(new Stance_Stand(((Ability)this).GetDurationForPawn(), (LocalTargetInfo)targets[0], (Verb)(object)base.verb));
	}

	public override void ApplyHediffs(params GlobalTargetInfo[] targetInfo)
	{
		((Ability)this).ApplyHediff(base.pawn);
	}
}
