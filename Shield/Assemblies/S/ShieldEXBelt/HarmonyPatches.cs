using HarmonyLib;
using Verse;

namespace ShieldEXBelt;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
	static HarmonyPatches()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("HemogenPatch").PatchAll();
	}
}
