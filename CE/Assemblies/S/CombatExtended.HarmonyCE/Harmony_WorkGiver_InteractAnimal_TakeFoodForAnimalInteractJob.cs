using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(WorkGiver_InteractAnimal), "TakeFoodForAnimalInteractJob", new Type[]
{
	typeof(Pawn),
	typeof(Pawn)
})]
public class Harmony_WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob
{
	public static bool rejectedInventoryMass;

	public static void Postfix(WorkGiver_InteractAnimal __instance, ref Job __result, Pawn pawn, Pawn tamee)
	{
		if (__result == null)
		{
			return;
		}
		int count = __result.count;
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
		ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(__result.targetA.Thing);
		int num2 = Mathf.CeilToInt(num / FoodUtility.GetNutrition(tamee, __result.targetA.Thing, finalIngestibleDef));
		if (compInventory != null)
		{
			if (compInventory.CanFitInInventory(__result.targetA.Thing, out var count2) && num2 <= count2)
			{
				__result.count = Mathf.Min(count, count2);
				pawn.Notify_HoldTrackerItem(__result.targetA.Thing, __result.count);
			}
			else
			{
				rejectedInventoryMass = true;
				__result = null;
			}
		}
	}
}
