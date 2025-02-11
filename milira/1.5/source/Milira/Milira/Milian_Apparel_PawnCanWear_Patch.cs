using HarmonyLib;
using RimWorld;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(Apparel))]
[HarmonyPatch("PawnCanWear")]
public static class Milian_Apparel_PawnCanWear_Patch
{
	[HarmonyPrefix]
	public static bool Prefix(Pawn pawn, ref bool __result, Apparel __instance)
	{
		if (__instance.def.apparel.tags.Contains("MilianApparel"))
		{
			if (MilianUtility.IsMilian(pawn))
			{
				__result = true;
				return false;
			}
			__result = false;
			return false;
		}
		return true;
	}
}
