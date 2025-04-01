using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RealDeathless;

public class Gene_RealDeathless : Gene
{
	public int tickCounter = 0;

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		Map map = pawn.Corpse.Map;
		if (map != null && pawn.Faction.IsPlayer)
		{
			ResurrectionUtility.TryResurrect(pawn.Corpse.InnerPawn);
			pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.ResurrectionSickness, pawn), null, null);
			Find.StoryWatcher.statsRecord.colonistsKilled--;
		}
	}

	public override void Tick()
	{
		base.Tick();
		tickCounter++;
		if (tickCounter != 60000)
		{
			return;
		}
		if (pawn.health != null)
		{
			List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
			List<Hediff_Injury> list = new List<Hediff_Injury>();
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury item)
				{
					list.Add(item);
				}
			}
			if (missingPartsCommonAncestors.Count > 0)
			{
				for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
				{
					BodyPartRecord part = missingPartsCommonAncestors[j].Part;
					pawn.health.RestorePart(part);
				}
			}
			if (list.Count > 0)
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].Severity = list[k].Severity - 10f;
				}
			}
		}
		tickCounter = 0;
	}
}
