using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class DefDatabase<T> where T : Def
{
	private static List<T> defsList = new List<T>();

	private static Dictionary<string, T> defsByName = new Dictionary<string, T>();

	private static Dictionary<ushort, T> defsByShortHash = new Dictionary<ushort, T>();

	public static IEnumerable<T> AllDefs => defsList;

	public static List<T> AllDefsListForReading => defsList;

	public static int DefCount => defsList.Count;

	public static void AddAllInMods()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ModContentPack item in LoadedModManager.RunningMods.OrderBy((ModContentPack m) => m.OverwritePriority).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
		{
			string sourceName2 = item.ToString();
			hashSet.Clear();
			foreach (T item2 in GenDefDatabase.DefsToGoInDatabase<T>(item))
			{
				if (!hashSet.Add(item2.defName))
				{
					Log.Error(string.Concat("Mod ", item, " has multiple ", typeof(T), "s named ", item2.defName, ". Skipping."));
				}
				else
				{
					AddDef(item2, sourceName2);
				}
			}
		}
		foreach (T item3 in LoadedModManager.PatchedDefsForReading.OfType<T>())
		{
			AddDef(item3, "Patches");
		}
		static void AddDef(T def, string sourceName)
		{
			if (def.defName == "UnnamedDef")
			{
				string text = "Unnamed" + typeof(T).Name + Rand.Range(1, 100000) + "A";
				Log.Error(typeof(T).Name + " in " + sourceName + " with label " + def.label + " lacks a defName. Giving name " + text);
				def.defName = text;
			}
			if (defsByName.TryGetValue(def.defName, out var value))
			{
				Remove(value);
			}
			Add(def);
		}
	}

	public static void Add(IEnumerable<T> defs)
	{
		foreach (T def in defs)
		{
			Add(def);
		}
	}

	public static void Add(T def)
	{
		if (def == null)
		{
			Log.Error("Tried to add null def to DefDatabase.");
			return;
		}
		while (defsByName.ContainsKey(def.defName))
		{
			Log.Error(string.Concat("Adding duplicate ", typeof(T), " name: ", def.defName));
			def.defName += Mathf.RoundToInt(Rand.Value * 1000f);
		}
		defsList.Add(def);
		defsByName.Add(def.defName, def);
		if (defsList.Count > 65535)
		{
			Log.Error(string.Concat("Too many ", typeof(T), "; over ", ushort.MaxValue));
		}
		def.index = (ushort)(defsList.Count - 1);
	}

	private static void Remove(T def)
	{
		defsByName.Remove(def.defName);
		defsList.Remove(def);
		SetIndices();
	}

	public static void Clear()
	{
		defsList.Clear();
		defsByName.Clear();
		defsByShortHash.Clear();
	}

	public static void ClearCachedData()
	{
		for (int i = 0; i < defsList.Count; i++)
		{
			defsList[i].ClearCachedData();
		}
	}

	public static void ResolveAllReferences(bool onlyExactlyMyType = true, bool parallel = false)
	{
		DeepProfiler.Start("SetIndices");
		try
		{
			SetIndices();
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("ResolveAllReferences " + typeof(T).FullName);
		try
		{
			Action<T> action = delegate(T def)
			{
				if (onlyExactlyMyType && def.GetType() != typeof(T))
				{
					return;
				}
				DeepProfiler.Start("Resolver call");
				try
				{
					def.ResolveReferences();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Error while resolving references for def ", def, ": ", ex));
				}
				finally
				{
					DeepProfiler.End();
				}
			};
			if (parallel)
			{
				GenThreading.ParallelForEach(defsList, action);
			}
			else
			{
				for (int i = 0; i < defsList.Count; i++)
				{
					action(defsList[i]);
				}
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("SetIndices");
		try
		{
			SetIndices();
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	private static void SetIndices()
	{
		for (int i = 0; i < defsList.Count; i++)
		{
			defsList[i].index = (ushort)i;
		}
		for (int j = 0; j < defsList.Count; j++)
		{
			defsList[j].PostSetIndices();
		}
	}

	public static void ErrorCheckAllDefs()
	{
		foreach (T allDef in AllDefs)
		{
			try
			{
				if (allDef.ignoreConfigErrors)
				{
					continue;
				}
				foreach (string item in allDef.ConfigErrors())
				{
					Log.Error(string.Concat("Config error in ", allDef, ": ", item));
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception in ConfigErrors() of " + allDef.defName + ": " + ex);
			}
		}
	}

	public static T GetNamed(string defName, bool errorOnFail = true)
	{
		if (errorOnFail)
		{
			if (defsByName.TryGetValue(defName, out var value))
			{
				return value;
			}
			Log.Error(string.Concat("Failed to find ", typeof(T), " named ", defName, ". There are ", defsList.Count, " defs of this type loaded."));
			return null;
		}
		if (defsByName.TryGetValue(defName, out var value2))
		{
			return value2;
		}
		return null;
	}

	public static T GetNamedSilentFail(string defName)
	{
		return GetNamed(defName, errorOnFail: false);
	}

	public static T GetByShortHash(ushort shortHash)
	{
		if (defsByShortHash.TryGetValue(shortHash, out var value))
		{
			return value;
		}
		return null;
	}

	public static void InitializeShortHashDictionary()
	{
		for (int i = 0; i < defsList.Count; i++)
		{
			defsByShortHash.SetOrAdd(defsList[i].shortHash, defsList[i]);
		}
	}

	public static T GetRandom()
	{
		return defsList.RandomElement();
	}
}
