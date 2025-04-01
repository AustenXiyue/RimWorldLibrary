using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_GoodwillImpact : Ability
{
	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (target.Thing is Pawn pawn && (pawn.HostileTo(base.pawn) || pawn.Faction == base.pawn.Faction || pawn.Faction == null))
		{
			if (showMessages)
			{
				Messages.Message("VPE.MustBeAllyOrNeutral".Translate(), pawn, MessageTypeDefOf.CautionInput);
			}
			return false;
		}
		return ((Ability)this).ValidateTarget(target, showMessages);
	}

	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		foreach (GlobalTargetInfo globalTargetInfo in targets)
		{
			Pawn pawn = globalTargetInfo.Thing as Pawn;
			int goodwillChange = (int)Mathf.Max(10f, base.pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 100f - 100f);
			pawn.Faction.TryAffectGoodwillWith(base.pawn.Faction, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, null, null);
		}
	}
}
