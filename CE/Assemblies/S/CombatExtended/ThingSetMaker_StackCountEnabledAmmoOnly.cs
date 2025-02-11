using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class ThingSetMaker_StackCountEnabledAmmoOnly : ThingSetMaker_StackCount
{
	private bool basic = true;

	private bool advanced = false;

	public override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		parms.validator = (ThingDef d) => CE_ThingSetMakerUtility.CanGenerate(d, basic, advanced);
		base.Generate(parms, outThings);
	}
}
