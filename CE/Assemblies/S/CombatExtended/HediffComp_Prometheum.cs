using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class HediffComp_Prometheum : HediffComp
{
	private const float InternalFireDamage = 2f;

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (!base.Pawn.IsHashIntervalTick(60))
		{
			return;
		}
		if (base.Pawn.Position.GetThingList(base.Pawn.Map).Any((Thing x) => x.def == ThingDefOf.Filth_FireFoam))
		{
			severityAdjustment = -1000f;
			return;
		}
		Fire fire = base.Pawn.GetAttachment(ThingDefOf.Fire) as Fire;
		if (fire == null && base.Pawn.Spawned)
		{
			base.Pawn.TryAttachFire(parent.Severity * 0.5f, null);
		}
		else if (fire != null)
		{
			fire.fireSize = Mathf.Min(fire.fireSize + parent.Severity * 0.5f, 1.75f);
		}
		if (base.Pawn.def.race.IsMechanoid)
		{
			BodyPartRecord bodyPartRecord = base.Pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Inside).RandomElement();
			if (bodyPartRecord != null)
			{
				base.Pawn.TakeDamage(new DamageInfo(CE_DamageDefOf.Flame_Secondary, 2f * base.Pawn.BodySize * parent.Severity, 0f, -1f, null, bodyPartRecord));
			}
		}
	}
}
