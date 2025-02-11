using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_MeleeStats : StatWorker
{
	public override bool IsDisabledFor(Thing thing)
	{
		return thing?.def?.building?.IsTurret ?? base.IsDisabledFor(thing);
	}
}
