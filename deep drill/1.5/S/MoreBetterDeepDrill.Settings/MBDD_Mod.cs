using System.Collections.Generic;
using MoreBetterDeepDrill.Types;
using MoreBetterDeepDrill.Utils;
using UnityEngine;
using Verse;

namespace MoreBetterDeepDrill.Settings;

public class MBDD_Mod : Mod
{
	public static MBDD_Settings ModSetting;

	private Vector2 scrollPosition = Vector2.zero;

	private float scrollViewHeight;

	public MBDD_Mod(ModContentPack content)
		: base(content)
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			ModSetting = GetSettings<MBDD_Settings>();
		});
		LongEventHandler.ExecuteWhenFinished(PushToDatabase);
		LongEventHandler.ExecuteWhenFinished(BuildDictionary);
		LongEventHandler.ExecuteWhenFinished(OreDictionary.Refresh);
	}

	public override string SettingsCategory()
	{
		return StaticValues.MoreBetterDeepDrill;
	}

	private void PushToDatabase()
	{
		ModSetting.database = DefDatabase<ThingDef>.AllDefsListForReading;
	}

	private void BuildDictionary()
	{
		if (ModSetting.oreDictionary == null || ModSetting.oreDictionary.Count <= 0)
		{
			OreDictionary.Build();
			AddExteraDrillable();
		}
	}

	private void AddExteraDrillable()
	{
		List<ThingDef> list = new List<ThingDef>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
		{
			if (item.building != null && (item.building.isResourceRock || item.building.isNaturalRock) && item.mineable)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			OreDictionary.AddExtraDrillable(list);
		}
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		float num = 0f;
		num += 40f;
		Widgets.Checkbox(0f, num, ref ModSetting.EnableInsectoids, 25f);
		Widgets.Label(new Rect(35f, num + 1f, inRect.width - 50f, 25f), "MBDD_Label_EnableInsectoids".Translate());
		num += 40f;
		Widgets.Checkbox(0f, num, ref ModSetting.EnableMechdroids, 25f);
		Widgets.Label(new Rect(35f, num + 1f, inRect.width - 50f, 25f), "MBDD_Label_EnableMechdroids".Translate());
		num += 80f;
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(0f, num, inRect.width - 50f, 30f), "MBDD_Label_OreListedDisplay".Translate());
		num += 40f;
		Text.Font = GameFont.Small;
		if (Widgets.ButtonText(new Rect(0f, num, 290f, 25f), "MBDD_ButtonText_ReBuildOreDictionary".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			OreDictionary.Build(rebuild: true);
			AddExteraDrillable();
		}
		if (ModSetting.oreDictionary == null || ModSetting.oreDictionary.Count <= 0)
		{
			return;
		}
		num += 40f;
		Rect outRect = new Rect(0f, num, 310f, 300f);
		Rect viewRect = new Rect(0f, num, outRect.width - 16f, (float)ModSetting.oreDictionary.Count * 32f);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float num2 = num;
		for (int i = 0; i < ModSetting.oreDictionary.Count; i++)
		{
			List<DrillableOre> oreDictionary = ModSetting.oreDictionary;
			Rect rect = new Rect(0f, num2, viewRect.width, 32f);
			Rect rect2 = rect.LeftPartPixels(32f);
			Rect rect3 = new Rect(rect.x + 35f, rect.y + 5f, rect.width - 32f, rect.height);
			Rect rect4 = new Rect(rect3.x + 185f, rect.y, 65f, rect.height);
			Widgets.ThingIcon(rect2, oreDictionary[i].OreDef, null, null, 1f, null, null);
			Widgets.Label(rect3, oreDictionary[i].OreDef.LabelCap);
			string buffer = oreDictionary[i].amountPerPortion.ToString();
			Widgets.TextFieldNumeric(rect4, ref oreDictionary[i].amountPerPortion, ref buffer);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			TooltipHandler.TipRegion(rect, oreDictionary[i].OreDef.description);
			num2 = (scrollViewHeight = num2 + 32f);
		}
		Widgets.EndScrollView();
	}
}
