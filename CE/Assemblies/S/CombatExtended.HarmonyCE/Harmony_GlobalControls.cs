using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(GlobalControlsUtility), "DoDate")]
internal static class Harmony_GlobalControls
{
	private const float magicExtraOffset = 8f;

	private static void Postfix(ref float curBaseY)
	{
		float num = (float)UI.screenWidth - 200f;
		Find.CurrentMap?.GetComponent<WeatherTracker>().DoWindGUI(num + 8f, ref curBaseY);
	}
}
