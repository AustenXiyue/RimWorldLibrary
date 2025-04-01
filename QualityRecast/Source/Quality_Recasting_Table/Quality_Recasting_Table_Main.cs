using UnityEngine;
using Verse;

namespace Quality_Recasting_Table;

[StaticConstructorOnStartup]
public class Quality_Recasting_Table_Main : Mod
{
	public Quality_Recasting_Table_Setting Quality_Recasting_Table_Setting;

	public static Quality_Recasting_Table_Main Instance { get; private set; }

	public Quality_Recasting_Table_Main(ModContentPack content)
		: base(content)
	{
		Quality_Recasting_Table_Setting = GetSettings<Quality_Recasting_Table_Setting>();
		Instance = this;
	}

	public override string SettingsCategory()
	{
		return "settingtitle".Translate();
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Rect rect = new Rect(0f, 0f, 0.6f * inRect.width, 0.8f * inRect.height);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(rect);
		listing_Standard.Gap(60f);
		if (listing_Standard.ButtonText("Restore_Defaults".Translate()))
		{
			Quality_Recasting_Table_Setting.InitData();
		}
		listing_Standard.Gap(40f);
		listing_Standard.CheckboxLabeled("Send_envelope".Translate(), ref Quality_Recasting_Table_Setting.enableEnvelope, "Send_envelope_hint".Translate());
		listing_Standard.Gap(40f);
		listing_Standard.Label("Probability_settings".Translate(), -1f, "Probability_settings_hint".Translate());
		Quality_Recasting_Table_Setting.recastprobabilitysetting = Mathf.RoundToInt(listing_Standard.Slider(Quality_Recasting_Table_Setting.recastprobabilitysetting, 1f, 3f));
		listing_Standard.Label(Quality_Recasting_Table_Setting.recastprobabilitysetting.ToString());
		listing_Standard.Gap(40f);
		listing_Standard.Label("Spending_quantity".Translate(), -1f, "Spending_quantity_hint".Translate());
		Quality_Recasting_Table_Setting.fuelspent = Mathf.RoundToInt(listing_Standard.Slider(Quality_Recasting_Table_Setting.fuelspent, 50f, 500f));
		listing_Standard.Label(Quality_Recasting_Table_Setting.fuelspent.ToString());
		listing_Standard.End();
	}
}
