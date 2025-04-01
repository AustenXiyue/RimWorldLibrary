using UnityEngine;
using Verse;

namespace RealDeathless;

public class Utilities_ModSettings : ModSettings
{
	public static bool setting_RD_Patch_ShouldBeDead = true;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref setting_RD_Patch_ShouldBeDead, "setting_RD_Patch_ShouldBeDead", defaultValue: true, forceSave: true);
	}

	public static void DoWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(inRect);
		listing_Standard.Label("stringRD_Setting_Global".Translate());
		listing_Standard.CheckboxLabeled("stringRD_Patch_ShouldBeDead".Translate(), ref setting_RD_Patch_ShouldBeDead, "stringRD_Patch_ShouldBeDead_Hint".Translate());
		listing_Standard.End();
	}
}
