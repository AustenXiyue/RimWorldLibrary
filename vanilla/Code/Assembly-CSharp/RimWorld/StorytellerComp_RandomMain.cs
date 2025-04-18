using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class StorytellerComp_RandomMain : StorytellerComp
{
	protected StorytellerCompProperties_RandomMain Props => (StorytellerCompProperties_RandomMain)props;

	public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		if (!Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1000f))
		{
			yield break;
		}
		bool flag = target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon);
		List<IncidentCategoryDef> list = new List<IncidentCategoryDef>();
		do
		{
			IncidentCategoryDef incidentCategoryDef = ChooseRandomCategory(target, list);
			IncidentParms parms = GenerateParms(incidentCategoryDef, target);
			if (TrySelectRandomIncident(UsableIncidentsInCategory(incidentCategoryDef, parms), out var foundDef, target))
			{
				if (!(Props.skipThreatBigIfRaidBeacon && flag) || foundDef.category != IncidentCategoryDefOf.ThreatBig)
				{
					yield return new FiringIncident(foundDef, this, parms);
				}
				break;
			}
			list.Add(incidentCategoryDef);
		}
		while (list.Count < Props.categoryWeights.Count);
	}

	private IncidentCategoryDef ChooseRandomCategory(IIncidentTarget target, List<IncidentCategoryDef> skipCategories)
	{
		if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig))
		{
			int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
			if (target.StoryState.LastThreatBigTick >= 0 && (float)num > 60000f * Props.maxThreatBigIntervalDays)
			{
				return IncidentCategoryDefOf.ThreatBig;
			}
		}
		return Props.categoryWeights.Where((IncidentCategoryEntry cw) => !skipCategories.Contains(cw.category)).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
	}

	public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
	{
		IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat, target);
		if (incidentParms.points >= 0f)
		{
			incidentParms.points *= Props.randomPointsFactorRange.RandomInRange;
		}
		return incidentParms;
	}
}
