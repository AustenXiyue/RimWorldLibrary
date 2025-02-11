using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE.Compatibility;

[HarmonyPatch]
internal class GraphicApparelDetour_Disable
{
	private static List<Assembly> target_asses = new List<Assembly>();

	private static bool Prepare()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("GraphicApparelDetour"))
			{
				target_asses.Add(assembly);
			}
		}
		if (target_asses.Any())
		{
			return true;
		}
		return false;
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		ReportOffendingDetourMods();
		foreach (Assembly ass in target_asses)
		{
			Type[] types = ass.GetTypes();
			foreach (Type type in types)
			{
				MethodBase method_info = null;
				if (type.Name.Contains("InjectorThingy"))
				{
					method_info = AccessTools.Method(type, "InjectStuff", (Type[])null, (Type[])null);
				}
				if (method_info != null)
				{
					yield return method_info;
				}
			}
		}
	}

	private static bool Prefix()
	{
		return false;
	}

	private static void ReportOffendingDetourMods()
	{
		List<string> list = new List<string>();
		foreach (ModContentPack runningMod in LoadedModManager.RunningMods)
		{
			foreach (Assembly loadedAssembly in runningMod.assemblies.loadedAssemblies)
			{
				if (target_asses.Contains(loadedAssembly))
				{
					list.Add(runningMod.Name);
				}
			}
		}
		if (!list.Any())
		{
			return;
		}
		bool flag = list.Count > 1;
		Log.Error("Combat Extended:: An incompatible and outdated detour has been detected and disabled in the following mod" + (flag ? "s" : "") + ":");
		foreach (string item in list)
		{
			Log.Error("   " + item);
		}
		Log.Error("Please ask the developer" + (flag ? "s" : "") + " of " + (flag ? "these mods" : "this mod") + " to update " + (flag ? "them" : "it") + "  with a more compatible patching method, such as the Harmony library.");
	}
}
