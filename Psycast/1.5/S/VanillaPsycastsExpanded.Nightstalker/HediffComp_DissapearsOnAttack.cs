using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

public class HediffComp_DissapearsOnAttack : HediffComp
{
	public override bool CompShouldRemove
	{
		get
		{
			Stance stance = base.Pawn?.stances?.curStance;
			if (stance is Stance_Warmup { ticksLeft: var ticksLeft } stance_Warmup)
			{
				if (ticksLeft <= 1)
				{
					Verb verb = stance_Warmup.verb;
					if (verb != null)
					{
						VerbProperties verbProps = verb.verbProps;
						if (verbProps != null && verbProps.violent)
						{
							goto IL_008e;
						}
					}
				}
			}
			else if (stance is Stance_Cooldown { verb: { verbProps: { violent: not false } } })
			{
				goto IL_008e;
			}
			return false;
			IL_008e:
			return true;
		}
	}
}
