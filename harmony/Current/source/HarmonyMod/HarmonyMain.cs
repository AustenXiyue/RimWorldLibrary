using System;
using System.Reflection;
using HarmonyLib;
using LudeonTK;
using Verse;

namespace HarmonyMod;

[StaticConstructorOnStartup]
public class HarmonyMain : Mod
{
	[TweakValue("Harmony", 0f, 100f)]
	public static bool noStacktraceCaching;

	[TweakValue("Harmony", 0f, 100f)]
	public static bool noStacktraceEnhancing;

	public static Version harmonyVersion;

	public static string modVersion;

	public HarmonyMain(ModContentPack content)
		: base(content)
	{
	}

	static HarmonyMain()
	{
		harmonyVersion = null;
		modVersion = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute), inherit: false)).Version;
		Harmony.VersionInfo(out harmonyVersion);
		new Harmony("net.pardeike.rimworld.lib.harmony").PatchAll();
	}
}
