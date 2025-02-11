using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Milira;

[HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
public static class JobDriver_Wait_CheckForAutoAttack_Patch
{
	private static bool Prefix(JobDriver_Wait __instance)
	{
		Pawn pawn = __instance.pawn;
		if (MilianUtility.IsMilian(pawn))
		{
			if (!pawn.kindDef.canMeleeAttack || pawn.Downed || pawn.stances.FullBodyBusy || pawn.IsCarryingPawn() || pawn.IsShambler)
			{
				return true;
			}
			bool flag = !pawn.WorkTagIsDisabled(WorkTags.Violent);
			bool flag2 = pawn.RaceProps.ToolUser && pawn.Faction == Faction.OfPlayer && !pawn.WorkTagIsDisabled(WorkTags.Firefighting);
			Fire fire = null;
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
				if (!c.InBounds(pawn.Map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(pawn.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (flag && pawn.kindDef.canMeleeAttack && thingList[j] is Pawn pawn2 && !pawn2.ThreatDisabled(pawn) && pawn.HostileTo(pawn2))
					{
						CompActivity comp = pawn2.GetComp<CompActivity>();
						if ((comp == null || comp.IsActive) && !pawn.ThreatDisabledBecauseNonAggressiveRoamer(pawn2) && GenHostility.IsActiveThreatTo(pawn2, pawn.Faction))
						{
							pawn.meleeVerbs.TryMeleeAttack(pawn2);
							__instance.collideWithPawns = true;
							return false;
						}
					}
					if (flag2 && thingList[j] is Fire fire2 && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != pawn))
					{
						fire = fire2;
					}
				}
			}
			if (fire != null && (!pawn.InMentalState || pawn.MentalState.def.allowBeatfire))
			{
				pawn.natives.TryBeatFire(fire);
			}
			else
			{
				if (!flag || !__instance.job.canUseRangedWeapon || __instance.job.def != JobDefOf.Wait_Combat || (pawn.Drafted && !pawn.drafter.FireAtWill))
				{
					return false;
				}
				Verb currentEffectiveVerb = pawn.CurrentEffectiveVerb;
				if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
				{
					TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
					if (currentEffectiveVerb.IsIncendiary_Ranged())
					{
						targetScanFlags |= TargetScanFlags.NeedNonBurning;
					}
					Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, targetScanFlags);
					if (thing != null)
					{
						pawn.TryStartAttack(thing);
						__instance.collideWithPawns = true;
					}
				}
			}
			return false;
		}
		return true;
	}
}
