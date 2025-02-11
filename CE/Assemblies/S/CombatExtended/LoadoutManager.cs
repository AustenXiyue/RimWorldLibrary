using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended;

public class LoadoutManager : GameComponent
{
	private Dictionary<Pawn, Loadout> _assignedLoadouts = new Dictionary<Pawn, Loadout>();

	private List<Loadout> _loadouts = new List<Loadout>();

	private List<Tracker> _trackers = new List<Tracker>();

	private Dictionary<Pawn, Tracker> _assignedTrackers = new Dictionary<Pawn, Tracker>();

	private static LoadoutManager _current;

	public static Dictionary<Pawn, Loadout> AssignedLoadouts => (_current != null) ? _current._assignedLoadouts : new Dictionary<Pawn, Loadout>();

	public static Loadout DefaultLoadout => (_current != null) ? _current._loadouts.First((Loadout l) => l.defaultLoadout) : MakeDefaultLoadout();

	public static List<Loadout> Loadouts => (_current != null) ? _current._loadouts : null;

	public LoadoutManager(Game game)
	{
		_loadouts.Add(MakeDefaultLoadout());
		_current = null;
	}

	public LoadoutManager()
	{
		_current = null;
	}

	public override void FinalizeInit()
	{
		base.FinalizeInit();
		_current = Current.Game.GetComponent<LoadoutManager>();
	}

	public override void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			PurgeHoldTrackerRolls();
			PurgeLoadoutRolls();
		}
		Scribe_Collections.Look(ref _loadouts, "loadouts", LookMode.Deep);
		Scribe_Collections.Look(ref _assignedLoadouts, "assignmentLoadouts", LookMode.Reference, LookMode.Reference);
		bool value = _trackers.Any();
		Scribe_Values.Look(ref value, "HasTrackers", defaultValue: false);
		if (value)
		{
			Scribe_Collections.Look(ref _trackers, "HoldTrackers", LookMode.Deep);
			Scribe_Collections.Look(ref _assignedTrackers, "assignedTrackers", LookMode.Reference, LookMode.Reference);
		}
	}

	private static Loadout MakeDefaultLoadout()
	{
		return new Loadout("CE_EmptyLoadoutName".Translate(), 1)
		{
			canBeDeleted = false,
			defaultLoadout = true
		};
	}

	public static List<HoldRecord> GetHoldRecords(Pawn pawn)
	{
		if (_current == null)
		{
			return null;
		}
		if (_current._assignedTrackers.TryGetValue(pawn, out var value))
		{
			return value.recs;
		}
		return null;
	}

	public static void PurgeHoldTrackerRolls()
	{
		if (_current == null)
		{
			return;
		}
		List<Pawn> list = new List<Pawn>(_current._assignedTrackers.Keys.Count);
		foreach (Pawn key in _current._assignedTrackers.Keys)
		{
			if (key.Dead)
			{
				list.Add(key);
			}
			else if (!_current._assignedTrackers[key].recs.Any())
			{
				list.Add(key);
			}
			else if (key.DestroyedOrNull())
			{
				list.Add(key);
			}
		}
		foreach (Pawn item2 in list)
		{
			Tracker item = _current._assignedTrackers[item2];
			_current._trackers.Remove(item);
			_current._assignedTrackers.Remove(item2);
		}
	}

	public static void PurgeLoadoutRolls()
	{
		if (_current == null)
		{
			return;
		}
		List<Pawn> list = new List<Pawn>(_current._assignedLoadouts.Keys.Count);
		foreach (Pawn key in _current._assignedLoadouts.Keys)
		{
			if (key.Dead)
			{
				list.Add(key);
			}
			else if (key.DestroyedOrNull())
			{
				list.Add(key);
			}
		}
		foreach (Pawn item in list)
		{
			_current._assignedLoadouts.Remove(item);
		}
	}

	public static void AddHoldRecords(Pawn pawn, List<HoldRecord> newRecords)
	{
		if (_current != null)
		{
			Tracker tracker = new Tracker(newRecords);
			_current._trackers.Add(tracker);
			_current._assignedTrackers.Add(pawn, tracker);
		}
	}

	public static void AddLoadout(Loadout loadout)
	{
		if (_current != null)
		{
			_current._loadouts.Add(loadout);
		}
	}

	public static void RemoveLoadout(Loadout loadout)
	{
		if (_current == null)
		{
			return;
		}
		List<Pawn> list = (from kvp in AssignedLoadouts
			where kvp.Value == loadout
			select kvp.Key).ToList();
		foreach (Pawn item in list)
		{
			AssignedLoadouts[item] = DefaultLoadout;
		}
		_current._loadouts.Remove(loadout);
	}

	public static void SortLoadouts()
	{
		if (_current != null)
		{
			_current._loadouts.Sort();
		}
	}

	internal static int GetUniqueTrackerID()
	{
		LoadoutManager component = Current.Game.GetComponent<LoadoutManager>();
		if (component != null && component._assignedTrackers.Values.Any())
		{
			return component._assignedTrackers.Values.Max((Tracker t) => t.uniqueID) + 1;
		}
		return 1;
	}

	internal static int GetUniqueLoadoutID()
	{
		LoadoutManager component = Current.Game.GetComponent<LoadoutManager>();
		if (component != null && component._loadouts.Any())
		{
			return component._loadouts.Max((Loadout l) => l.uniqueID) + 1;
		}
		return 1;
	}

	internal static string GetUniqueLabel()
	{
		return GetUniqueLabel("CE_DefaultLoadoutName".Translate());
	}

	internal static bool IsUniqueLabel(string label)
	{
		LoadoutManager component = Current.Game.GetComponent<LoadoutManager>();
		if (component == null)
		{
			return false;
		}
		return !component._loadouts.Any((Loadout l) => l.label == label);
	}

	internal static string GetUniqueLabel(string head)
	{
		LoadoutManager component = Current.Game.GetComponent<LoadoutManager>();
		int num = 1;
		string label;
		if (component != null)
		{
			do
			{
				label = head + num++;
			}
			while (component._loadouts.Any((Loadout l) => l.label == label));
		}
		else
		{
			label = head + num++;
		}
		return label;
	}

	internal static Loadout GetLoadoutById(int id)
	{
		if (_current == null)
		{
			return null;
		}
		return Loadouts.Find((Loadout x) => x.uniqueID == id);
	}
}
