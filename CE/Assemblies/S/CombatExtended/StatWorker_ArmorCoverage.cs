using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_ArmorCoverage : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		return Controller.settings.ShowExtraStats && req.HasThing && (req.Thing as Pawn)?.apparel != null;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		float num = 0f;
		Pawn pawn = (Pawn)req.Thing;
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			float humanBodyCoverage = item.def.apparel.HumanBodyCoverage;
			num += item.GetStatValue(StatDefOf.ArmorRating_Sharp) * humanBodyCoverage;
		}
		return num;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetExplanationUnfinalized(req, numberSense));
		Pawn pawn = (Pawn)req.Thing;
		if (pawn.apparel.WornApparelCount > 0)
		{
			stringBuilder.AppendLine();
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				stringBuilder.AppendLine($"{item.LabelCap}: {item.GetStatValue(StatDefOf.ArmorRating_Sharp)} x {item.def.apparel.HumanBodyCoverage.ToStringPercent()}");
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString().Trim();
	}
}
