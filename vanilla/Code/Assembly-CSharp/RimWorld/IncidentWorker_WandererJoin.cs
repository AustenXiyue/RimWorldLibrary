using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_WandererJoin : IncidentWorker
{
	private const float RelationWithColonistWeight = 20f;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		return CanSpawnJoiner(map);
	}

	public virtual Pawn GeneratePawn()
	{
		Gender? fixedGender = null;
		if (def.pawnFixedGender != 0)
		{
			fixedGender = def.pawnFixedGender;
		}
		Ideo result = null;
		if (ModsConfig.IdeologyActive && !Find.IdeoManager.IdeosListForReading.Where((Ideo i) => !Faction.OfPlayer.ideos.Has(i)).TryRandomElementByWeight((Ideo x) => IdeoUtility.IdeoChangeToWeight(null, x), out result))
		{
			Find.IdeoManager.IdeosListForReading.Where((Ideo i) => !Faction.OfPlayer.ideos.IsPrimary(i)).TryRandomElementByWeight((Ideo x) => IdeoUtility.IdeoChangeToWeight(null, x), out result);
		}
		return PawnGenerator.GeneratePawn(new PawnGenerationRequest(def.pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, def.pawnMustBeCapableOfViolence, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, fixedGender, null, null, null, result, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null));
	}

	public virtual bool CanSpawnJoiner(Map map)
	{
		IntVec3 cell;
		return TryFindEntryCell(map, out cell);
	}

	public virtual void SpawnJoiner(Map map, Pawn pawn)
	{
		TryFindEntryCell(map, out var cell);
		GenSpawn.Spawn(pawn, cell, map);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!CanSpawnJoiner(map))
		{
			return false;
		}
		Pawn pawn = GeneratePawn();
		SpawnJoiner(map, pawn);
		if (def.pawnHediff != null)
		{
			pawn.health.AddHediff(def.pawnHediff, null, null);
		}
		TaggedString text = ((def.pawnHediff != null) ? def.letterText.Formatted(pawn.Named("PAWN"), NamedArgumentUtility.Named(def.pawnHediff, "HEDIFF")).AdjustedFor(pawn) : def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn));
		TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
		SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, pawn);
		return true;
	}

	private bool TryFindEntryCell(Map map, out IntVec3 cell)
	{
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
	}
}
