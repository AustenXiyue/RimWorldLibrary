using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(PawnCapacityUtility), "CalculateNaturalPartsAverageEfficiency")]
internal static class Harmony_PawnCapacityUtility
{
	internal static bool Prefix(ref float __result, HediffSet diffSet, BodyPartGroupDef bodyPartGroup)
	{
		ThingWithComps thingWithComps = diffSet.pawn.equipment?.Primary;
		if (thingWithComps?.def.tools != null && thingWithComps.def.tools.Any((Tool t) => t.linkedBodyPartsGroup == bodyPartGroup))
		{
			__result = 1f;
			return false;
		}
		return true;
	}
}
