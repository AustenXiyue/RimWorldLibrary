using HarmonyLib;
using MoreBetterDeepDrill.Comp;
using MoreBetterDeepDrill.Utils;
using Verse;

namespace MoreBetterDeepDrill.Patch;

[HarmonyPatch(typeof(DeepResourceGrid), "DeepResourcesOnGUI")]
public class Patch_DeepResourceGrid_DeepResourcesOnGUI
{
	private static void Postfix(DeepResourceGrid __instance)
	{
		Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
		if (singleSelectedThing != null && singleSelectedThing.TryGetComp<MBDD_CompRangedDeepDrill>() != null)
		{
			DeepDrillUtil.RenderMouseAttachments(singleSelectedThing.MapHeld);
		}
	}
}
