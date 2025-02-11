using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace CombatExtended;

public class Loadout : IExposable, ILoadReferenceable, IComparable
{
	public bool canBeDeleted = true;

	public bool defaultLoadout = false;

	public string label;

	internal int uniqueID;

	private List<LoadoutSlot> _slots = new List<LoadoutSlot>();

	public float Bulk => _slots.Sum((LoadoutSlot slot) => slot.bulk * (float)slot.count);

	public string LabelCap => label.CapitalizeFirst();

	public int SlotCount => _slots.Count;

	public List<LoadoutSlot> Slots => _slots;

	public float Weight => _slots.Sum((LoadoutSlot slot) => slot.mass * (float)slot.count);

	public Loadout()
	{
		label = LoadoutManager.GetUniqueLabel();
		uniqueID = LoadoutManager.GetUniqueLoadoutID();
	}

	public Loadout(string label)
	{
		this.label = label;
		uniqueID = LoadoutManager.GetUniqueLoadoutID();
	}

	public Loadout(string label, int uniqueID)
	{
		this.label = label;
		this.uniqueID = uniqueID;
	}

	public void AddBasicSlots()
	{
		IEnumerable<LoadoutGenericDef> enumerable = DefDatabase<LoadoutGenericDef>.AllDefs.Where((LoadoutGenericDef d) => d.isBasic);
		foreach (LoadoutGenericDef item in enumerable)
		{
			LoadoutSlot slot = new LoadoutSlot(item);
			AddSlot(slot);
		}
	}

	private static Loadout Copy(Loadout source)
	{
		Loadout loadout = new Loadout(UniqueLabel(source.label));
		loadout.defaultLoadout = source.defaultLoadout;
		loadout.canBeDeleted = source.canBeDeleted;
		loadout._slots = new List<LoadoutSlot>();
		foreach (LoadoutSlot slot in source.Slots)
		{
			loadout.AddSlot(slot.Copy());
		}
		return loadout;
	}

	private static string UniqueLabel(string label)
	{
		Regex regex = new Regex("^(.*?)\\d+$");
		if (regex.IsMatch(label))
		{
			label = regex.Replace(label, "$1");
		}
		return LoadoutManager.GetUniqueLabel(label);
	}

	public Loadout Copy()
	{
		return Copy(this);
	}

	public void AddSlot(LoadoutSlot slot)
	{
		LoadoutSlot loadoutSlot = _slots.FirstOrDefault(slot.isSameDef);
		if (loadoutSlot != null)
		{
			loadoutSlot.count += slot.count;
		}
		else
		{
			_slots.Add(slot);
		}
	}

	public static Loadout FromConfig(LoadoutConfig loadoutConfig, out List<string> unloadableDefNames)
	{
		string text = (LoadoutManager.IsUniqueLabel(loadoutConfig.label) ? loadoutConfig.label : UniqueLabel(loadoutConfig.label));
		Loadout loadout = new Loadout(text);
		unloadableDefNames = new List<string>();
		LoadoutSlotConfig[] slots = loadoutConfig.slots;
		foreach (LoadoutSlotConfig loadoutSlotConfig in slots)
		{
			LoadoutSlot loadoutSlot = LoadoutSlot.FromConfig(loadoutSlotConfig);
			if (loadoutSlot == null)
			{
				unloadableDefNames.Add(loadoutSlotConfig.defName);
			}
			else
			{
				loadout.AddSlot(LoadoutSlot.FromConfig(loadoutSlotConfig));
			}
		}
		return loadout;
	}

	public LoadoutConfig ToConfig()
	{
		List<LoadoutSlotConfig> list = new List<LoadoutSlotConfig>();
		foreach (LoadoutSlot slot in _slots)
		{
			list.Add(slot.ToConfig());
		}
		return new LoadoutConfig
		{
			label = label,
			slots = list.ToArray()
		};
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref label, "label");
		Scribe_Values.Look(ref uniqueID, "uniqueID", 0);
		Scribe_Values.Look(ref canBeDeleted, "canBeDeleted", defaultValue: true);
		Scribe_Values.Look(ref defaultLoadout, "defaultLoadout", defaultValue: false);
		Scribe_Collections.Look(ref _slots, "slots", LookMode.Deep);
	}

	public string GetUniqueLoadID()
	{
		return "Loadout_" + label + "_" + uniqueID;
	}

	public void MoveSlot(LoadoutSlot slot, int toIndex)
	{
		int fromIndex = _slots.IndexOf(slot);
		MoveTo(fromIndex, toIndex);
	}

	public void RemoveSlot(LoadoutSlot slot)
	{
		_slots.Remove(slot);
	}

	public void RemoveSlot(int index)
	{
		_slots.RemoveAt(index);
	}

	private int MoveTo(int fromIndex, int toIndex)
	{
		if (fromIndex < 0 || fromIndex >= _slots.Count || toIndex < 0 || toIndex >= _slots.Count)
		{
			throw new Exception("Attempted to move i " + fromIndex + " to " + toIndex + ", bounds are [0," + (_slots.Count - 1) + "].");
		}
		LoadoutSlot item = _slots[fromIndex];
		_slots.RemoveAt(fromIndex);
		if (fromIndex + 1 < toIndex)
		{
			toIndex--;
		}
		_slots.Insert(toIndex, item);
		return toIndex;
	}

	public int CompareTo(object obj)
	{
		if (!(obj is Loadout loadout))
		{
			throw new ArgumentException("Loadout.CompareTo(obj), obj is not of type Loadout.");
		}
		if (defaultLoadout && loadout.defaultLoadout)
		{
			return 0;
		}
		if (defaultLoadout)
		{
			return -1;
		}
		if (loadout.defaultLoadout)
		{
			return 1;
		}
		return label.CompareTo(loadout.label);
	}
}
