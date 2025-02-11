using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Milira;

public class GenStep_SunBlastFurnace : GenStep_Scatterer
{
	private const int Size = 7;

	public override int SeedPart => 69356128;

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
		foreach (IntVec3 item in CellRect.CenteredOn(c, 7, 7))
		{
			if (!item.InBounds(map) || item.GetEdifice(map) != null)
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		CellRect cellRect = CellRect.CenteredOn(loc, 7, 7).ClipInsideMap(map);
		ResolveParams resolveParams = default(ResolveParams);
		resolveParams.rect = cellRect;
		resolveParams.faction = faction;
		resolveParams.singleThingDef = MiliraDefOf.Milira_SunBlastFurnace;
		resolveParams.wallStuff = MiliraDefOf.Milira_SunPlateSteel;
		resolveParams.floorDef = MiliraDefOf.SterileTile;
		BaseGen.globalSettings.map = map;
		BaseGen.symbolStack.Push("milira_SunBlastFurnaceCell", resolveParams);
		BaseGen.Generate();
		MapGenerator.SetVar("RectOfInterest", cellRect);
	}
}
