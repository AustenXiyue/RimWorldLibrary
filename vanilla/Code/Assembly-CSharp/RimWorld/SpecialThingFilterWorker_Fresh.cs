using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_Fresh : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		CompRottable compRottable = t.TryGetComp<CompRottable>();
		if (compRottable == null)
		{
			if (t.def.IsIngestible && !t.def.IsDrug)
			{
				return true;
			}
			return false;
		}
		return compRottable.Stage == RotStage.Fresh;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		if (def.GetCompProperties<CompProperties_Rottable>() == null)
		{
			if (def.IsIngestible)
			{
				return !def.IsDrug;
			}
			return false;
		}
		return true;
	}

	public override bool AlwaysMatches(ThingDef def)
	{
		CompProperties_Rottable compProperties = def.GetCompProperties<CompProperties_Rottable>();
		if (compProperties != null && compProperties.rotDestroys)
		{
			return true;
		}
		if (compProperties == null && def.IsIngestible && !def.IsDrug)
		{
			return true;
		}
		return false;
	}
}
