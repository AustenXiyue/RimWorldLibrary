using Verse;
using Verse.AI;

namespace CombatExtended;

public class ThinkNode_ConditionalHasLoadout : ThinkNode_Conditional
{
	public override bool Satisfied(Pawn pawn)
	{
		Loadout loadout = pawn.GetLoadout();
		return loadout != null && !loadout.Slots.NullOrEmpty();
	}
}
