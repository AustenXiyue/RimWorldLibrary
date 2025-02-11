using System.Collections.Generic;
using System.Linq;
using CombatExtended.RocketGUI;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended.WorldObjects;

public static class WorldHealthGUIUtility
{
	private static List<WorldObject> visibleObjects = new List<WorldObject>(1000);

	public static void OnGUIWorldObjectHealth()
	{
		WorldObjectTrackerCE component = Find.World.GetComponent<WorldObjectTrackerCE>();
		visibleObjects.Clear();
		visibleObjects.AddRange(component.TrackedObjects.Where((WorldObject w) => Visible(w)));
		for (int i = 0; i < visibleObjects.Count; i++)
		{
			DrawHealthBar(visibleObjects[i]);
		}
	}

	private static bool Visible(WorldObject worldObject)
	{
		return !worldObject.HiddenBehindTerrainNow() && (worldObject.GetComponent<HealthComp>()?.Health ?? 1f) < 1f;
	}

	private static void DrawHealthBar(WorldObject worldObject)
	{
		if (worldObject is MapParent { HasMap: not false, Map: not null } mapParent && Find.Maps.Contains(mapParent.Map))
		{
			return;
		}
		HealthComp comp = worldObject.GetComponent<HealthComp>();
		if (comp != null)
		{
			Vector2 vector = worldObject.ScreenPos();
			Rect rect = new Rect(new Vector2(vector.x - 15f, vector.y + 18f), new Vector2(30f, 5f));
			Color red = Color.red;
			global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Tiny;
				Widgets.DrawBoxSolid(rect, Color.black);
				Widgets.DrawBoxSolid(rect.ContractedBy(1f).LeftPart(comp.Health), GetHealthBarColor(comp.Health));
			});
		}
	}

	private static Color GetHealthBarColor(float health)
	{
		if (health > 0.5f)
		{
			return Color.green;
		}
		if (health > 0.2f)
		{
			return Color.yellow;
		}
		return Color.red;
	}
}
