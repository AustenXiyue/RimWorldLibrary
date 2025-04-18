using System.Collections.Generic;

namespace Verse;

public abstract class SectionLayer_Things : SectionLayer
{
	private CellRect bounds;

	protected bool requireAddToMapMesh;

	public SectionLayer_Things(Section section)
		: base(section)
	{
	}

	public override CellRect GetBoundaryRect()
	{
		return bounds;
	}

	public override void DrawLayer()
	{
		if (DebugViewSettings.drawThingsPrinted)
		{
			base.DrawLayer();
		}
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		bounds = section.CellRect;
		foreach (IntVec3 item in section.CellRect)
		{
			List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if ((thing.def.seeThroughFog || !base.Map.fogGrid.IsFogged(thing.Position)) && thing.def.drawerType != 0 && (thing.def.drawerType != DrawerType.RealtimeOnly || !requireAddToMapMesh) && (!(thing.def.hideAtSnowDepth < 1f) || !(base.Map.snowGrid.GetDepth(thing.Position) > thing.def.hideAtSnowDepth)) && thing.Position.x == item.x && thing.Position.z == item.z)
				{
					TakePrintFrom(thing);
					bounds.Encapsulate(thing.OccupiedDrawRect());
				}
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	protected abstract void TakePrintFrom(Thing t);
}
