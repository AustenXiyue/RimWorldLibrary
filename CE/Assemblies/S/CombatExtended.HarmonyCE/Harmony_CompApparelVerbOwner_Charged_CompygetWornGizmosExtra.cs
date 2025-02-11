using System.Collections.Generic;
using CombatExtended.Compatibility;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompApparelVerbOwner_Charged), "CompGetWornGizmosExtra")]
public static class Harmony_CompApparelVerbOwner_Charged_CompygetWornGizmosExtra
{
	public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompApparelReloadable __instance)
	{
		foreach (Gizmo item in __result)
		{
			yield return item;
		}
		if (__instance.Wearer.IsColonistPlayerControlled)
		{
			yield return new Command_ReloadArmor
			{
				compReloadable = __instance,
				action = delegate
				{
					TryReloadArmor(__instance);
				},
				defaultLabel = string.Concat("CE_ReloadLabel".Translate(), " worn armor"),
				defaultDesc = "CE_ReloadDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/Reload")
			};
		}
	}

	[Multiplayer.SyncMethod]
	private static void TryReloadArmor(CompApparelReloadable __instance)
	{
		ThingWithComps parent = __instance.parent;
		Pawn wearer = __instance.Wearer;
		Job __result = null;
		bool flag = Harmony_JobGiver_Reload_TryGiveJob.Prefix(wearer, ref __result);
		if (__result != null)
		{
			wearer.jobs.StartJob(__result, JobCondition.InterruptForced, null, wearer.CurJob?.def != __result.def, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
		}
	}
}
