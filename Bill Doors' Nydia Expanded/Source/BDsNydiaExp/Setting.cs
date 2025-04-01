using UnityEngine;
using Verse;

namespace BDsNydiaExp
{
    public class Setting : ModSettings
    {
        public bool noWings = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref noWings, "noWings");
        }
    }
    public class BDsNydiaExp_Mod : Mod
    {
        public static Setting settings;
        public static bool NoWings => settings.noWings;

        public BDsNydiaExp_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("BDsNydiaExp_NoWingsLabel".Translate(), ref settings.noWings, "BDsNydiaExp_NoWingsDesc".Translate());
            listing_Standard.End();
        }
        public override string SettingsCategory()
        {
            return "BDsNydiaExp_Setting".Translate();
        }
    }
}
