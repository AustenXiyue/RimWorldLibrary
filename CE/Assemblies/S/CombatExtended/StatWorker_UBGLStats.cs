using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_UBGLStats : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		CompProperties_UnderBarrel compProperties_UnderBarrel = req.Thing?.TryGetComp<CompUnderBarrel>()?.Props ?? null;
		if (compProperties_UnderBarrel == null && req.Def != null)
		{
			compProperties_UnderBarrel = (req.Def as ThingDef)?.GetCompProperties<CompProperties_UnderBarrel>() ?? null;
		}
		return base.ShouldShowFor(req) && compProperties_UnderBarrel != null;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		CompProperties_UnderBarrel compProperties_UnderBarrel = req.Thing?.TryGetComp<CompUnderBarrel>()?.Props ?? null;
		if (compProperties_UnderBarrel == null)
		{
			compProperties_UnderBarrel = ((ThingDef)req.Def)?.GetCompProperties<CompProperties_UnderBarrel>() ?? null;
		}
		if (compProperties_UnderBarrel != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Concat("WarmupTime".Translate() + ": ", compProperties_UnderBarrel.verbPropsUnderBarrel.warmupTime.ToString()));
			stringBuilder.AppendLine(string.Concat("Range".Translate() + ": ", compProperties_UnderBarrel.verbPropsUnderBarrel.range.ToString()));
			stringBuilder.AppendLine(string.Concat("CE_AmmoSet".Translate() + ": ", compProperties_UnderBarrel.propsUnderBarrel.ammoSet?.ToString()));
			stringBuilder.AppendLine(string.Concat("CE_MagazineSize".Translate() + ": ", compProperties_UnderBarrel.propsUnderBarrel.magazineSize.ToString()));
			return stringBuilder.ToString();
		}
		return base.GetExplanationUnfinalized(req, numberSense);
	}

	public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
	{
		return "CE_UBGLStats_Title".Translate();
	}
}
