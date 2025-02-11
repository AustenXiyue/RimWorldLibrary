using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_WorkSpeedGlobal : StatWorker
{
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
				stringBuilder.AppendLine("CE_CarriedBulk".Translate() + ": x" + compInventory.workSpeedFactor.ToStringPercent());
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
			if (compInventory != null)
			{
				num *= compInventory.workSpeedFactor;
			}
		}
		return num;
	}
}
