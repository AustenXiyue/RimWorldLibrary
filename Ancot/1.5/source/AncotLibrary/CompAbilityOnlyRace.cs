using RimWorld;
using Verse;

namespace AncotLibrary;

public class CompAbilityOnlyRace : CompAbilityEffect
{
	public new CompProperties_AbilityOnlyRace Props => (CompProperties_AbilityOnlyRace)props;

	public override bool GizmoDisabled(out string reason)
	{
		Pawn pawn = parent.pawn;
		if (!Props.races.Contains(pawn.def.defName))
		{
			reason = "Ancot.Ability_OnlyRace".Translate();
			return true;
		}
		reason = "";
		return false;
	}
}
