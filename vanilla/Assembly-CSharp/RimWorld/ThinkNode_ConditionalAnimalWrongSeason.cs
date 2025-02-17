using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalAnimalWrongSeason : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.IsNonMutantAnimal)
		{
			return !pawn.Map.mapTemperature.SeasonAcceptableFor(pawn.def);
		}
		return false;
	}
}
