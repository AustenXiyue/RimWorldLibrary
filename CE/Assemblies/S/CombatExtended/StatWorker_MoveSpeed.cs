using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

internal class StatWorker_MoveSpeed : StatWorker
{
	private const float CrouchWalkFactor = 0.67f;

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetExplanationUnfinalized(req, numberSense));
		if (req.HasThing)
		{
			CompInventory compInventory = req.Thing.TryGetComp<CompInventory>();
			if (compInventory != null)
			{
				stringBuilder.AppendLine();
				if (stat.defName != "MeleeDodgeChance")
				{
					stringBuilder.AppendLine("CE_CarriedWeight".Translate() + ": x" + compInventory.moveSpeedFactor.ToStringPercent());
				}
				else
				{
					stringBuilder.AppendLine("CE_CarriedWeight".Translate() + ": x" + MassBulkUtility.DodgeWeightFactor(compInventory.currentWeight, compInventory.capacityWeight).ToStringPercent());
					stringBuilder.AppendLine("CE_BulkEffect".Translate() + " x" + (MassBulkUtility.DodgeChanceFactor(compInventory.currentBulk, compInventory.capacityBulk) * 100f).ToString() + "%");
				}
				if (compInventory.encumberPenalty > 0f)
				{
					stringBuilder.AppendLine("CE_Encumbered".Translate() + ": -" + compInventory.encumberPenalty.ToStringPercent());
				}
				if (stat.defName != "MeleeDodgeChance")
				{
					stringBuilder.AppendLine("CE_FinalModifier".Translate() + ": x" + GetStatFactor(req.Thing).ToStringPercent());
				}
			}
			CompSuppressable compSuppressable = req.Thing.TryGetComp<CompSuppressable>();
			if (compSuppressable != null && compSuppressable.IsCrouchWalking)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(string.Format("{0}: x{1}", "CE_CrouchWalking".Translate(), 0.67f.ToStringPercent()));
			}
		}
		return stringBuilder.ToString();
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		float num = base.GetValueUnfinalized(req, applyPostProcess);
		if (req.HasThing)
		{
			CompInventory compInventory = req.Thing.TryGetComp<CompInventory>();
			if (stat.defName != "MeleeDodgeChance" && compInventory != null)
			{
				num *= GetStatFactor(req.Thing);
			}
			if (stat.defName == "MeleeDodgeChance" && compInventory != null)
			{
				num *= MassBulkUtility.DodgeChanceFactor(compInventory.currentBulk, compInventory.capacityBulk);
				num *= MassBulkUtility.DodgeWeightFactor(compInventory.currentWeight, compInventory.capacityWeight);
				if (compInventory.currentWeight > compInventory.capacityWeight)
				{
					num -= compInventory.encumberPenalty;
				}
			}
		}
		return num;
	}

	private float GetStatFactor(Thing thing)
	{
		float num = 1f;
		CompInventory compInventory = thing.TryGetComp<CompInventory>();
		if (compInventory != null)
		{
			num = Mathf.Clamp(compInventory.moveSpeedFactor - compInventory.encumberPenalty, 0.5f, 1f);
		}
		CompSuppressable compSuppressable = thing.TryGetComp<CompSuppressable>();
		if (compSuppressable != null && compSuppressable.IsCrouchWalking)
		{
			num *= 0.67f;
		}
		return num;
	}
}
