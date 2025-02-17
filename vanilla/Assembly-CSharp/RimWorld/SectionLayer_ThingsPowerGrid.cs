using Verse;

namespace RimWorld;

public class SectionLayer_ThingsPowerGrid : SectionLayer_Things
{
	public SectionLayer_ThingsPowerGrid(Section section)
		: base(section)
	{
		requireAddToMapMesh = false;
		relevantChangeTypes = MapMeshFlagDefOf.PowerGrid;
	}

	public override void DrawLayer()
	{
		if (OverlayDrawHandler.ShouldDrawPowerGrid)
		{
			base.DrawLayer();
		}
	}

	protected override void TakePrintFrom(Thing t)
	{
		if ((t.Faction == null || t.Faction == Faction.OfPlayer) && t is Building building)
		{
			building.PrintForPowerGrid(this);
		}
	}
}
