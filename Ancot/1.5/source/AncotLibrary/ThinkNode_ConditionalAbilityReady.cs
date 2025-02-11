using RimWorld;
using Verse;
using Verse.AI;

namespace AncotLibrary;

public class ThinkNode_ConditionalAbilityReady : ThinkNode_Conditional
{
	public AbilityDef ability;

	protected override bool Satisfied(Pawn pawn)
	{
		if (ability == null)
		{
			return false;
		}
		return pawn.abilities.GetAbility(ability).CanCast;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalHasAbility thinkNode_ConditionalHasAbility = (ThinkNode_ConditionalHasAbility)base.DeepCopy(resolve);
		thinkNode_ConditionalHasAbility.ability = ability;
		return thinkNode_ConditionalHasAbility;
	}
}
