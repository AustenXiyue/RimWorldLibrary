using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Milira;

public class GenStep_SunLightFuelStation_TechPrintRoom : GenStep_Scatterer
{
	private const int Size = 6;

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
		foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 6f, useCenter: true))
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
		ThingDef named = DefDatabase<ThingDef>.GetNamed("Techprint_Milira_SunLightFuelGenerator");
		if (named != null && !MiliraDefOf.Milira_SunLightFuelGenerator.IsFinished)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
			CellRect cellRect = CellRect.CenteredOn(loc, 5, 5).ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = cellRect;
			resolveParams.faction = faction;
			resolveParams.singleThingDef = named;
			resolveParams.wallStuff = MiliraDefOf.Milira_SunPlateSteel;
			resolveParams.floorDef = MiliraDefOf.SterileTile;
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("milian_SunLightFuelStation_TechPrintRoom", resolveParams);
			BaseGen.Generate();
			MapGenerator.SetVar("RectOfInterest", cellRect);
		}
	}
}
