using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_PowerLeap : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		Map map = ((Ability)this).Caster.Map;
		JumpingPawn jumpingPawn = (JumpingPawn)(object)PawnFlyer.MakeFlyer(VPE_DefOf.VPE_JumpingPawn, ((Ability)this).CasterPawn, targets[0].Cell, null, null, flyWithCarriedThing: false, null);
		((AbilityPawnFlyer)jumpingPawn).ability = (Ability)(object)this;
		GenSpawn.Spawn((Thing)(object)jumpingPawn, ((Ability)this).Caster.Position, map);
		((Ability)this).Cast(targets);
	}
}
