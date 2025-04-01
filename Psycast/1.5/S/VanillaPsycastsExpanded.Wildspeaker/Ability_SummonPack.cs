using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Wildspeaker;

public class Ability_SummonPack : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		Map map = targets[0].Map;
		float num = ((Ability)this).GetPowerForPawn();
		List<Pawn> list = new List<Pawn>();
		PawnKindDef animalKind;
		while (num > 0f && AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(num, map.Tile, out animalKind))
		{
			num -= animalKind.combatPower;
			Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null));
			list.Add(item);
		}
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal))
		{
			result = CellFinder.RandomEdgeCell(map);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Pawn pawn = list[i];
			GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(result, map, 10), map);
			pawn.mindState.mentalStateHandler.TryStartMentalState(VPE_DefOf.VPE_ManhunterTerritorial);
			pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(25000, 35000);
		}
		Find.LetterStack.ReceiveLetter("VPE.PackSummon".Translate(), "VPE.PackSummon.Desc".Translate(base.pawn.NameShortColored), LetterDefOf.PositiveEvent, new TargetInfo(result, map));
	}
}
