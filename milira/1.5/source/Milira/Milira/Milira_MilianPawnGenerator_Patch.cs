using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Milira;

[HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) })]
[HarmonyPatch(typeof(PawnGenerator))]
[HarmonyPatch("GeneratePawn")]
public static class Milira_MilianPawnGenerator_Patch
{
	private static readonly SimpleCurve MilianDifficultyToEliteChance = new SimpleCurve
	{
		new CurvePoint(0f, 0.01f),
		new CurvePoint(1f, 0.02f),
		new CurvePoint(2.2f, 0.06f),
		new CurvePoint(5f, 0.2f)
	};

	[HarmonyPostfix]
	public static void Postfix(ref Pawn __result, PawnGenerationRequest request)
	{
		if (!MilianUtility.IsMilian(__result))
		{
			return;
		}
		List<ThingDef> apparelRequired = request.KindDef.apparelRequired;
		if (apparelRequired == null)
		{
			return;
		}
		if (request.Faction.def.defName == "Milira_Faction")
		{
			float threatScale = Find.Storyteller.difficulty.threatScale;
			Map map = Find.CurrentMap;
			if (map != null && map.IsPocketMap)
			{
				map = Find.AnyPlayerHomeMap;
			}
			float playerWealthForStoryteller = map.PlayerWealthForStoryteller;
			if (MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_EquipmentMaterial && Rand.Chance(MilianDifficultyToEliteChance.Evaluate(threatScale)))
			{
				if (MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality)
				{
					PawnGenerationRequest request2 = request;
					bool forceNormalGearQuality = true;
					QualityCategory value = QualityCategory.Normal;
					ThingDef milira_SplendidSteel = MiliraDefOf.Milira_SplendidSteel;
					request2.KindDef.forceNormalGearQuality = false;
					if (MilianUtility.IsMilian_KnightClass(__result) || MilianUtility.IsMilian_RookClass(__result))
					{
						request2.KindDef.weaponStuffOverride = MiliraDefOf.Milira_SunPlateSteel;
					}
					if (playerWealthForStoryteller > 300000f && playerWealthForStoryteller <= 600000f)
					{
						request2.KindDef.forceWeaponQuality = QualityCategory.Good;
					}
					else if (playerWealthForStoryteller > 600000f && playerWealthForStoryteller <= 900000f)
					{
						request2.KindDef.forceWeaponQuality = QualityCategory.Excellent;
					}
					else if (playerWealthForStoryteller > 900000f && playerWealthForStoryteller <= 1500000f)
					{
						request2.KindDef.forceWeaponQuality = QualityCategory.Masterwork;
					}
					else if (playerWealthForStoryteller > 1500000f)
					{
						request2.KindDef.forceWeaponQuality = QualityCategory.Legendary;
					}
					else
					{
						request2.KindDef.forceWeaponQuality = QualityCategory.Normal;
					}
					PawnWeaponGenerator.TryGenerateWeaponFor(__result, request2);
					request2.KindDef.forceNormalGearQuality = forceNormalGearQuality;
					request2.KindDef.forceWeaponQuality = value;
					if (MilianUtility.IsMilian_KnightClass(__result) || MilianUtility.IsMilian_RookClass(__result))
					{
						request2.KindDef.weaponStuffOverride = milira_SplendidSteel;
					}
				}
				for (int i = 0; i < apparelRequired.Count; i++)
				{
					Apparel apparel = (Apparel)ThingMaker.MakeThing(apparelRequired[i], GenStuff.DefaultStuffFor(apparelRequired[i]));
					if (MiliraDefOf.Milira_SunPlateSteel.stuffProps.CanMake(apparelRequired[i]))
					{
						apparel = (Apparel)ThingMaker.MakeThing(apparelRequired[i], MiliraDefOf.Milira_SunPlateSteel);
					}
					if (MiliraDefOf.Milira_Feather.stuffProps.CanMake(apparelRequired[i]))
					{
						apparel = (Apparel)ThingMaker.MakeThing(apparelRequired[i], MiliraDefOf.Milira_Feather);
					}
					if (MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality)
					{
						QualityCategory q = QualityCategory.Normal;
						if (playerWealthForStoryteller > 300000f && playerWealthForStoryteller <= 600000f)
						{
							q = QualityCategory.Good;
						}
						else if (playerWealthForStoryteller > 600000f && playerWealthForStoryteller <= 900000f)
						{
							q = QualityCategory.Excellent;
						}
						else if (playerWealthForStoryteller > 900000f && playerWealthForStoryteller <= 1500000f)
						{
							q = QualityCategory.Masterwork;
						}
						else if (playerWealthForStoryteller > 1500000f)
						{
							q = QualityCategory.Legendary;
						}
						apparel.TryGetComp<CompQuality>()?.SetQuality(q, ArtGenerationContext.Colony);
					}
					__result.apparel.Wear(apparel, dropReplacedApparel: true, locked: true);
				}
				return;
			}
			if (MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality)
			{
				PawnGenerationRequest request3 = request;
				bool forceNormalGearQuality2 = true;
				QualityCategory value2 = QualityCategory.Normal;
				request3.KindDef.forceNormalGearQuality = false;
				if (playerWealthForStoryteller > 300000f && playerWealthForStoryteller <= 600000f)
				{
					request3.KindDef.forceWeaponQuality = QualityCategory.Good;
				}
				else if (playerWealthForStoryteller > 600000f && playerWealthForStoryteller <= 900000f)
				{
					request3.KindDef.forceWeaponQuality = QualityCategory.Excellent;
				}
				else if (playerWealthForStoryteller > 900000f && playerWealthForStoryteller <= 1500000f)
				{
					request3.KindDef.forceWeaponQuality = QualityCategory.Masterwork;
				}
				else if (playerWealthForStoryteller > 1500000f)
				{
					request3.KindDef.forceWeaponQuality = QualityCategory.Legendary;
				}
				else
				{
					request3.KindDef.forceWeaponQuality = QualityCategory.Normal;
				}
				PawnWeaponGenerator.TryGenerateWeaponFor(__result, request3);
				request3.KindDef.forceNormalGearQuality = forceNormalGearQuality2;
				request3.KindDef.forceWeaponQuality = value2;
			}
			for (int j = 0; j < apparelRequired.Count; j++)
			{
				Apparel apparel2 = (Apparel)ThingMaker.MakeThing(apparelRequired[j], GenStuff.DefaultStuffFor(apparelRequired[j]));
				if (MiliraDefOf.Milira_SplendidSteel.stuffProps.CanMake(apparelRequired[j]))
				{
					apparel2 = (Apparel)ThingMaker.MakeThing(apparelRequired[j], MiliraDefOf.Milira_SplendidSteel);
				}
				if (MiliraDefOf.Milira_FeatherThread.stuffProps.CanMake(apparelRequired[j]))
				{
					apparel2 = (Apparel)ThingMaker.MakeThing(apparelRequired[j], MiliraDefOf.Milira_FeatherThread);
				}
				if (MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_EquipmentQuality)
				{
					QualityCategory q2 = QualityCategory.Normal;
					if (playerWealthForStoryteller > 300000f && playerWealthForStoryteller <= 600000f)
					{
						q2 = QualityCategory.Good;
					}
					else if (playerWealthForStoryteller > 600000f && playerWealthForStoryteller <= 900000f)
					{
						q2 = QualityCategory.Excellent;
					}
					else if (playerWealthForStoryteller > 900000f && playerWealthForStoryteller <= 1500000f)
					{
						q2 = QualityCategory.Masterwork;
					}
					else if (playerWealthForStoryteller > 1500000f)
					{
						q2 = QualityCategory.Legendary;
					}
					apparel2.TryGetComp<CompQuality>()?.SetQuality(q2, ArtGenerationContext.Colony);
				}
				__result.apparel.Wear(apparel2, dropReplacedApparel: true, locked: true);
			}
			return;
		}
		for (int k = 0; k < apparelRequired.Count; k++)
		{
			Apparel newApparel = (Apparel)ThingMaker.MakeThing(apparelRequired[k], GenStuff.DefaultStuffFor(apparelRequired[k]));
			if (MiliraDefOf.Milira_SplendidSteel.stuffProps.CanMake(apparelRequired[k]))
			{
				newApparel = (Apparel)ThingMaker.MakeThing(apparelRequired[k], MiliraDefOf.Milira_SplendidSteel);
			}
			if (MiliraDefOf.Milira_FeatherThread.stuffProps.CanMake(apparelRequired[k]))
			{
				newApparel = (Apparel)ThingMaker.MakeThing(apparelRequired[k], MiliraDefOf.Milira_FeatherThread);
			}
			__result.apparel.Wear(newApparel, dropReplacedApparel: true, locked: true);
		}
	}
}
