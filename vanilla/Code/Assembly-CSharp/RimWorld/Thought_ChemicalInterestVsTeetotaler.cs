namespace RimWorld;

public class Thought_ChemicalInterestVsTeetotaler : Thought_SituationalSocial
{
	public override float OpinionOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		int num = pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
		if (otherPawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) >= 0)
		{
			return 0f;
		}
		if (num == 1)
		{
			return -20f;
		}
		return -30f;
	}
}
