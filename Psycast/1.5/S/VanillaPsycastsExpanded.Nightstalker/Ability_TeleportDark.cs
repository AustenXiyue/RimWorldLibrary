using RimWorld;
using RimWorld.Planet;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Nightstalker;

public class Ability_TeleportDark : Ability_Teleport
{
	public override FleckDef[] EffectSet => new FleckDef[3]
	{
		VPE_DefOf.VPE_PsycastSkipFlashEntry_DarkBlue,
		FleckDefOf.PsycastSkipInnerExit,
		FleckDefOf.PsycastSkipOuterRingExit
	};

	public override bool CanHitTarget(LocalTargetInfo target)
	{
		return (double)((Ability)this).pawn.Map.glowGrid.GroundGlowAt(target.Cell) <= 0.29 && !target.Cell.Fogged(((Ability)this).pawn.Map) && target.Cell.Walkable(((Ability)this).pawn.Map);
	}

	public override void ModifyTargets(ref GlobalTargetInfo[] targets)
	{
		((Ability)this).ModifyTargets(ref targets);
		targets = new GlobalTargetInfo[2]
		{
			((Ability)this).pawn,
			targets[0]
		};
	}
}
