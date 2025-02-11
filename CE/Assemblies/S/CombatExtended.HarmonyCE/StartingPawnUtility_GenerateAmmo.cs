using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(StartingPawnUtility), "GeneratePossessions")]
internal static class StartingPawnUtility_GenerateAmmo
{
	private static IntRange magRange = new IntRange(3, 5);

	internal static void Postfix(Pawn pawn)
	{
		Dictionary<Pawn, List<ThingDefCount>> startingPossessions = Find.GameInitData.startingPossessions;
		if (!startingPossessions.ContainsKey(pawn))
		{
			return;
		}
		List<ThingDefCount> list = new List<ThingDefCount>();
		foreach (ThingDefCount item in startingPossessions[pawn])
		{
			CompProperties_AmmoUser compProperties = item.thingDef.GetCompProperties<CompProperties_AmmoUser>();
			if (compProperties != null && compProperties.ammoSet != null)
			{
				int num = compProperties.AmmoGenPerMagOverride;
				if (num <= 0)
				{
					num = Mathf.Max(compProperties.magazineSize, 1);
				}
				num *= magRange.RandomInRange;
				list.Add(new ThingDefCount(compProperties.ammoSet.ammoTypes.First().ammo, num));
			}
		}
		foreach (ThingDefCount item2 in list)
		{
			startingPossessions[pawn].Add(item2);
		}
	}
}
