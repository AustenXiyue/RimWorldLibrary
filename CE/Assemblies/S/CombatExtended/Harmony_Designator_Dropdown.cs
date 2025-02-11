using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class Harmony_Designator_Dropdown
{
	private static void Postfix(Designator_Dropdown __instance, bool __result)
	{
		if (__result && !__instance.activeDesignator.Visible)
		{
			Designator designator = __instance.elements.FirstOrDefault((Designator x) => x.Visible);
			if (designator != null)
			{
				__instance.SetActiveDesignator(designator, explicitySet: false);
			}
		}
	}
}
