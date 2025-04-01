using RimWorld;
using Verse;

namespace Quality_Recasting_Table;

[DefOf]
public static class RecastingDefOf
{
	public static JobDef DoRecasting;

	public static SoundDef TR_RecastCompleted;

	static RecastingDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RecastingDefOf));
	}
}
