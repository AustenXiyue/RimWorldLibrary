using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class MassBulkUtility
{
	public const float WeightCapacityPerBodySize = 35f;

	public const float BulkCapacityPerBodySize = 20f;

	public static float BaseCarryWeight(Pawn p)
	{
		return p.BodySize * 35f;
	}

	public static float BaseCarryBulk(Pawn p)
	{
		return p.BodySize * 20f;
	}

	public static float MoveSpeedFactor(float weight, float weightCapacity)
	{
		float num = weight / weightCapacity;
		if (float.IsNaN(num))
		{
			num = 1f;
		}
		if (num <= 0.25f)
		{
			return 1f;
		}
		return Mathf.Lerp(1f, 0.75f, num);
	}

	public static float WorkSpeedFactor(float bulk, float bulkCapacity)
	{
		if (bulk / bulkCapacity <= 0.35f)
		{
			return 1f;
		}
		return Mathf.Lerp(1f, 0.75f, bulk / bulkCapacity - 0.35f);
	}

	public static float DodgeChanceFactor(float bulk, float bulkCapacity)
	{
		if (bulk / bulkCapacity <= 0.5f)
		{
			return 1f;
		}
		return (float)Math.Round(Mathf.Lerp(1f, 0.87f, Math.Min(bulk / bulkCapacity, 1f)), 2);
	}

	public static float HitChanceBulkFactor(float bulk, float bulkCapacity)
	{
		if (bulk / bulkCapacity <= 0.25f)
		{
			return 1f;
		}
		return (float)Math.Round(Mathf.Lerp(1f, 0.75f, Math.Min(bulk / bulkCapacity, 1f)), 2);
	}

	public static float DodgeWeightFactor(float weight, float weightCapacity)
	{
		if (weight / weightCapacity <= 0.25f)
		{
			return 1f;
		}
		return (float)Math.Round(Mathf.Lerp(1f, 0.87f, Math.Min(weight / weightCapacity, 1f)), 2);
	}

	public static float EncumberPenalty(float weight, float weightCapacity)
	{
		if (weight > weightCapacity)
		{
			float num = weight / weightCapacity;
			if (float.IsPositiveInfinity(num))
			{
				return 1f;
			}
			return num - 1f;
		}
		return 0f;
	}
}
