using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class GenWorld
{
	private static int cachedTile_noSnap = -1;

	private static int cachedFrame_noSnap = -1;

	private static int cachedTile_snap = -1;

	private static int cachedFrame_snap = -1;

	public const float MaxRayLength = 1500f;

	private static List<WorldObject> tmpWorldObjectsUnderMouse = new List<WorldObject>();

	public static int MouseTile(bool snapToExpandableWorldObjects = false)
	{
		if (snapToExpandableWorldObjects)
		{
			if (cachedFrame_snap == Time.frameCount)
			{
				return cachedTile_snap;
			}
			cachedTile_snap = TileAt(UI.MousePositionOnUI, snapToExpandableWorldObjects: true);
			cachedFrame_snap = Time.frameCount;
			return cachedTile_snap;
		}
		if (cachedFrame_noSnap == Time.frameCount)
		{
			return cachedTile_noSnap;
		}
		cachedTile_noSnap = TileAt(UI.MousePositionOnUI);
		cachedFrame_noSnap = Time.frameCount;
		return cachedTile_noSnap;
	}

	public static int TileAt(Vector2 clickPos, bool snapToExpandableWorldObjects = false)
	{
		Camera worldCamera = Find.WorldCamera;
		if (!worldCamera.gameObject.activeInHierarchy)
		{
			return -1;
		}
		if (snapToExpandableWorldObjects)
		{
			ExpandableWorldObjectsUtility.GetExpandedWorldObjectUnderMouse(UI.MousePositionOnUI, tmpWorldObjectsUnderMouse);
			if (tmpWorldObjectsUnderMouse.Any())
			{
				int tile = tmpWorldObjectsUnderMouse[0].Tile;
				tmpWorldObjectsUnderMouse.Clear();
				return tile;
			}
		}
		Ray ray = worldCamera.ScreenPointToRay(clickPos * Prefs.UIScale);
		int worldLayerMask = WorldCameraManager.WorldLayerMask;
		if (Physics.Raycast(ray, out var hitInfo, 1500f, worldLayerMask))
		{
			return Find.World.renderer.GetTileIDFromRayHit(hitInfo);
		}
		return -1;
	}

	public static string GetPollutionDescription(float pollution)
	{
		if (pollution <= 0f)
		{
			return "TilePollutionNone".Translate();
		}
		if (pollution <= 0.333f)
		{
			return "TilePollutionLight".Translate();
		}
		if (pollution <= 0.666f)
		{
			return "TilePollutionModerate".Translate();
		}
		return "TilePollutionExtreme".Translate();
	}
}
