using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
internal static class Harmony_JobGiver_Reload_TryGiveJob
{
	internal static bool Prefix(Pawn pawn, ref Job __result)
	{
		CompApparelReloadable compReloadable = (CompApparelReloadable)ReloadableUtility.FindSomeReloadableComponent(pawn, allowForcedReload: false);
		if (compReloadable != null)
		{
			Thing thing2 = pawn?.inventory?.innerContainer?.InnerListForReading?.Find((Thing thing) => thing.def == compReloadable.AmmoDef);
			if (thing2 != null)
			{
				int count = Mathf.Min(compReloadable.MaxAmmoNeeded(allowForcedReload: true), thing2.stackCount);
				pawn.inventory.innerContainer.TryDrop(thing2, pawn.Position, pawn.Map, ThingPlaceMode.Direct, count, out var resultingThing);
				List<Thing> chosenAmmo = new List<Thing> { resultingThing };
				__result = JobGiver_Reload.MakeReloadJob(compReloadable, chosenAmmo);
				return false;
			}
		}
		return true;
	}
}
