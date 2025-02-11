using Verse;

namespace CombatExtended.HarmonyCE;

public class Harmony_ThingOwner_NotifyRemoved_Patch
{
	public static void Postfix(ThingOwner __instance, Thing item)
	{
		if (item != null)
		{
			CE_Utility.TryUpdateInventory(__instance);
		}
	}
}
