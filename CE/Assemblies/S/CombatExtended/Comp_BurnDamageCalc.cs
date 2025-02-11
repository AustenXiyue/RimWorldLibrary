using Verse;

namespace CombatExtended;

public class Comp_BurnDamageCalc : ThingComp
{
	public bool deflectedSharp;

	public ThingDef weapon;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref deflectedSharp, "deflsharp", defaultValue: false);
		Scribe_Defs.Look(ref weapon, "weapon");
	}
}
