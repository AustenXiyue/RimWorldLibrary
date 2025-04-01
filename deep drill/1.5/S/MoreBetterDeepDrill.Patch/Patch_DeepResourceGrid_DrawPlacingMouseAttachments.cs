using HarmonyLib;
using MoreBetterDeepDrill.Comp;
using MoreBetterDeepDrill.Utils;
using Verse;

namespace MoreBetterDeepDrill.Patch;

[HarmonyPatch(typeof(DeepResourceGrid), "DrawPlacingMouseAttachments")]
public class Patch_DeepResourceGrid_DrawPlacingMouseAttachments
{
	private static void Postfix(DeepResourceGrid __instance, BuildableDef placingDef)
	{
		Map currentMap = Find.CurrentMap;
		if (placingDef is ThingDef thingDef && thingDef.CompDefFor<MBDD_CompRangedDeepDrill>() != null && DeepDrillUtil.AnyActiveDeepScannersOnMap(currentMap))
		{
			DeepDrillUtil.RenderMouseAttachments(currentMap);
		}
	}
}
