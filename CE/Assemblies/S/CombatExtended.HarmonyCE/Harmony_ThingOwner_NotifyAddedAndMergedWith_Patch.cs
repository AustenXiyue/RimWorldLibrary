using Verse;

namespace CombatExtended.HarmonyCE;

public class Harmony_ThingOwner_NotifyAddedAndMergedWith_Patch
{
	public static void Postfix(ThingOwner __instance, Thing item, int mergedCount)
	{
		if (item != null && mergedCount != 0)
		{
			CE_Utility.TryUpdateInventory(__instance);
		}
	}
}
