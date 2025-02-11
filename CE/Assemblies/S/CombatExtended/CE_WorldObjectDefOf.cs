using RimWorld;

namespace CombatExtended;

[DefOf]
public static class CE_WorldObjectDefOf
{
	public static WorldObjectDef TravelingShell;

	static CE_WorldObjectDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_WorldObjectDefOf));
	}
}
