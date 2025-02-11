using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CombatExtended;

public class WorldStrengthTracker : WorldComponent
{
	private bool initialized = false;

	private List<FactionStrengthTracker> trackers = new List<FactionStrengthTracker>();

	public WorldStrengthTracker(World world)
		: base(world)
	{
	}

	public override void WorldComponentTick()
	{
		base.WorldComponentTick();
		if (!initialized)
		{
			Rebuild();
		}
		if (GenTicks.TicksGame % 30000 == 0)
		{
			Rebuild();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		try
		{
			Scribe_Collections.Look(ref trackers, "trackers", LookMode.Deep);
		}
		catch (Exception arg)
		{
			Log.Error("CE: WorldStrengthTracker failed to scribe, rebuilding.");
			Log.Error($"CE: {arg}");
			trackers.Clear();
		}
		if (trackers == null)
		{
			trackers = new List<FactionStrengthTracker>();
		}
	}

	public FactionStrengthTracker GetFactionTracker(Faction faction)
	{
		if (faction == null)
		{
			return null;
		}
		if (faction.defeated || faction.IsPlayer)
		{
			return null;
		}
		FactionStrengthTracker factionStrengthTracker = trackers.FirstOrDefault((FactionStrengthTracker t) => t.Faction == faction);
		if (factionStrengthTracker != null)
		{
			return factionStrengthTracker;
		}
		Rebuild();
		return trackers.FirstOrDefault((FactionStrengthTracker t) => t.Faction == faction);
	}

	public void Rebuild()
	{
		trackers.RemoveAll((FactionStrengthTracker t) => t.Faction?.defeated ?? true);
		foreach (Faction faction in world.factionManager.AllFactions)
		{
			if (!faction.IsPlayer && !faction.defeated && !trackers.Any((FactionStrengthTracker s) => s.Faction == faction))
			{
				FactionStrengthTracker item = new FactionStrengthTracker(faction);
				trackers.Add(item);
			}
		}
	}
}
