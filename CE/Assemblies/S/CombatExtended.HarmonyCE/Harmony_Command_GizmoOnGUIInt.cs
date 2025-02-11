using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
public static class Harmony_Command_GizmoOnGUIInt
{
	public static void Prefix(Command __instance)
	{
		if (__instance is Command_VerbTarget { verb: Verb_MarkForArtillery verb } command_VerbTarget && !verb.MarkingConditionsMet())
		{
			((Gizmo)command_VerbTarget).disabled = true;
			command_VerbTarget.disabledReason = "CE_MarkingUnavailableReason".Translate();
		}
	}
}
