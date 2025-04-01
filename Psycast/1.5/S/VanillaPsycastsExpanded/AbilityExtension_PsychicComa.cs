using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_PsychicComa : AbilityExtension_AbilityMod
{
	public float hours;

	public HediffDef coma;

	public StatDef multiplier;

	public int ticks;

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		float num = hours * 2500f + (float)ticks;
		float statValue = ability.pawn.GetStatValue(multiplier ?? StatDefOf.PsychicSensitivity);
		num *= (Mathf.Approximately(statValue, 0f) ? 10f : (1f / statValue));
		Hediff hediff = HediffMaker.MakeHediff(coma ?? VPE_DefOf.PsychicComa, ability.pawn);
		hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(num);
		ability.pawn.health.AddHediff(hediff, null, null);
	}
}
