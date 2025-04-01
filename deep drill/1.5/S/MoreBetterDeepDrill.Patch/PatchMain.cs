using System.Reflection;
using HarmonyLib;
using Verse;

namespace MoreBetterDeepDrill.Patch;

[StaticConstructorOnStartup]
public class PatchMain
{
	public static Harmony instance;

	static PatchMain()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		instance = new Harmony("MoreBetterDeepDrill.Patch");
		instance.PatchAll(Assembly.GetExecutingAssembly());
		Log.Message("MoreBetterDeepDrill Patched");
	}
}
