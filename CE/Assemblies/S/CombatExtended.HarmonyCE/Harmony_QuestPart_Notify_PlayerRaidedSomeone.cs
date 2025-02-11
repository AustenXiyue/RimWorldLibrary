using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(QuestPart_Notify_PlayerRaidedSomeone), "Notify_QuestSignalReceived")]
internal static class Harmony_QuestPart_Notify_PlayerRaidedSomeone
{
	internal static bool Prefix(Signal signal, QuestPart_Notify_PlayerRaidedSomeone __instance)
	{
		if (signal.tag == __instance.inSignal && signal.args.TryGetArg("MAP", out Map arg))
		{
			IdeoUtility.Notify_PlayerRaidedSomeone(arg.mapPawns.FreeColonistsSpawned);
			return false;
		}
		return true;
	}
}
