using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_BodyPartSharpArmor : StatWorker_BodyPartDensity
{
	public override string UnitString => "CE_mmRHA".Translate();

	public override float GetBaseValueFor(StatRequest req)
	{
		Pawn pawn = (Pawn)req.Thing;
		return 20f * (pawn.RaceProps.IsFlesh ? 0.011f : 0.2f);
	}
}
