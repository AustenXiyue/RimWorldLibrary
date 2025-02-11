using System.Collections.Generic;
using Verse;
using VFESecurity;

namespace CombatExtended.Compatibility;

[StaticConstructorOnStartup]
public class CETrenches
{
	private static bool vfeInstalled;

	private const string VFES_ModName = "Vanilla Furniture Expanded - Security";

	static CETrenches()
	{
		vfeInstalled = ModLister.HasActiveModWithName("Vanilla Furniture Expanded - Security");
	}

	private static bool checkVFE(IntVec3 cell, Map map, out float heightAdjust)
	{
		heightAdjust = 0f;
		List<Thing> thingList = cell.GetThingList(map);
		using (List<Thing>.Enumerator enumerator = thingList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Thing current = enumerator.Current;
				CompProperties_TerrainSetter compProperties = current.def.GetCompProperties<CompProperties_TerrainSetter>();
				if (compProperties == null)
				{
					return false;
				}
				TerrainDef terrainDef = compProperties.terrainDef;
				TerrainDefExtension val = TerrainDefExtension.Get((Def)terrainDef);
				heightAdjust = (0f - val.coverEffectiveness) * 2f;
				return true;
			}
		}
		return false;
	}

	public static float GetHeightAdjust(IntVec3 cell, Map map)
	{
		if (map == null)
		{
			return 0f;
		}
		if (vfeInstalled)
		{
			float heightAdjust = 0f;
			if (checkVFE(cell, map, out heightAdjust))
			{
				return heightAdjust;
			}
		}
		return 0f;
	}
}
