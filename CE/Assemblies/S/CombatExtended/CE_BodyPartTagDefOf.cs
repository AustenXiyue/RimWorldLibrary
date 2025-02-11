using RimWorld;
using Verse;

namespace CombatExtended;

[DefOf]
public class CE_BodyPartTagDefOf
{
	public static BodyPartTagDef OutsideSquishy;

	static CE_BodyPartTagDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_BodyPartTagDefOf));
	}
}
