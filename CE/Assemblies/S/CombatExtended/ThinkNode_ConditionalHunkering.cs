using Verse;
using Verse.AI;

namespace CombatExtended;

internal class ThinkNode_ConditionalHunkering : ThinkNode_Conditional
{
	public override bool Satisfied(Pawn pawn)
	{
		CompSuppressable compSuppressable = pawn.TryGetComp<CompSuppressable>();
		return compSuppressable != null && compSuppressable.CanReactToSuppression && compSuppressable.IsHunkering;
	}
}
