using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_MeleeDamageBase : StatWorker_MeleeStats
{
	public const float damageVariationMin = 0.5f;

	public const float damageVariationMax = 1.5f;

	public const float damageVariationPerSkillLevel = 0.025f;

	public static float GetDamageVariationMin(Pawn pawn)
	{
		float unskilledReturnValue = 0.5f;
		if (!ShouldUseSkillVariation(pawn, ref unskilledReturnValue))
		{
			return unskilledReturnValue;
		}
		return 0.5f + 0.025f * (float)pawn.skills.GetSkill(SkillDefOf.Melee).Level;
	}

	public static float GetDamageVariationMax(Pawn pawn)
	{
		float unskilledReturnValue = 1.5f;
		if (!ShouldUseSkillVariation(pawn, ref unskilledReturnValue))
		{
			return unskilledReturnValue;
		}
		return 1.5f - 0.025f * (float)(20 - pawn.skills.GetSkill(SkillDefOf.Melee).Level);
	}

	public static bool ShouldUseSkillVariation(Pawn pawn, ref float unskilledReturnValue)
	{
		if (pawn == null)
		{
			return false;
		}
		if ((pawn?.skills?.GetSkill(SkillDefOf.Melee) ?? null) == null)
		{
			unskilledReturnValue = 1f;
			return false;
		}
		return true;
	}

	public static float GetAdjustedDamage(ToolCE tool, Thing thingOwner)
	{
		return tool.AdjustedBaseMeleeDamageAmount(thingOwner, tool.capacities?.First()?.VerbsProperties?.First()?.meleeDamageDef);
	}
}
