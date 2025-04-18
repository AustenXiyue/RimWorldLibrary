using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Hospital : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Building_Bed building_Bed && building_Bed.def.building.bed_humanlike)
			{
				if (building_Bed.ForPrisoners)
				{
					return 0f;
				}
				if (building_Bed.Medical)
				{
					num++;
				}
			}
		}
		return (float)num * 100000f;
	}
}
