using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_DeadmansApparel : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		if (t is Apparel apparel)
		{
			return apparel.WornByCorpse;
		}
		return false;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		if (def.IsApparel)
		{
			return def.apparel.careIfWornByCorpse;
		}
		return false;
	}
}
