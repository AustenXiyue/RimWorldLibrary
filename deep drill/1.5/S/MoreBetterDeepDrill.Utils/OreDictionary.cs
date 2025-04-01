using System;
using System.Collections.Generic;
using System.Linq;
using MoreBetterDeepDrill.Types;
using Verse;

namespace MoreBetterDeepDrill.Utils;

[StaticConstructorOnStartup]
public static class OreDictionary
{
	private static Predicate<ThingDef> validOre;

	public static Dictionary<ThingDef, DrillableOre> DrillableOreDict;

	static OreDictionary()
	{
		validOre = (ThingDef def) => def.deepCommonality > 0f;
	}

	public static void Build(bool rebuild = false)
	{
		List<DrillableOre> list = ((rebuild || StaticValues.ModSetting.oreDictionary.NullOrEmpty()) ? new List<DrillableOre>() : StaticValues.ModSetting.oreDictionary);
		foreach (ThingDef ore in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => validOre(def)))
		{
			if (rebuild || list.NullOrEmpty() || !list.Any((DrillableOre x) => ore == x.OreDef))
			{
				list.Add(new DrillableOre(ore, ore.deepCountPerPortion));
			}
		}
		StaticValues.ModSetting.oreDictionary = list;
	}

	public static void Refresh()
	{
		List<DrillableOre> oreDictionary = StaticValues.ModSetting.oreDictionary;
		for (int num = oreDictionary.Count - 1; num >= 0; num--)
		{
			if (oreDictionary[num] == null)
			{
				oreDictionary.Remove(oreDictionary[num]);
			}
		}
	}

	public static void AddExtraDrillable(List<ThingDef> defs)
	{
		bool flag = false;
		List<DrillableOre> oreDictionary = StaticValues.ModSetting.oreDictionary;
		List<DrillableOre> list = new List<DrillableOre>();
		if (oreDictionary == null)
		{
			return;
		}
		foreach (ThingDef def in defs)
		{
			int amountPerPortion = 1;
			flag = false;
			ThingDef thingDef;
			if (def.building != null)
			{
				thingDef = def.building.mineableThing;
				amountPerPortion = def.building.mineableYield;
			}
			else
			{
				thingDef = def;
			}
			foreach (DrillableOre item in oreDictionary)
			{
				if (item.OreDef != null && item.OreDef == thingDef)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(new DrillableOre(thingDef, amountPerPortion));
			}
		}
		foreach (DrillableOre item2 in list)
		{
			oreDictionary.Add(item2);
		}
	}
}
