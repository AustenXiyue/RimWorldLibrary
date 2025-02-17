using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnGroupMaker
{
	public PawnGroupKindDef kindDef;

	public float commonality = 100f;

	public List<RaidStrategyDef> disallowedStrategies;

	public float maxTotalPoints = 9999999f;

	public List<PawnGenOption> options = new List<PawnGenOption>();

	public List<PawnGenOption> traders = new List<PawnGenOption>();

	public List<PawnGenOption> carriers = new List<PawnGenOption>();

	public List<PawnGenOption> guards = new List<PawnGenOption>();

	public float MinPointsToGenerateAnything(FactionDef faction, PawnGroupMakerParms parms = null)
	{
		return kindDef.Worker.MinPointsToGenerateAnything(this, faction, parms);
	}

	public IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, bool errorOnZeroResults = true)
	{
		return kindDef.Worker.GeneratePawns(parms, this, errorOnZeroResults);
	}

	public IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms)
	{
		return kindDef.Worker.GeneratePawnKindsExample(parms, this);
	}

	public bool CanGenerateFrom(PawnGroupMakerParms parms)
	{
		if (parms.points > maxTotalPoints)
		{
			return false;
		}
		if (disallowedStrategies != null && disallowedStrategies.Contains(parms.raidStrategy))
		{
			return false;
		}
		if (parms.raidStrategy != null && parms.raidStrategy.Worker is RaidStrategyWorker_WithRequiredPawnKinds raidStrategyWorker_WithRequiredPawnKinds && !raidStrategyWorker_WithRequiredPawnKinds.CanUseWithGroupMaker(this))
		{
			return false;
		}
		return kindDef.Worker.CanGenerateFrom(parms, this);
	}
}
