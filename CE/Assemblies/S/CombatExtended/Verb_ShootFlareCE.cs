using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Verb_ShootFlareCE : Verb_ShootMortarCE
{
	private const float MISSRADIUS_FACTOR = 0.4f;

	private const float DISTFACTOR_FACTOR = 0.9f;

	public override ShiftVecReport ShiftVecReportFor(LocalTargetInfo target)
	{
		ShiftVecReport shiftVecReport = base.ShiftVecReportFor(target);
		shiftVecReport.shotDist = Vector3.Distance(target.CenterVector3, caster.TrueCenter()) * 0.9f;
		return shiftVecReport;
	}

	protected override float GetMissRadiusForDist(float targDist)
	{
		return base.GetMissRadiusForDist(targDist) * 0.4f;
	}
}
