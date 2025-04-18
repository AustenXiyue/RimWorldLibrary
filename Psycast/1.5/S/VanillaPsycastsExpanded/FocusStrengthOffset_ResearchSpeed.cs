using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public class FocusStrengthOffset_ResearchSpeed : FocusStrengthOffset
{
	public override bool CanApply(Thing parent, Pawn user = null)
	{
		return parent is Building_ResearchBench;
	}

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return offset * parent.GetStatValue(StatDefOf.ResearchSpeedFactor);
	}

	public override string GetExplanation(Thing parent)
	{
		return "Difficulty_ResearchSpeedFactor_Label".Translate() + ": " + GetOffset(parent).ToStringWithSign("0%");
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		if (def == null)
		{
			return "";
		}
		return "Difficulty_ResearchSpeedFactor_Label".Translate() + ": " + (offset * def.GetStatValueAbstract(StatDefOf.ResearchSpeedFactor)).ToStringWithSign("0%");
	}
}
