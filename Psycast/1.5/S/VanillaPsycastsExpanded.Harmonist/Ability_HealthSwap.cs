using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class Ability_HealthSwap : Ability
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		if (!(targets[0].Thing is Pawn pawn) || !(targets[1].Thing is Pawn pawn2))
		{
			return;
		}
		MoteBetween moteBetween = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
		moteBetween.Attach(pawn, pawn2);
		moteBetween.Scale = 1f;
		moteBetween.exactPosition = pawn.DrawPos;
		GenSpawn.Spawn(moteBetween, pawn.Position, pawn.MapHeld);
		List<Hediff> list = pawn.health.hediffSet.hediffs.Where(ShouldTransfer).ToList();
		List<Hediff> list2 = pawn2.health.hediffSet.hediffs.Where(ShouldTransfer).ToList();
		foreach (Hediff item in list)
		{
			pawn.health.RemoveHediff(item);
		}
		foreach (Hediff item2 in list2)
		{
			pawn2.health.RemoveHediff(item2);
		}
		AddAll(pawn, list2);
		AddAll(pawn2, list);
	}

	private static bool ShouldTransfer(Hediff hediff)
	{
		bool flag = ((hediff is Hediff_Injury || hediff is Hediff_MissingPart || hediff is Hediff_Addiction) ? true : false);
		return flag || hediff.def.tendable || hediff.def.makesSickThought || hediff.def.HasComp(typeof(HediffComp_Immunizable));
	}

	private static void AddAll(Pawn pawn, List<Hediff> hediffs)
	{
		TryAdd();
		TryAdd();
		void TryAdd()
		{
			hediffs.RemoveAll(delegate(Hediff hediff)
			{
				if (!pawn.health.hediffSet.PartIsMissing(hediff.Part))
				{
					try
					{
						pawn.health.AddHediff(hediff, hediff.Part, null);
						return true;
					}
					catch (Exception arg)
					{
						Log.Error($"Error while swapping: {arg}");
						return false;
					}
				}
				return false;
			});
		}
	}
}
