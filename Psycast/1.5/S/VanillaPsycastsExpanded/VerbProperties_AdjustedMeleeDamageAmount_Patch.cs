using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[]
{
	typeof(Verb),
	typeof(Pawn)
})]
public static class VerbProperties_AdjustedMeleeDamageAmount_Patch
{
	public static bool multiplyByPawnMeleeSkill;

	private static void Postfix(ref float __result, Verb ownerVerb, Pawn attacker)
	{
		if (attacker != null && multiplyByPawnMeleeSkill)
		{
			__result *= (float)attacker.skills.GetSkill(SkillDefOf.Melee).Level / 10f * attacker.GetStatValue(StatDefOf.PsychicSensitivity);
		}
	}
}
