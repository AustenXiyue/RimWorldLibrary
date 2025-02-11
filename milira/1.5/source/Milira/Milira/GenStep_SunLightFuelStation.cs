using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Milira;

public class GenStep_SunLightFuelStation : GenStep_Scatterer
{
	private const int Size = 7;

	public override int SeedPart => 69356159;

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
		{
			return false;
		}
		if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
		{
			return false;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 10f, useCenter: true))
		{
			if (!item.InBounds(map) || item.GetEdifice(map) != null || item.Roofed(map))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		CellRect cellRect = CellRect.CenteredOn(loc, 10, 10).ClipInsideMap(map);
		ResolveParams resolveParams = default(ResolveParams);
		resolveParams.rect = cellRect;
		resolveParams.faction = faction;
		BaseGen.globalSettings.map = map;
		BaseGen.symbolStack.Push("milira_SunLightFuelStation_Integration", resolveParams);
		BaseGen.Generate();
		MapGenerator.SetVar("RectOfInterest", cellRect);
	}
}
