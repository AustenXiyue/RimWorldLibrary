using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryTransferEquipmentToContainer")]
internal static class Pawn_EquipmentTracker_TryTransferEquipmentToContainer
{
	private static void Postfix(Pawn_EquipmentTracker __instance, Pawn ___pawn)
	{
		if (___pawn.Spawned)
		{
			___pawn.stances.CancelBusyStanceSoft();
		}
	}
}
