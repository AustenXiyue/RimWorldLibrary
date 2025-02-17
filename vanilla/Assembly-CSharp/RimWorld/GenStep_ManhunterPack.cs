using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_ManhunterPack : GenStep
{
	public FloatRange defaultPointsRange = new FloatRange(300f, 500f);

	private int MinRoomCells = 225;

	public override int SeedPart => 457293335;

	public override void Generate(Map map, GenStepParams parms)
	{
		TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true);
		if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams) && x.GetRoom(map).CellCount >= MinRoomCells, map, out var result))
		{
			float points = ((parms.sitePart != null) ? parms.sitePart.parms.threatPoints : defaultPointsRange.RandomInRange);
			PawnKindDef animalKind;
			if (parms.sitePart != null && parms.sitePart.parms.animalKind != null)
			{
				animalKind = parms.sitePart.parms.animalKind;
			}
			else if (!ManhunterPackGenStepUtility.TryGetAnimalsKind(points, map.Tile, out animalKind))
			{
				return;
			}
			List<Pawn> list = AggressiveAnimalIncidentUtility.GenerateAnimals(animalKind, map.Tile, points);
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(result, map, 10);
				GenSpawn.Spawn(list[i], loc, map, Rot4.Random);
				list[i].health.AddHediff(HediffDefOf.Scaria, null, null);
				list[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
			}
		}
	}
}
