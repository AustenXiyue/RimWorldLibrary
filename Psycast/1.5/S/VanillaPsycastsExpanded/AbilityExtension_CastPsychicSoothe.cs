using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_CastPsychicSoothe : AbilityExtension_AbilityMod
{
	public Gender gender;

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		List<GlobalTargetInfo> list = new List<GlobalTargetInfo>();
		foreach (Pawn item in ability.pawn.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (!item.Dead && item.gender == gender && item.needs != null && item.needs.mood != null)
			{
				ability.ApplyHediff(item);
			}
		}
	}
}
