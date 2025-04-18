using AncotLibrary;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Milira;

public static class MiliraFlyUtility_Hammer
{
	public static bool DoJump(Pawn pawn, LocalTargetInfo currentTarget, CompMeleeWeaponCharge_Ability comp, VerbProperties verbProps, Ability triggeringAbility = null, LocalTargetInfo target = default(LocalTargetInfo), ThingDef pawnFlyerOverride = null)
	{
		if (comp != null && !comp.CanBeUsed)
		{
			return false;
		}
		comp?.UsedOnce();
		IntVec3 position = pawn.Position;
		IntVec3 cell = currentTarget.Cell;
		Vector3 vector = (cell - position).ToVector3();
		vector.Normalize();
		Map map = pawn.Map;
		bool flag = Find.Selector.IsSelected(pawn);
		MiliraPawnFlyer_Hammer miliraPawnFlyer_Hammer = MiliraPawnFlyer_Hammer.MakeFlyer(pawnFlyerOverride ?? MiliraDefOf.Milira_PawnJumper_Hammer, pawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding, verbProps.flyWithCarriedThing, null, triggeringAbility, target);
		if (miliraPawnFlyer_Hammer != null)
		{
			FleckMaker.ThrowDustPuff(position.ToVector3Shifted() - vector, map, 2f);
			MiliraDefOf.Milira_SpearFire.Spawn(pawn.Position, currentTarget.Cell, map).Cleanup();
			GenSpawn.Spawn(miliraPawnFlyer_Hammer, cell, map);
			if (flag)
			{
				Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
			}
			return true;
		}
		return false;
	}

	public static void OrderJump(Pawn pawn, LocalTargetInfo target, Verb verb, float range)
	{
		Map map = pawn.Map;
		IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(target.Cell, pawn, (IntVec3 c) => ValidJumpTarget(map, c) && CanHitTargetFrom(pawn, pawn.Position, c, range));
		Job job = JobMaker.MakeJob(JobDefOf.CastJump, intVec);
		job.verbToUse = verb;
		if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
		{
			FleckMaker.Static(intVec, map, RimWorld.FleckDefOf.FeedbackGoto);
		}
	}

	public static bool CanHitTargetFrom(Pawn pawn, IntVec3 root, LocalTargetInfo targ, float range)
	{
		float num = range * range;
		IntVec3 cell = targ.Cell;
		if ((float)pawn.Position.DistanceToSquared(cell) <= num)
		{
			return GenSight.LineOfSight(root, cell, pawn.Map);
		}
		return false;
	}

	public static bool ValidJumpTarget(Map map, IntVec3 cell)
	{
		if (!cell.IsValid || !cell.InBounds(map))
		{
			return false;
		}
		if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && edifice is Building_Door { Open: false })
		{
			return false;
		}
		return true;
	}
}
