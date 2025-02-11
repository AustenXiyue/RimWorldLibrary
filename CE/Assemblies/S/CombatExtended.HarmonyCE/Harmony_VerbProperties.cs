using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(VerbProperties))]
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class Harmony_VerbProperties
{
	private static Dictionary<VerbProperties, bool> cache = new Dictionary<VerbProperties, bool>();

	internal static void Postfix(VerbProperties __instance, ref bool __result)
	{
		if (!__result && !cache.TryGetValue(__instance, out __result))
		{
			lock (cache)
			{
				__result = typeof(Verb_LaunchProjectileCE).IsAssignableFrom(__instance.verbClass);
				cache[__instance] = __result;
			}
		}
	}
}
