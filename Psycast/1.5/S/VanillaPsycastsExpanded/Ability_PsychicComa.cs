using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_PsychicComa : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		Hediff hediff = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, base.pawn);
		hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = (int)(300000f / base.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
		base.pawn.health.AddHediff(hediff, null, null);
	}
}
