using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AncotLibrary;

public class AncotPawnGenUtility
{
	public static List<Thing> GeneratePawnsWithCommonality(GenStepParams parms, Faction faction, Map map, float fixedPoint, List<PawnkindWithCommonality> pawnkindsWithCommonality)
	{
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
		pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
		pawnGroupMakerParms.tile = map.Tile;
		pawnGroupMakerParms.faction = faction;
		pawnGroupMakerParms.points = parms.sitePart.parms.threatPoints;
		if (fixedPoint != 0f)
		{
			pawnGroupMakerParms.points = fixedPoint;
		}
		List<Thing> list = new List<Thing>();
		float pointsLeft;
		PawnkindWithCommonality result;
		for (pointsLeft = pawnGroupMakerParms.points; pointsLeft > 0f && pawnkindsWithCommonality.Where((PawnkindWithCommonality p) => p.pawnkindDef.combatPower <= pointsLeft).TryRandomElementByWeight((PawnkindWithCommonality p) => p.commonality, out result); pointsLeft -= result.pawnkindDef.combatPower)
		{
			list.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(result.pawnkindDef, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null)));
		}
		return list;
	}

	public static List<Thing> GenerateRequiredPawns(Faction faction, List<PawnkindWithAmount> pawnkindsRequired)
	{
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < pawnkindsRequired.Count; i++)
		{
			PawnkindWithAmount pawnkindWithAmount = pawnkindsRequired[i];
			for (int j = 0; (float)j < pawnkindWithAmount.amount; j++)
			{
				list.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkindWithAmount.pawnkindDef, faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null)));
			}
		}
		return list;
	}
}
