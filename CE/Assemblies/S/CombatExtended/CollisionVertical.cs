using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public struct CollisionVertical
{
	public const float ThickRoofThicknessMultiplier = 2f;

	public const float NaturalRoofThicknessMultiplier = 2f;

	public const float MeterPerCellHeight = 1.75f;

	public const float WallCollisionHeight = 2f;

	public const float BodyRegionBottomHeight = 0.45f;

	public const float BodyRegionMiddleHeight = 0.85f;

	private readonly FloatRange heightRange;

	public readonly float shotHeight;

	public FloatRange HeightRange => new FloatRange(heightRange.min, heightRange.max);

	public float Min => heightRange.min;

	public float Max => heightRange.max;

	public float BottomHeight => Max * 0.45f;

	public float MiddleHeight => Max * 0.85f;

	public CollisionVertical(Thing thing)
	{
		CalculateHeightRange(thing, out heightRange, out shotHeight);
	}

	private static void CalculateHeightRange(Thing thing, out FloatRange heightRange, out float shotHeight)
	{
		shotHeight = 0f;
		heightRange = new FloatRange(0f, 0f);
		if (thing == null)
		{
			return;
		}
		if (thing is Plant plant)
		{
			heightRange = new FloatRange(0f, BoundsInjector.ForPlant(plant).y);
			return;
		}
		if (thing is Building)
		{
			if (!(thing is Building_Door { Open: not false }))
			{
				if (thing.def.Fillage == FillCategory.Full)
				{
					heightRange = new FloatRange(0f, 2f);
					shotHeight = 2f;
				}
				else
				{
					float fillPercent = thing.def.fillPercent;
					heightRange = new FloatRange(Mathf.Min(0f, fillPercent), Mathf.Max(0f, fillPercent));
					shotHeight = fillPercent;
				}
			}
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float heightAdjust = CETrenches.GetHeightAdjust(thing.Position, thing.Map);
		if (thing is Pawn pawn)
		{
			num = CE_Utility.GetCollisionBodyFactors(pawn).y;
			num2 = num * 0.14999998f;
			if (pawn.IsCrouching())
			{
				float num3 = 0.45f * num;
				Map map = pawn.Map;
				foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(pawn.Position))
				{
					if (!item.InBounds(map))
					{
						continue;
					}
					Thing cover = item.GetCover(map);
					if (cover != null && cover.def.Fillage == FillCategory.Partial && !cover.IsPlant())
					{
						float num4 = new CollisionVertical(cover).Max - heightAdjust;
						if (num4 > num3)
						{
							num3 = num4;
						}
					}
				}
				num = Mathf.Min(num, num3 + 0.01f + num2);
			}
		}
		else
		{
			num = thing.def.fillPercent;
		}
		float num5 = 0f;
		if (thing.Map != null)
		{
			Thing cover2 = thing.Position.GetCover(thing.Map);
			if (cover2 != null && cover2.GetHashCode() != thing.GetHashCode() && !cover2.IsPlant())
			{
				num5 = new CollisionVertical(cover2).heightRange.max;
			}
		}
		float num6 = num;
		heightRange = new FloatRange(Mathf.Min(num5, num5 + num6) + heightAdjust, Mathf.Max(num5, num5 + num6) + heightAdjust);
		shotHeight = heightRange.max - num2;
	}

	public BodyPartHeight GetCollisionBodyHeight(float projectileHeight)
	{
		if (projectileHeight < BottomHeight)
		{
			return BodyPartHeight.Bottom;
		}
		if (projectileHeight < MiddleHeight)
		{
			return BodyPartHeight.Middle;
		}
		return BodyPartHeight.Top;
	}
}
