using Verse;
using Verse.AI;

namespace Milira;

public class ThinkNode_ConditionalCarrier : ThinkNode_Conditional
{
	public float minIngredientCount = 0f;

	protected override bool Satisfied(Pawn pawn)
	{
		CompThingCarrier compThingCarrier = pawn.TryGetComp<CompThingCarrier>();
		if (compThingCarrier == null || (float)compThingCarrier.IngredientCount < minIngredientCount)
		{
			return false;
		}
		return true;
	}
}
