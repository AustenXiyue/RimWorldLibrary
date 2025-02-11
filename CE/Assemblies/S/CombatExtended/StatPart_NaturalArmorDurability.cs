using RimWorld;
using Verse;

namespace CombatExtended;

public class StatPart_NaturalArmorDurability : StatPart
{
	private float getMinArmorExplicit(CompArmorDurability comp)
	{
		if (parentStat == StatDefOf.ArmorRating_Sharp)
		{
			return comp.durabilityProps.MinArmorValueSharp;
		}
		if (parentStat == StatDefOf.ArmorRating_Blunt)
		{
			return comp.durabilityProps.MinArmorValueBlunt;
		}
		if (parentStat == StatDefOf.ArmorRating_Heat)
		{
			return comp.durabilityProps.MinArmorValueHeat;
		}
		if (parentStat == CE_StatDefOf.ArmorRating_Electric)
		{
			return comp.durabilityProps.MinArmorValueElectric;
		}
		return -1f;
	}

	private float getMinArmor(CompArmorDurability comp, float val)
	{
		float minArmorExplicit = getMinArmorExplicit(comp);
		return (minArmorExplicit >= 0f) ? minArmorExplicit : (comp.durabilityProps.MinArmorPct * val);
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (val == 0f)
		{
			return;
		}
		CompArmorDurability compArmorDurability = req.Thing?.TryGetComp<CompArmorDurability>() ?? null;
		if (compArmorDurability != null)
		{
			Pawn pawn = (Pawn)req.Thing;
			float minArmor = getMinArmor(compArmorDurability, val);
			val -= (val - minArmor) * (1f - compArmorDurability.curDurabilityPercent);
			if (val < minArmor)
			{
				val = minArmor;
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		CompArmorDurability compArmorDurability = req.Thing?.TryGetComp<CompArmorDurability>() ?? null;
		if (compArmorDurability != null)
		{
			Pawn pawn = (Pawn)req.Thing;
			string text = ((getMinArmorExplicit(compArmorDurability) >= 0f) ? ("Minimal armor value: " + getMinArmorExplicit(compArmorDurability)) : ("Minimal armor percentage: " + compArmorDurability.durabilityProps.MinArmorPct.ToStringPercent()));
			return "Armor durability: " + compArmorDurability.curDurabilityPercent.ToStringPercent() + "\n" + compArmorDurability.curDurability + "/" + compArmorDurability.maxDurability + "\n" + text;
		}
		return null;
	}
}
