using Verse;

namespace AncotLibrary;

public class PlaceWorker_OnlyOneInMapColonist : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (map.listerBuildings.allBuildingsColonist.ContainsAny((Building t) => t.def == checkingDef))
		{
			return new AcceptanceReport("Ancot.OnlyOneInMap".Translate());
		}
		return true;
	}
}
