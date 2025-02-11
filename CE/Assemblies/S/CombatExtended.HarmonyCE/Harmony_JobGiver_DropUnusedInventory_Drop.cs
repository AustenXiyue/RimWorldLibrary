using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(JobGiver_DropUnusedInventory), "Drop")]
public static class Harmony_JobGiver_DropUnusedInventory_Drop
{
	public static bool Prefix(JobGiver_DropUnusedInventory __instance, Pawn pawn, Thing thing)
	{
		if (thing.def.IsIngestible && !thing.def.IsDrug && (int)thing.def.ingestible.preferability <= 5)
		{
			if (pawn.HoldTrackerIsHeld(thing))
			{
				pawn.HoldTrackerForget(thing);
			}
			return true;
		}
		Loadout loadout = pawn.GetLoadout();
		return loadout == null || loadout.SlotCount <= 0;
	}
}
