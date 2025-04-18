using Verse;

namespace RimWorld;

[DefOf]
public static class PawnCapacityDefOf
{
	public static PawnCapacityDef Consciousness;

	public static PawnCapacityDef Sight;

	public static PawnCapacityDef Hearing;

	public static PawnCapacityDef Moving;

	public static PawnCapacityDef Manipulation;

	public static PawnCapacityDef Talking;

	public static PawnCapacityDef Breathing;

	public static PawnCapacityDef BloodFiltration;

	public static PawnCapacityDef BloodPumping;

	static PawnCapacityDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PawnCapacityDefOf));
	}
}
