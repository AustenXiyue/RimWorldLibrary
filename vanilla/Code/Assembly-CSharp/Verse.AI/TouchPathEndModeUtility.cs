using System;
using System.Collections.Generic;

namespace Verse.AI;

public static class TouchPathEndModeUtility
{
	[Obsolete]
	public static bool IsCornerTouchAllowed(int cornerX, int cornerZ, int adjCardinal1X, int adjCardinal1Z, int adjCardinal2X, int adjCardinal2Z, PathingContext pc)
	{
		return IsCornerTouchAllowed_NewTemp(null, cornerX, cornerZ, adjCardinal1X, adjCardinal1Z, adjCardinal2X, adjCardinal2Z, pc);
	}

	public static bool IsCornerTouchAllowed_NewTemp(LocalTargetInfo? target, int cornerX, int cornerZ, int adjCardinal1X, int adjCardinal1Z, int adjCardinal2X, int adjCardinal2Z, PathingContext pc)
	{
		Building building = pc.map.edificeGrid[new IntVec3(cornerX, 0, cornerZ)];
		if (building != null && target?.Pawn == null && MakesOccupiedCellsAlwaysReachableDiagonally(building.def))
		{
			return true;
		}
		IntVec3 intVec = new IntVec3(adjCardinal1X, 0, adjCardinal1Z);
		IntVec3 intVec2 = new IntVec3(adjCardinal2X, 0, adjCardinal2Z);
		if ((pc.pathGrid.Walkable(intVec) && intVec.GetDoor(pc.map) == null) || (pc.pathGrid.Walkable(intVec2) && intVec2.GetDoor(pc.map) == null))
		{
			return true;
		}
		return false;
	}

	public static bool MakesOccupiedCellsAlwaysReachableDiagonally(ThingDef def)
	{
		ThingDef thingDef = (def.IsFrame ? (def.entityDefToBuild as ThingDef) : def);
		if (thingDef != null && thingDef.CanInteractThroughCorners)
		{
			return true;
		}
		return false;
	}

	[Obsolete]
	public static bool IsAdjacentCornerAndNotAllowed(IntVec3 cell, IntVec3 BL, IntVec3 TL, IntVec3 TR, IntVec3 BR, PathingContext pc)
	{
		return IsAdjacentCornerAndNotAllowed_NewTemp(cell, null, BL, TL, TR, BR, pc);
	}

	public static bool IsAdjacentCornerAndNotAllowed_NewTemp(IntVec3 cell, LocalTargetInfo? target, IntVec3 BL, IntVec3 TL, IntVec3 TR, IntVec3 BR, PathingContext pc)
	{
		if (cell == BL && !IsCornerTouchAllowed_NewTemp(target, BL.x + 1, BL.z + 1, BL.x + 1, BL.z, BL.x, BL.z + 1, pc))
		{
			return true;
		}
		if (cell == TL && !IsCornerTouchAllowed_NewTemp(target, TL.x + 1, TL.z - 1, TL.x + 1, TL.z, TL.x, TL.z - 1, pc))
		{
			return true;
		}
		if (cell == TR && !IsCornerTouchAllowed_NewTemp(target, TR.x - 1, TR.z - 1, TR.x - 1, TR.z, TR.x, TR.z - 1, pc))
		{
			return true;
		}
		if (cell == BR && !IsCornerTouchAllowed_NewTemp(target, BR.x - 1, BR.z + 1, BR.x - 1, BR.z, BR.x, BR.z + 1, pc))
		{
			return true;
		}
		return false;
	}

	public static void AddAllowedAdjacentRegions(LocalTargetInfo dest, TraverseParms traverseParams, Map map, List<Region> regions)
	{
		GenAdj.GetAdjacentCorners(dest, out var BL, out var TL, out var TR, out var BR);
		PathingContext pc = map.pathing.For(traverseParams);
		if (!dest.HasThing || (dest.Thing.def.size.x == 1 && dest.Thing.def.size.z == 1))
		{
			IntVec3 cell = dest.Cell;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = GenAdj.AdjacentCells[i] + cell;
				if (intVec.InBounds(map) && !IsAdjacentCornerAndNotAllowed_NewTemp(intVec, dest, BL, TL, TR, BR, pc))
				{
					Region region = intVec.GetRegion(map);
					if (region != null && region.Allows(traverseParams, isDestination: true))
					{
						regions.Add(region);
					}
				}
			}
			return;
		}
		List<IntVec3> list = GenAdjFast.AdjacentCells8Way(dest);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].InBounds(map) && !IsAdjacentCornerAndNotAllowed_NewTemp(list[j], dest, BL, TL, TR, BR, pc))
			{
				Region region2 = list[j].GetRegion(map);
				if (region2 != null && region2.Allows(traverseParams, isDestination: true))
				{
					regions.Add(region2);
				}
			}
		}
	}

	public static bool IsAdjacentOrInsideAndAllowedToTouch(IntVec3 root, LocalTargetInfo target, PathingContext pc)
	{
		GenAdj.GetAdjacentCorners(target, out var BL, out var TL, out var TR, out var BR);
		if (root.AdjacentTo8WayOrInside(target))
		{
			return !IsAdjacentCornerAndNotAllowed_NewTemp(root, target, BL, TL, TR, BR, pc);
		}
		return false;
	}
}
