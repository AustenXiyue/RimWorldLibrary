using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Milira;

public class PawnsArrivalModeWorker_BattleAngelClusterDrop : PawnsArrivalModeWorker
{
	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
	}

	public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
	{
		IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map);
		TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!parms.spawnCenter.IsValid)
		{
			parms.spawnCenter = MiliraClusterUtility.FindClusterPosition(map, parms.mechClusterSketch, 100, 0.5f);
		}
		parms.spawnRotation = Rot4.Random;
		return true;
	}
}
