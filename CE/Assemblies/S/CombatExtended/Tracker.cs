using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class Tracker : IExposable, ILoadReferenceable
{
	private List<HoldRecord> _recs;

	internal int uniqueID;

	public List<HoldRecord> recs => _recs;

	public Tracker()
	{
		uniqueID = LoadoutManager.GetUniqueTrackerID();
		_recs = new List<HoldRecord>();
	}

	public Tracker(List<HoldRecord> newRecs)
	{
		uniqueID = LoadoutManager.GetUniqueTrackerID();
		_recs = newRecs;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref uniqueID, "TrackerID", 0);
		Scribe_Collections.Look(ref _recs, "HoldRecords", LookMode.Undefined);
	}

	public string GetUniqueLoadID()
	{
		return "Loadout_" + uniqueID;
	}
}
