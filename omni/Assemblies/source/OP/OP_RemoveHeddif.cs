using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OP;

public class OP_RemoveHeddif : IngestionOutcomeDoer
{
	public List<HediffDef> hediffDefs;

	protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
	{
		foreach (HediffDef hediffDef in hediffDefs)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}
}
