using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_GroupLink : Ability
{
	public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
	{
		Hediff_GroupLink hediff_GroupLink = ((Ability)this).ApplyHediff(targetPawn, hediffDef, bodyPart, duration, severity) as Hediff_GroupLink;
		hediff_GroupLink.LinkAllPawnsAround();
		return (Hediff)(object)hediff_GroupLink;
	}
}
