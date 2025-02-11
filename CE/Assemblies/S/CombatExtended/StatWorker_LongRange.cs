using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_LongRange : StatWorker
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return (((req.Def as ThingDef)?.GetProjectile()?.projectile as ProjectilePropertiesCE)?.shellingProps?.range).GetValueOrDefault();
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		return GetValueUnfinalized(req) > 0f && base.ShouldShowFor(req);
	}
}
