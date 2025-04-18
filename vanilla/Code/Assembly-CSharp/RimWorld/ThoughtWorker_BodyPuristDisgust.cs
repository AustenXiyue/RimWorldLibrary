using Verse;

namespace RimWorld;

public class ThoughtWorker_BodyPuristDisgust : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
	{
		if (!p.RaceProps.Humanlike)
		{
			return false;
		}
		if (!p.story.traits.HasTrait(TraitDefOf.BodyPurist))
		{
			return false;
		}
		if (!RelationsUtility.PawnsKnowEachOther(p, other))
		{
			return false;
		}
		if (other.def != p.def)
		{
			return false;
		}
		int num = GeneUtility.AddedAndImplantedPartsWithXenogenesCount(other);
		if (num > 0)
		{
			return ThoughtState.ActiveAtStage(num - 1);
		}
		return false;
	}
}
