using RimWorld;
using Verse;

namespace Milira;

public class IncidentWorker_MiliraThreatRaid : IncidentWorker_RaidEnemy
{
	protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
	{
		if (base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer) && f.def.defName == "Milira_Faction")
		{
			if (!desperate)
			{
				return true;
			}
			return true;
		}
		return false;
	}
}
