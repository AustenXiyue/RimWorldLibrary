using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
internal static class Harmony_ApparelTracker_Notify_ApparelAdded
{
	internal static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
	{
		ApparelDefExtension modExtension = apparel.def.GetModExtension<ApparelDefExtension>();
		if (modExtension != null && modExtension.isRadioPack && __instance.pawn.equipment?.equipment?.Any == true)
		{
			foreach (Verb allEquipmentVerb in __instance.pawn.equipment.AllEquipmentVerbs)
			{
				if (allEquipmentVerb is Verb_MarkForArtillery verb_MarkForArtillery)
				{
					verb_MarkForArtillery.Dirty();
				}
			}
		}
		HediffDef hediffDef = apparel.def.GetModExtension<ApparelHediffExtension>()?.hediff;
		if (hediffDef != null)
		{
			Pawn pawn = __instance.pawn;
			pawn.health.AddHediff(hediffDef, null, null);
		}
	}
}
