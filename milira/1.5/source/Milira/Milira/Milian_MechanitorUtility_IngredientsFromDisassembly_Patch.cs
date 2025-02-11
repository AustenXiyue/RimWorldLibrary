using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(MechanitorUtility))]
[HarmonyPatch("IngredientsFromDisassembly")]
public static class Milian_MechanitorUtility_IngredientsFromDisassembly_Patch
{
	[HarmonyPrefix]
	public static bool Prefix(ThingDef mech, ref List<ThingDefCountClass> __result)
	{
		if (mech.race.body.defName == "Milian_Body")
		{
			List<ThingDefCountClass> list = new List<ThingDefCountClass>();
			list.Clear();
			RecipeDef milian_Mechanoid_ForDisassembly = MiliraDefOf.Milian_Mechanoid_ForDisassembly;
			for (int i = 0; i < milian_Mechanoid_ForDisassembly.ingredients.Count; i++)
			{
				ThingDef thingDef = milian_Mechanoid_ForDisassembly.ingredients[i].filter.AllowedThingDefs.FirstOrDefault();
				int count = Mathf.RoundToInt(milian_Mechanoid_ForDisassembly.ingredients[i].GetBaseCount());
				list.Add(new ThingDefCountClass(thingDef, count));
			}
			__result = list;
			return false;
		}
		return true;
	}
}
