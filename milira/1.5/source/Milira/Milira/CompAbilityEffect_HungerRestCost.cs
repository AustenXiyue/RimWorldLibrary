using RimWorld;
using Verse;

namespace Milira;

public class CompAbilityEffect_HungerRestCost : CompAbilityEffect
{
	public new CompProperties_AbilityHungerRestCost Props => (CompProperties_AbilityHungerRestCost)props;

	private bool HasEnoughHunger
	{
		get
		{
			Pawn pawn = parent.pawn;
			return Props.hungerCost <= 0f || pawn.needs.food == null || (pawn.needs.food.CurLevelPercentage >= Props.hungerCost && pawn.needs.food.CurLevelPercentage >= Props.hungerThreshold);
		}
	}

	private bool HasEnoughRest
	{
		get
		{
			Pawn pawn = parent.pawn;
			return Props.restCost <= 0f || pawn.needs.rest == null || (pawn.needs.rest.CurLevelPercentage >= Props.restCost && pawn.needs.rest.CurLevelPercentage >= Props.restThreshold);
		}
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = parent.pawn;
		if (pawn.Faction.IsPlayer || !MiliraRaceSettings.MiliraRace_ModSetting_MiliraDifficulty_TirelessFly)
		{
			if (Props.hungerCost > 0f && pawn.needs.food != null)
			{
				pawn.needs.food.CurLevelPercentage -= Props.hungerCost;
			}
			if (Props.restCost > 0f && pawn.needs.rest != null)
			{
				pawn.needs.rest.CurLevelPercentage -= Props.restCost;
			}
		}
	}

	public override bool GizmoDisabled(out string reason)
	{
		Pawn pawn = parent.pawn;
		if (!HasEnoughHunger)
		{
			reason = "Milira.AbilityDisabled_Hunger".Translate(pawn);
			return true;
		}
		if (!HasEnoughRest)
		{
			reason = "Milira.AbilityDisabled_Rest".Translate(pawn);
			return true;
		}
		reason = "";
		return false;
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return HasEnoughHunger && HasEnoughRest;
	}
}
