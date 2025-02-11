using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

public static class Harmony_Faction
{
	[HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
	public static class Harmony_Faction_Notify_LeaderDied
	{
		public static void Prefix(Faction __instance)
		{
			__instance.GetStrengthTracker()?.Notify_LeaderKilled();
		}
	}
}
