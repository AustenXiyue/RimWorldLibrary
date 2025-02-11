using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public class Harmony_ApparelLayerDef
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Postfix(ApparelLayerDef __instance, ref bool __result)
	{
		if (Controller.settings.ShowBackpacks)
		{
			__result = __result || __instance == CE_ApparelLayerDefOf.Backpack;
		}
	}
}
