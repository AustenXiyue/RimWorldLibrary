using System;
using RimWorld;

namespace Verse;

public class SectionLayer_ThingsGeneral : SectionLayer_Things
{
	public SectionLayer_ThingsGeneral(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Things;
		requireAddToMapMesh = true;
	}

	protected override void TakePrintFrom(Thing t)
	{
		try
		{
			t.Print(this);
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat("Exception printing ", t, " at ", t.Position, ": ", ex));
		}
	}
}
