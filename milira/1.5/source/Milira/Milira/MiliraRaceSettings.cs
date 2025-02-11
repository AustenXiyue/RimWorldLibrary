using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Milira;

public class MiliraRaceSettings : ModSettings
{
	public static bool MiliraRace_ModSetting_StoryOverall = true;

	public static bool MiliraRace_ModSetting_MilianClusterInMap = true;

	public static bool MiliraRace_ModSetting_MilianSmallClusterInMap = true;

	public static bool MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality = false;

	public static bool MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial = false;

	public static bool MiliraRace_ModSetting_MilianDifficulty_Promotion = true;

	public static bool MiliraRace_ModSetting_MilianDifficulty_FastPromotion = false;

	public static bool MiliraRace_ModSetting_MilianDifficulty_WidePromotion = false;

	public static bool MiliraRace_ModSetting_MilianDifficulty_ClusterResonator = true;

	public static bool MiliraRace_ModSetting_MilianDifficulty_ClusterFortress = false;

	public static bool MiliraRace_ModSetting_MiliraDifficulty_TirelessFly = false;

	public static bool MiliraRace_ModSetting_MilianHairColor = false;

	public static bool MiliraRace_ModSetting_MilianHairColorOffset = false;

	public static MiliraDifficultyScale currentGameDifficulty = MiliraDifficultyScale.Normal;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref MiliraRace_ModSetting_StoryOverall, "MiliraRace_ModSetting_StoryOverall", defaultValue: true, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianClusterInMap, "MiliraRace_ModSetting_MilianClusterInMap", defaultValue: true, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianSmallClusterInMap, "MiliraRace_ModSetting_MilianSmallClusterInMap", defaultValue: true, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality, "MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality", defaultValue: false, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial, "MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial", defaultValue: false, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_Promotion, "MiliraRace_ModSetting_MilianDifficulty_Promotion", defaultValue: true, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_FastPromotion, "MiliraRace_ModSetting_MilianDifficulty_FastPromotion", defaultValue: false, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_WidePromotion, "MiliraRace_ModSetting_MilianDifficulty_WidePromotion", defaultValue: false, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_ClusterResonator, "MiliraRace_ModSetting_MilianDifficulty_ClusterResonator", defaultValue: true, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MilianDifficulty_ClusterFortress, "MiliraRace_ModSetting_MilianDifficulty_ClusterFortress", defaultValue: false, forceSave: true);
		Scribe_Values.Look(ref MiliraRace_ModSetting_MiliraDifficulty_TirelessFly, "MiliraRace_ModSetting_MiliraDifficulty_TirelessFly", defaultValue: false, forceSave: true);
		if (ModsConfig.IdeologyActive)
		{
		}
		if (ModsConfig.RoyaltyActive)
		{
		}
		if (ModsConfig.BiotechActive)
		{
			Scribe_Values.Look(ref MiliraRace_ModSetting_MilianHairColor, "MiliraRace_ModSetting_MilianHairColor", defaultValue: false, forceSave: true);
			Scribe_Values.Look(ref MiliraRace_ModSetting_MilianHairColorOffset, "MiliraRace_ModSetting_MilianHairColorOffset", defaultValue: false, forceSave: true);
		}
		Scribe_Values.Look(ref currentGameDifficulty, "currentGameDifficulty", MiliraDifficultyScale.Normal, forceSave: true);
	}

	public static void DoWindowContents(Rect rect)
	{
		Rect rect2 = new Rect(rect.x, rect.y, rect.width, 30f);
		DrawDifficultySetting(rect2);
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(rect);
		listing_Standard.Gap(5f);
		listing_Standard.Gap(30f);
		listing_Standard.CheckboxLabeled("MiliraRaceSetting_StoryOverall_Label".Translate(), ref MiliraRace_ModSetting_StoryOverall, "MiliraRaceSetting_StoryOverall_Desc".Translate());
		if (MiliraRace_ModSetting_StoryOverall)
		{
		}
		listing_Standard.CheckboxLabeled("MiliraRaceSetting_MilianClusterInMap_Label".Translate(), ref MiliraRace_ModSetting_MilianClusterInMap, "MiliraRaceSetting_MilianClusterInMap_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRaceSetting_MilianSmallClusterInMap_Label".Translate(), ref MiliraRace_ModSetting_MilianSmallClusterInMap, "MiliraRaceSetting_MilianSmallClusterInMap_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality, "MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial, "MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianDifficulty_Promotion_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_Promotion, "MiliraRace_ModSetting_MilianDifficulty_Promotion_Desc".Translate());
		if (MiliraRace_ModSetting_MilianDifficulty_Promotion)
		{
			listing_Standard.CheckboxLabeled("·         " + "MiliraRace_ModSetting_MilianDifficulty_FastPromotion_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_FastPromotion, "MiliraRace_ModSetting_MilianDifficulty_FastPromotion_Desc".Translate());
			listing_Standard.CheckboxLabeled("·         " + "MiliraRace_ModSetting_MilianDifficulty_WidePromotion_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_WidePromotion, "MiliraRace_ModSetting_MilianDifficulty_WidePromotion_Desc".Translate());
		}
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianDifficulty_ClusterResonator_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_ClusterResonator, "MiliraRace_ModSetting_MilianDifficulty_ClusterResonator_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianDifficulty_ClusterFortress_Label".Translate(), ref MiliraRace_ModSetting_MilianDifficulty_ClusterFortress, "MiliraRace_ModSetting_MilianDifficulty_ClusterFortress_Desc".Translate());
		listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MiliraDifficulty_TirelessFly_Label".Translate(), ref MiliraRace_ModSetting_MiliraDifficulty_TirelessFly, "MiliraRace_ModSetting_MiliraDifficulty_TirelessFly_Desc".Translate());
		if (ModsConfig.IdeologyActive)
		{
		}
		if (ModsConfig.RoyaltyActive)
		{
		}
		if (ModsConfig.BiotechActive)
		{
			listing_Standard.CheckboxLabeled("MiliraRace_ModSetting_MilianHairColor_Label".Translate(), ref MiliraRace_ModSetting_MilianHairColor, "MiliraRace_ModSetting_MilianHairColor_Desc".Translate());
			if (MiliraRace_ModSetting_MilianHairColor)
			{
				listing_Standard.CheckboxLabeled("·         " + "MiliraRace_ModSetting_MilianHairColorOffset_Label".Translate(), ref MiliraRace_ModSetting_MilianHairColorOffset, "MiliraRace_ModSetting_MilianHairColorOffset_Desc".Translate());
			}
		}
		listing_Standard.End();
	}

	public static void DrawDifficultySetting(Rect rect)
	{
		if (Widgets.ButtonText(rect, "Milira.DifficultyScaleSetting".Translate() + ": " + CurrentDifficultyScaleLabel(currentGameDifficulty), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (MiliraDifficultyScale scale in Enum.GetValues(typeof(MiliraDifficultyScale)))
			{
				DifficultyScaleInfo(scale, out var label, out var _);
				list.Add(new FloatMenuOption(label, delegate
				{
					DifficultySettingTo(scale);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		TooltipHandler.TipRegion(rect, CurrentDifficultyScaleDesc(currentGameDifficulty));
	}

	public static void DifficultySettingTo(MiliraDifficultyScale scale)
	{
		currentGameDifficulty = scale;
	}

	public static string CurrentDifficultyScaleLabel(MiliraDifficultyScale scale)
	{
		DifficultyScaleInfo(scale, out var label, out var _);
		return label;
	}

	public static string CurrentDifficultyScaleDesc(MiliraDifficultyScale scale)
	{
		DifficultyScaleInfo(scale, out var _, out var desc);
		return desc;
	}

	public static float DifficultyScale(MiliraDifficultyScale scale)
	{
		return scale switch
		{
			MiliraDifficultyScale.Relax => 0.2f, 
			MiliraDifficultyScale.Easy => 0.6f, 
			MiliraDifficultyScale.Normal => 1f, 
			MiliraDifficultyScale.Hard => 1.2f, 
			MiliraDifficultyScale.Crazy => 1.5f, 
			_ => 1f, 
		};
	}

	public static void DifficultyScaleInfo(MiliraDifficultyScale scale, out string label, out string desc)
	{
		label = "";
		desc = "";
		switch (scale)
		{
		case MiliraDifficultyScale.Relax:
			label = "Milira.Difficulty_Relax".Translate();
			desc = "Milira.Difficulty_RelaxDesc".Translate();
			break;
		case MiliraDifficultyScale.Easy:
			label = "Milira.Difficulty_Easy".Translate();
			desc = "Milira.Difficulty_EasyDesc".Translate();
			break;
		case MiliraDifficultyScale.Normal:
			label = "Milira.Difficulty_Normal".Translate();
			desc = "Milira.Difficulty_NormalDesc".Translate();
			break;
		case MiliraDifficultyScale.Hard:
			label = "Milira.Difficulty_Hard".Translate();
			desc = "Milira.Difficulty_HardDesc".Translate();
			break;
		case MiliraDifficultyScale.Crazy:
			label = "Milira.Difficulty_Crazy".Translate();
			desc = "Milira.Difficulty_CrazyDesc".Translate();
			break;
		}
	}
}
