using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_Smeltable : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		if (!CanEverMatch(t.def))
		{
			return false;
		}
		return t.Smeltable;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		return def.smeltable;
	}

	public override bool AlwaysMatches(ThingDef def)
	{
		if (def.smeltable)
		{
			return !def.MadeFromStuff;
		}
		return false;
	}
}
