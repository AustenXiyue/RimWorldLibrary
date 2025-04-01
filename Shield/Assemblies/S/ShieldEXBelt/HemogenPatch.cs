using HarmonyLib;
using RimWorld;

namespace ShieldEXBelt;

[HarmonyPatch(typeof(Gene_Resource), "ResetMax")]
public class HemogenPatch
{
	[HarmonyPostfix]
	public static void postfix(Gene_Resource __instance)
	{
		Gene_Hemogen firstGeneOfType = __instance.pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
		if (firstGeneOfType == null)
		{
			return;
		}
		foreach (Apparel item in __instance.pawn.apparel.WornApparel)
		{
			CompShieldEx4 comp = item.GetComp<CompShieldEx4>();
			if (comp != null)
			{
				firstGeneOfType.SetMax(firstGeneOfType.Max + comp.HemogenOffset);
				comp.remember = comp.HemogenOffset;
			}
		}
	}
}
