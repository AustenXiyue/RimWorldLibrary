using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(TooltipUtility), "ShotCalculationTipString")]
public class Harmony_TooltipUtility_ShotCalculationTipString_Patch
{
	public static void Postfix(ref string __result, Thing target)
	{
		Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
		if (!__result.NullOrEmpty() || singleSelectedThing == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Verb_LaunchProjectileCE verb_LaunchProjectileCE = null;
		if (singleSelectedThing is Pawn pawn && pawn != target && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectileCE)
		{
			verb_LaunchProjectileCE = pawn.equipment.PrimaryEq.PrimaryVerb as Verb_LaunchProjectileCE;
		}
		if (singleSelectedThing is Building_TurretGunCE building_TurretGunCE && building_TurretGunCE != target)
		{
			verb_LaunchProjectileCE = building_TurretGunCE.AttackVerb as Verb_LaunchProjectileCE;
		}
		if (verb_LaunchProjectileCE != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("ShotBy".Translate(singleSelectedThing.LabelShort, singleSelectedThing) + ":\n");
			if (verb_LaunchProjectileCE.CanHitTarget(target, out var report))
			{
				ShiftVecReport shiftVecReport = verb_LaunchProjectileCE.ShiftVecReportFor(new LocalTargetInfo(target));
				stringBuilder.Append(shiftVecReport.GetTextReadout());
			}
			else
			{
				stringBuilder.Append("CannotHit".Translate());
				if (!report.NullOrEmpty())
				{
					stringBuilder.Append(" " + report + ".");
				}
			}
			if (target is Pawn { Faction: null, InAggroMentalState: false } pawn2)
			{
				float num = ((!verb_LaunchProjectileCE.IsMeleeAttack) ? PawnUtility.GetManhunterOnDamageChance(pawn2, singleSelectedThing) : PawnUtility.GetManhunterOnDamageChance(pawn2, 0f, singleSelectedThing));
				if (num > 0f)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(string.Format("{0}: {1}", "ManhunterPerHit".Translate(), num.ToStringPercent()));
				}
			}
		}
		__result = stringBuilder.ToString();
	}
}
