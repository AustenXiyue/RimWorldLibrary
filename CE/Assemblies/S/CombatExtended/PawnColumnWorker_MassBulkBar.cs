using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class PawnColumnWorker_MassBulkBar : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	private const string BarMass = "CEMass";

	private const string BarBulk = "CEBulk";

	private static int _MinWidth = 40;

	private static int _OptimalWidth = 50;

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null)
		{
			Rect canvas = new Rect(rect.x, rect.y + 2f, rect.width, rect.height - 4f);
			if (def.defName.Equals("CEBulk"))
			{
				Utility_Loadouts.DrawBar(canvas, compInventory.currentBulk, compInventory.capacityBulk, "", pawn.GetBulkTip());
			}
			if (def.defName.Equals("CEMass"))
			{
				Utility_Loadouts.DrawBar(canvas, compInventory.currentWeight, compInventory.capacityWeight, "", pawn.GetWeightTip());
			}
		}
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(_MinWidth));
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(Mathf.CeilToInt(_OptimalWidth), GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 65);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		CompInventory compInventory = a.TryGetComp<CompInventory>();
		CompInventory compInventory2 = b.TryGetComp<CompInventory>();
		if (compInventory == null || compInventory2 == null)
		{
			return 0;
		}
		if (def.defName.Equals("CEBulk"))
		{
			return (compInventory.capacityBulk - compInventory.currentBulk).CompareTo(compInventory2.capacityBulk - compInventory2.currentBulk);
		}
		if (def.defName.Equals("CEMass"))
		{
			return (compInventory.capacityWeight - compInventory.currentWeight).CompareTo(compInventory2.capacityWeight - compInventory2.currentWeight);
		}
		return 0;
	}
}
