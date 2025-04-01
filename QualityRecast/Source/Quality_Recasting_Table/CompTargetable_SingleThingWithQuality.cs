using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Quality_Recasting_Table;

public class CompTargetable_SingleThingWithQuality : CompTargetable
{
	protected override bool PlayerChoosesTarget => true;

	protected override TargetingParameters GetTargetingParameters()
	{
		return new TargetingParameters
		{
			canTargetItems = true,
			mapObjectTargetsMustBeAutoAttackable = false,
			canTargetLocations = false,
			canTargetPawns = false,
			canTargetBuildings = false,
			validator = (TargetInfo targ) => targ.Thing != null && (targ.Thing is MinifiedThing || targ.Thing.def.category == ThingCategory.Item)
		};
	}

	public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
	{
		yield return targetChosenByPlayer;
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (target.Thing is MinifiedThing minifiedThing && minifiedThing.InnerThing?.TryGetComp<CompQuality>() != null)
		{
			return base.ValidateTarget(minifiedThing.InnerThing, showMessages);
		}
		if (target.Thing.TryGetComp<CompQuality>() != null)
		{
			return base.ValidateTarget(target.Thing, showMessages);
		}
		return false;
	}
}
