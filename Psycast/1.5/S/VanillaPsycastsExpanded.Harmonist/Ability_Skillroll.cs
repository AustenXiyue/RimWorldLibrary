using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class Ability_Skillroll : Ability
{
	private static readonly Func<Pawn, SkillDef, PawnGenerationRequest, int> finalLevelOfSkill = AccessTools.Method(typeof(PawnGenerator), "FinalLevelOfSkill", (Type[])null, (Type[])null).CreateDelegate<Func<Pawn, SkillDef, PawnGenerationRequest, int>>();

	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		Pawn pawn = targets[0].Thing as Pawn;
		int num = 0;
		PawnKindDef kindDef = pawn.kindDef;
		DevelopmentalStage developmentalStage = pawn.DevelopmentalStage;
		PawnGenerationRequest arg = new PawnGenerationRequest(kindDef, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, developmentalStage, null, null, null);
		foreach (SkillRecord skill in pawn.skills.skills)
		{
			int levelInt = skill.levelInt;
			skill.levelInt = finalLevelOfSkill(pawn, skill.def, arg);
			num += levelInt - skill.levelInt;
		}
		num = Mathf.RoundToInt((float)num * 1.1f);
		for (int i = 0; i < num; i++)
		{
			SkillRecord skillRecord = pawn.skills.skills.Where((SkillRecord skill) => !skill.TotallyDisabled && skill.levelInt < 20).RandomElement();
			skillRecord.levelInt++;
		}
	}

	public override bool CanHitTarget(LocalTargetInfo target)
	{
		int result;
		if (((Ability)this).CanHitTarget(target))
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				Faction faction = pawn.Faction;
				if (faction != null)
				{
					pawn = base.pawn;
					if (pawn != null)
					{
						Faction faction2 = pawn.Faction;
						if (faction2 != null)
						{
							result = ((faction2 == faction || faction2.RelationKindWith(faction) == FactionRelationKind.Ally) ? 1 : 0);
							goto IL_0046;
						}
					}
				}
			}
		}
		result = 0;
		goto IL_0046;
		IL_0046:
		return (byte)result != 0;
	}
}
