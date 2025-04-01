using System.Collections.Generic;
using MoreBetterDeepDrill.Types;
using Verse;

namespace MoreBetterDeepDrill.Settings;

public class MBDD_Settings : ModSettings
{
	public List<DrillableOre> oreDictionary;

	public List<ThingDef> database;

	public bool EnableInsectoids = true;

	public bool EnableMechdroids;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref EnableInsectoids, "MBDD_EnableInsectoids", defaultValue: true);
		Scribe_Values.Look(ref EnableMechdroids, "MBDD_EnableMechdroids", defaultValue: false);
		Scribe_Collections.Look(ref oreDictionary, "MBDD_OreDictionary", LookMode.Deep);
		CheckVaild(ref oreDictionary);
	}

	private void CheckVaild(ref List<DrillableOre> oreDict)
	{
		for (int num = oreDict.Count - 1; num >= 0; num--)
		{
			if (oreDict[num].OreDef == null)
			{
				oreDict.Remove(oreDict[num]);
			}
		}
	}
}
