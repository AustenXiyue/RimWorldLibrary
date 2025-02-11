using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(Corpse))]
[HarmonyPatch("ButcherProducts")]
public static class Milira_CorpseButch_Patch
{
	[HarmonyPostfix]
	public static void Postfix(Corpse __instance, ref IEnumerable<Thing> __result, Pawn butcher, float efficiency)
	{
		Pawn innerPawn = __instance.InnerPawn;
		SimpleCurve simpleCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(5f, 0f),
			new CurvePoint(20f, 0.6f)
		};
		List<Thing> list = new List<Thing>();
		if (!MilianUtility.IsMilian(innerPawn))
		{
			return;
		}
		if ((butcher.RaceProps.Humanlike && butcher.skills.GetSkill(SkillDefOf.Crafting) != null && Rand.Chance(simpleCurve.Evaluate(butcher.skills.GetSkill(SkillDefOf.Crafting).Level))) || (butcher.def.defName == "Milira_Race" && butcher.skills.GetSkill(SkillDefOf.Crafting).Level > 6) || (!butcher.RaceProps.Humanlike && Rand.Chance(0.1f)))
		{
			if (MilianUtility.IsMilian_PawnClass(innerPawn))
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_Pawn));
			}
			else if (MilianUtility.IsMilian_KnightClass(innerPawn))
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_Knight));
			}
			else if (MilianUtility.IsMilian_BishopClass(innerPawn))
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_Bishop));
			}
			else if (MilianUtility.IsMilian_RookClass(innerPawn))
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_Rook));
			}
			else if (innerPawn.def.defName == "Milian_Mechanoid_Queen")
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_Queen));
			}
			else if (innerPawn.def.defName == "Milian_Mechanoid_King")
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milian_NamePlate_King));
			}
			if (Rand.Chance(0.15f))
			{
				list.Add(ThingMaker.MakeThing(MiliraDefOf.Milira_SolarCrystal));
			}
		}
		__result = __result.Concat(list);
	}
}
