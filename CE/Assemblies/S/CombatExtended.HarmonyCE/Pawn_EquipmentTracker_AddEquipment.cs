using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PrimaryDestroyed")]
internal static class Pawn_EquipmentTracker_AddEquipment
{
	private static void Postfix(Pawn_EquipmentTracker __instance, Pawn ___pawn)
	{
		___pawn.TryGetComp<CompInventory>()?.SwitchToNextViableWeapon();
	}
}
