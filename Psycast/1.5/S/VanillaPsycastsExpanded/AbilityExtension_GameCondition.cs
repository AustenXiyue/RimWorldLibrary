using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_GameCondition : AbilityExtension_AbilityMod
{
	public GameConditionDef gameCondition;

	public FloatRange? durationDays;

	public bool sendLetter = false;

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		GameCondition cond = GameConditionMaker.MakeCondition(gameCondition, durationDays.HasValue ? ((int)(durationDays.Value.RandomInRange * 60000f)) : ability.GetDurationForPawn());
		ability.pawn.Map.gameConditionManager.RegisterCondition(cond);
		if (sendLetter)
		{
			ChoiceLetter let = LetterMaker.MakeLetter(gameCondition.LabelCap, gameCondition.letterText, LetterDefOf.NegativeEvent, LookTargets.Invalid, null, null, gameCondition.letterHyperlinks);
			Find.LetterStack.ReceiveLetter(let);
		}
	}
}
