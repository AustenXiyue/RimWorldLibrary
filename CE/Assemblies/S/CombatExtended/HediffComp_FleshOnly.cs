using Verse;

namespace CombatExtended;

public class HediffComp_FleshOnly : HediffComp
{
	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		if (!base.Pawn.RaceProps.IsFlesh)
		{
			parent.Severity = 0f;
		}
	}
}
