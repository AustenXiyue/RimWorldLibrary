using RimWorld;
using Verse;

namespace OP;

public class OP_ItemAbility_IronTalon : CompTargetEffect
{
	public OP_CompProperties_ItemAbility_IronTalon Props => (OP_CompProperties_ItemAbility_IronTalon)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (target == null)
		{
			return;
		}
		switch (target.def.category)
		{
		case ThingCategory.Pawn:
			if (target != null && !((Pawn)target).Dead)
			{
				target.Kill(null);
			}
			break;
		case ThingCategory.Plant:
			if (target != null)
			{
				((Plant)target).Kill(default(DamageInfo));
			}
			break;
		}
	}
}
