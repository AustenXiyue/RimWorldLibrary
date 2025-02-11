using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(ThingDef), "PostLoad")]
internal static class PostLoad_PostFix
{
	[HarmonyPostfix]
	public static void Postfix(ThingDef __instance)
	{
		if (!__instance.HasModExtension<PartialArmorExt>())
		{
			return;
		}
		List<DefModExtension> list = __instance.modExtensions.Where((DefModExtension e) => e.GetType() == typeof(PartialArmorExt)).ToList();
		if (list.Count > 1)
		{
			for (int i = 0; i < list.Count - 1; i++)
			{
				__instance.modExtensions.Remove(list[i]);
			}
		}
	}
}
