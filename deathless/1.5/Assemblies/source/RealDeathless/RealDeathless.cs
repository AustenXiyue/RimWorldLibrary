using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RealDeathless;

public class RealDeathless : Mod
{
	public static Harmony harmonyInstance;

	public RealDeathless(ModContentPack modContent)
		: base(modContent)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		GetSettings<Utilities_ModSettings>();
		harmonyInstance = new Harmony("StarEngraver.RealDeathless");
		harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
	}

	public override string SettingsCategory()
	{
		return "ModRealDeathless".Translate();
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Utilities_ModSettings.DoWindowContents(inRect);
	}
}
