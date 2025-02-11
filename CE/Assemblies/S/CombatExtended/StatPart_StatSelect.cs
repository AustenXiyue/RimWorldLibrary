using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace CombatExtended;

public abstract class StatPart_StatSelect : StatPart
{
	public bool includeWeapons;

	public StatDef weaponStat = null;

	public StatDef apparelStat;

	public bool sumApparelsStat = false;

	public StatDef implantStat = null;

	public bool sumImplantsStat = true;

	private Dictionary<EquipmentStatKey, float> cachedStats = new Dictionary<EquipmentStatKey, float>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float GetEquipmentStat(ThingWithComps equipment, StatDef stat)
	{
		EquipmentStatKey key = new EquipmentStatKey(equipment);
		float result;
		if (!cachedStats.TryGetValue(key, out var value))
		{
			float num = (cachedStats[key] = equipment.GetStatValue(stat));
			result = num;
		}
		else
		{
			result = value;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float GetAddedPartStat(Hediff_AddedPart addedPart)
	{
		return addedPart.def.spawnThingOnRemoved.GetStatValueAbstract(implantStat);
	}

	protected abstract float Select(float first, float second);

	public override void TransformValue(StatRequest req, ref float result)
	{
		if (!req.HasThing || !(req.Thing is Pawn pawn))
		{
			return;
		}
		if (apparelStat != null && pawn.apparel != null)
		{
			if (sumApparelsStat)
			{
				float num = 0f;
				ThingOwner<Apparel> wornApparel = pawn.apparel.wornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					num += GetEquipmentStat(wornApparel[i], apparelStat);
				}
				result = Select(num, result);
			}
			else
			{
				ThingOwner<Apparel> wornApparel2 = pawn.apparel.wornApparel;
				for (int j = 0; j < wornApparel2.Count; j++)
				{
					result = Select(result, GetEquipmentStat(wornApparel2[j], apparelStat));
				}
			}
		}
		if (weaponStat != null && pawn.equipment != null && pawn.equipment.Primary != null)
		{
			result = Select(result, GetEquipmentStat(pawn.equipment.Primary, weaponStat));
		}
		if (implantStat == null)
		{
			return;
		}
		if (sumImplantsStat)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			float num2 = 0f;
			for (int k = 0; k < hediffs.Count; k++)
			{
				if (hediffs[k] is Hediff_AddedPart hediff_AddedPart && hediff_AddedPart.def.spawnThingOnRemoved != null)
				{
					num2 += GetAddedPartStat(hediff_AddedPart);
				}
			}
			result = Select(num2, result);
			return;
		}
		List<Hediff> hediffs2 = pawn.health.hediffSet.hediffs;
		for (int l = 0; l < hediffs2.Count; l++)
		{
			if (hediffs2[l] is Hediff_AddedPart hediff_AddedPart2 && hediff_AddedPart2.def.spawnThingOnRemoved != null)
			{
				result = Select(result, GetAddedPartStat(hediff_AddedPart2));
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Pawn != null)
		{
			return "";
		}
		return null;
	}
}
