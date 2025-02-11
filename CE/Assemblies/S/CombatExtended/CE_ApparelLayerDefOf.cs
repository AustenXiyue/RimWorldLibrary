using RimWorld;
using Verse;

namespace CombatExtended;

[DefOf]
public static class CE_ApparelLayerDefOf
{
	public static ApparelLayerDef Webbing;

	public static ApparelLayerDef Backpack;

	public static ApparelLayerDef Shield;

	public static ApparelLayerDef OnHead;

	public static ApparelLayerDef StrappedHead;

	static CE_ApparelLayerDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_ApparelLayerDefOf));
	}
}
