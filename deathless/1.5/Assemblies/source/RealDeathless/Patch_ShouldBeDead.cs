using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RealDeathless;

[StaticConstructorOnStartup]
public class Patch_ShouldBeDead
{
	static Patch_ShouldBeDead()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		if (Utilities_ModSettings.setting_RD_Patch_ShouldBeDead)
		{
			RealDeathless.harmonyInstance.Patch((MethodBase)AccessTools.Method(typeof(Pawn_HealthTracker), "ShouldBeDead", (Type[])null, (Type[])null), new HarmonyMethod(typeof(Patch_ShouldBeDead), "Prefix_ShouldBeDead", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	private static bool Prefix_ShouldBeDead(Pawn_HealthTracker __instance, ref bool __result)
	{
		Pawn value = Traverse.Create((object)__instance).Field("pawn").GetValue<Pawn>();
		if (value.IsColonistPlayerControlled && value.genes != null && value.genes.HasGene(DefDatabase<GeneDef>.GetNamed("RealDeathless")))
		{
			__result = false;
		}
		else
		{
			__result = true;
		}
		return __result;
	}
}
