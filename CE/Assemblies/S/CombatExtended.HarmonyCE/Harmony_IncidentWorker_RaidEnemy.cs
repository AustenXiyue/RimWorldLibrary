using HarmonyLib;
using RimWorld;

namespace CombatExtended.HarmonyCE;

public static class Harmony_IncidentWorker_RaidEnemy
{
	[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "ResolveRaidPoints")]
	public static class Harmony_IncidentWorker_RaidEnemy_ResolveRaidPoints
	{
		public static void Postfix(ref IncidentParms parms)
		{
			FactionStrengthTracker strengthTracker = parms.faction.GetStrengthTracker();
			if (strengthTracker != null)
			{
				parms.points *= strengthTracker.StrengthPointsMultiplier;
			}
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
	public static class Harmony_IncidentWorker_RaidEnemy_TryExecuteWorker
	{
		public static bool Prefix(IncidentParms parms)
		{
			return parms.faction.GetStrengthTracker()?.CanRaid ?? true;
		}
	}
}
