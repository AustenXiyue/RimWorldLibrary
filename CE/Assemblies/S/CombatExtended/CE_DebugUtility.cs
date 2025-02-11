using System.Collections.Generic;
using System.Linq;
using CombatExtended.WorldObjects;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class CE_DebugUtility
{
	[CE_DebugTooltip(CE_DebugTooltipType.Map, KeyCode.None)]
	public static string CellPositionTip(Map map, IntVec3 cell)
	{
		return $"Cell: ({cell.x}, {cell.z})";
	}

	[CE_DebugTooltip(CE_DebugTooltipType.World, KeyCode.None)]
	public static string TileIndexTip(World world, int tile)
	{
		return $"Tile index: {tile}";
	}

	[DebugOutput("CE", false, name = "Not patched WorldObjecDefs")]
	public static void NotPatchedWorldObjectDefs()
	{
		IEnumerable<WorldObjectDef> dataSources = DefDatabase<WorldObjectDef>.AllDefsListForReading.Where((WorldObjectDef x) => !x.comps.Any((WorldObjectCompProperties comp) => comp is WorldObjectCompProperties_Health) || !x.comps.Any((WorldObjectCompProperties comp) => comp is WorldObjectCompProperties_Hostility));
		DebugTables.MakeTablesDialog(dataSources, new TableDataGetter<WorldObjectDef>("ModName", (WorldObjectDef d) => d.modContentPack?.Name ?? ""), new TableDataGetter<WorldObjectDef>("def", (WorldObjectDef d) => d.defName ?? ""), new TableDataGetter<WorldObjectDef>("label", (WorldObjectDef d) => d.label ?? ""), new TableDataGetter<WorldObjectDef>("HostilityPatched", (WorldObjectDef d) => d.comps.Any((WorldObjectCompProperties comp) => comp is WorldObjectCompProperties_Hostility)), new TableDataGetter<WorldObjectDef>("HealthPatched", (WorldObjectDef d) => d.comps.Any((WorldObjectCompProperties comp) => comp is WorldObjectCompProperties_Health)));
	}

	[DebugAction("CE", null, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld)]
	public static void Heal()
	{
		int tileID = GenWorld.MouseTile();
		foreach (WorldObject item in Find.WorldObjects.ObjectsAt(tileID).ToList())
		{
			HealthComp component;
			if ((component = item.GetComponent<HealthComp>()) != null)
			{
				component.Health = 1f;
				component.recentShells.Clear();
			}
		}
	}
}
