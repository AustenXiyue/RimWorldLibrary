using RimWorld;
using Verse;

namespace CombatExtended;

[DefOf]
public static class CE_LetterDefOf
{
	public static LetterDef CE_ThreatBig;

	static CE_LetterDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_LetterDefOf));
	}
}
