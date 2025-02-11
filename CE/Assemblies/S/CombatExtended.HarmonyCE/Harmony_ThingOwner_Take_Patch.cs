using Verse;

namespace CombatExtended.HarmonyCE;

public class Harmony_ThingOwner_Take_Patch
{
	public static void Postfix(ThingOwner __instance, Thing __result)
	{
		if (__result != null)
		{
			CE_Utility.TryUpdateInventory(__instance);
		}
	}
}
