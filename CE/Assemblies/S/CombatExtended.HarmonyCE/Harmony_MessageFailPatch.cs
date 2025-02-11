using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
public static class Harmony_MessageFailPatch
{
	public static IEnumerable<MethodBase> TargetMethods()
	{
		yield return AccessTools.Method(typeof(WorkGiver_Tame), "JobOnThing", (Type[])null, (Type[])null);
		yield return AccessTools.Method(typeof(WorkGiver_Train), "JobOnThing", (Type[])null, (Type[])null);
	}

	public static void Postfix()
	{
		if (Harmony_WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob.rejectedInventoryMass)
		{
			JobFailReason.Is("CE_InventoryFull_TameFail".TranslateSimple());
			Harmony_WorkGiver_InteractAnimal_TakeFoodForAnimalInteractJob.rejectedInventoryMass = false;
		}
	}
}
