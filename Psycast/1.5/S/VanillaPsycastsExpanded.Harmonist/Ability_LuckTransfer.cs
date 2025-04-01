using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class Ability_LuckTransfer : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		if (targets[0].Thing is Pawn pawn && targets[1].Thing is Pawn pawn2)
		{
			MoteBetween moteBetween = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
			moteBetween.Attach(pawn, pawn2);
			moteBetween.Scale = 1f;
			moteBetween.exactPosition = pawn.DrawPos;
			GenSpawn.Spawn(moteBetween, pawn.Position, pawn.MapHeld);
			moteBetween = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
			moteBetween.Attach(pawn2, pawn);
			moteBetween.Scale = 1f;
			moteBetween.exactPosition = pawn2.DrawPos;
			GenSpawn.Spawn(moteBetween, pawn2.Position, pawn2.MapHeld);
			int ticksToDisappear = Mathf.RoundToInt((float)((Ability)this).GetDurationForPawn() * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
			pawn.health.AddHediff(VPE_DefOf.VPE_Lucky, null, null).TryGetComp<HediffComp_Disappears>().ticksToDisappear = ticksToDisappear;
			pawn2.health.AddHediff(VPE_DefOf.VPE_UnLucky, null, null).TryGetComp<HediffComp_Disappears>().ticksToDisappear = ticksToDisappear;
		}
	}
}
