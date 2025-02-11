using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended.WorldObjects;

public class HostilityRaider : IExposable
{
	private int ticksToRaid = -1;

	private IncidentParms parms;

	private float points = -1f;

	public HostilityComp comp;

	public virtual bool AbleToRaidResponse
	{
		get
		{
			FactionStrengthTracker strengthTracker = comp.parent.Faction.GetStrengthTracker();
			if (strengthTracker == null || !strengthTracker.CanRaid)
			{
				return false;
			}
			bool? flag = null;
			if (comp.parent is Site site)
			{
				foreach (SitePart part in site.parts)
				{
					flag = part.def.GetModExtension<WorldObjectHostilityExtension>()?.AbleToRaidResponse;
					if (flag.HasValue)
					{
						return flag.Value;
					}
				}
			}
			flag = comp.parent.Faction?.def.GetModExtension<WorldObjectHostilityExtension>()?.AbleToRaidResponse;
			if (flag.HasValue)
			{
				return flag.Value;
			}
			flag = (comp.props as WorldObjectCompProperties_Hostility).AbleToRaidResponse;
			if (flag.HasValue)
			{
				return flag.Value;
			}
			return strengthTracker?.CanRaid ?? false;
		}
	}

	public virtual void ThrottledTick()
	{
		if (ticksToRaid > 0 || points < 0f)
		{
			ticksToRaid -= 15;
			return;
		}
		if (parms != null && Find.Maps.Contains(parms.target))
		{
			IncidentDef raidEnemy = IncidentDefOf.RaidEnemy;
			raidEnemy.Worker.TryExecute(parms);
		}
		points = -1f;
		parms = null;
		ticksToRaid = -1;
	}

	public virtual bool TryRaid(Map targetMap, float points)
	{
		if (!AbleToRaidResponse)
		{
			return false;
		}
		if (points <= 0f)
		{
			return false;
		}
		this.points = points;
		ticksToRaid = Rand.Range(3000, 15000);
		string name = comp.parent.Faction.Name;
		string label = comp.parent.Label;
		StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
		parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
		parms.faction = comp.parent.Faction;
		parms.points = points;
		parms.customLetterDef = CE_LetterDefOf.CE_ThreatBig;
		parms.customLetterLabel = "CE_Message_CounterRaid_Label".Translate(name);
		parms.customLetterText = "CE_Message_CounterRaid_Desc".Translate(name, label);
		parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
		if ((int)comp.parent.Faction.def.techLevel >= 4)
		{
			if (Rand.Chance(Mathf.Min(points / 10000f, 0.5f)))
			{
				parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
			}
			else if (Rand.Chance(0.25f))
			{
				parms.raidArrivalMode = PawnsArrivalModeDefOf.EmergeFromWater;
			}
			else
			{
				parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
			}
		}
		else
		{
			parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
		}
		return true;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref parms, "raidParms");
		Scribe_Values.Look(ref points, "points", 0f);
		Scribe_Values.Look(ref ticksToRaid, "ticksToRaid", 0);
	}
}
