using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
internal static class Pawn_EquipmentTracker_TryDropEquipment
{
	private static void Postfix(Pawn_EquipmentTracker __instance, Pawn ___pawn)
	{
		if (___pawn.Spawned)
		{
			___pawn.stances.CancelBusyStanceSoft();
		}
	}
}
