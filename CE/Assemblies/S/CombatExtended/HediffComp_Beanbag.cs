using UnityEngine;
using Verse;

namespace CombatExtended;

public class HediffComp_Beanbag : HediffComp
{
	public HediffCompProperties_Beanbag Props => props as HediffCompProperties_Beanbag;

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		if (!parent.pawn.RaceProps.IsMechanoid)
		{
			float sevOffset = Props.BaseSeverityPerDamage * parent.Severity / Mathf.Pow(parent.pawn.HealthScale, 2f);
			HealthUtility.AdjustSeverity(parent.pawn, CE_HediffDefOf.MuscleSpasms, sevOffset);
		}
	}
}
