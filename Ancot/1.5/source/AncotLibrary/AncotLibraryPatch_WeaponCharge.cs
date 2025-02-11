using System.Reflection;
using HarmonyLib;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
public class AncotLibraryPatch_WeaponCharge
{
	static AncotLibraryPatch_WeaponCharge()
	{
		Harmony harmony = new Harmony("Ancot.AncotLibraryPatch_WeaponCharge");
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
