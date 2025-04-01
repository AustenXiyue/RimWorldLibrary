using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_Corpsetalk : Ability_TargetCorpse
{
	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		Hediff_CorpseTalk hediff_CorpseTalk = ((Ability)this).ApplyHediff(((Ability)this).pawn) as Hediff_CorpseTalk;
		if (hediff_CorpseTalk.skillXPDifferences != null)
		{
			hediff_CorpseTalk.ResetSkills();
		}
		else
		{
			hediff_CorpseTalk.skillXPDifferences = new Dictionary<SkillDef, int>();
		}
		Corpse corpse = targets[0].Thing as Corpse;
		foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
		{
			SkillRecord skill = ((Ability)this).pawn.skills.GetSkill(allDef);
			SkillRecord skill2 = corpse.InnerPawn.skills.GetSkill(allDef);
			int num = skill2.Level - skill.Level;
			if (num > 0)
			{
				int level = skill.Level;
				skill.Level = Mathf.Min(20, skill.Level + num);
				hediff_CorpseTalk.skillXPDifferences[allDef] = skill.Level - level;
			}
		}
	}
}
