using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
internal static class Harmony_ApparelTracker_Notify_ApparelRemoved
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
			Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == hediffDef);
			if (hediff == null)
			{
				Log.Warning($"Combat Extended :: Apparel {apparel} tried removing hediff {hediffDef} from {pawn} but could not find any");
			}
			else
			{
				pawn.health.RemoveHediff(hediff);
			}
		}
	}
}
