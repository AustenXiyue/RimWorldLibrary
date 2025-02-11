using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
internal static class Harmony_PawnWeaponGenerator_TryGenerateWeaponFor
{
	public static void Postfix(Pawn pawn, PawnGenerationRequest request)
	{
		LoadoutPropertiesExtension modExtension = pawn.kindDef.GetModExtension<LoadoutPropertiesExtension>();
		if (modExtension != null)
		{
			float biocodeWeaponChance = ((request.BiocodeWeaponChance > 0f) ? request.BiocodeWeaponChance : pawn.kindDef.biocodeWeaponChance);
			modExtension.GenerateLoadoutFor(pawn, biocodeWeaponChance);
		}
	}
}
