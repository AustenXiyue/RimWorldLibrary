using UnityEngine;
using Verse;

namespace OP;

public class OP_Mod : Mod
{
	public static OP_Settings ModSetting;

	public OP_Mod(ModContentPack content)
		: base(content)
	{
		ModSetting = GetSettings<OP_Settings>();
	}

	public override string SettingsCategory()
	{
		return "OP_SettingsCategoryLabel".Translate();
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(inRect);
		listing_Standard.CheckboxLabeled("OP_EnableClearEntropy_Label".Translate(), ref ModSetting.EnableClearEntropy);
		listing_Standard.End();
		ModSetting.Write();
	}
}
