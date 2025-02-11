using RimWorld;
using Verse;

namespace CombatExtended;

internal class ThoughtWorker_Suppressed : ThoughtWorker
{
	public override ThoughtState CurrentStateInternal(Pawn p)
	{
		CompSuppressable compSuppressable = p.TryGetComp<CompSuppressable>();
		if (compSuppressable != null)
		{
			if (compSuppressable.IsHunkering)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			if (compSuppressable.isSuppressed)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (compSuppressable.CurrentSuppression > 0f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
		}
		return ThoughtState.Inactive;
	}
}
