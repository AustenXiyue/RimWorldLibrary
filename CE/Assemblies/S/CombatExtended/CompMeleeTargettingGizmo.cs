using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class CompMeleeTargettingGizmo : ThingComp
{
	private static List<BodyPartDef> priorityList;

	public BodyPartHeight heightInt = BodyPartHeight.Undefined;

	public BodyPartDef targetBodyPart = BodyPartDefOf.Torso;

	public Pawn PawnParent => (Pawn)parent;

	public Thing primaryWeapon => PawnParent?.equipment.Primary;

	public bool SkillReqA => PawnParent.skills.GetSkill(SkillDefOf.Melee).Level >= 8;

	public bool SkillReqBP => PawnParent.skills.GetSkill(SkillDefOf.Melee).Level > 15;

	private string heightString
	{
		get
		{
			string text = heightInt.ToString();
			if (text.ToLower() == "undefined")
			{
				text = "Automatic";
			}
			return text;
		}
	}

	static CompMeleeTargettingGizmo()
	{
		priorityList = null;
		priorityList = new List<BodyPartDef>
		{
			CE_BodyPartDefOf.Neck,
			BodyPartDefOf.Eye,
			BodyPartDefOf.Head
		};
	}

	public BodyPartHeight finalHeight(Pawn target)
	{
		if (PawnParent.Faction == Faction.OfPlayer && heightInt != 0)
		{
			return heightInt;
		}
		float num = 1f;
		if (primaryWeapon != null)
		{
			num = primaryWeapon.def.tools.Max((Tool x) => (!(x is ToolCE toolCE)) ? 0f : toolCE.armorPenetrationSharp) * primaryWeapon.GetStatValue(CE_StatDefOf.MeleePenetrationFactor);
		}
		if (PawnParent.skills.GetSkill(SkillDefOf.Melee).Level < 16 && PawnParent.skills.GetSkill(SkillDefOf.Melee).Level > 7)
		{
			BodyPartRecord torso = (from x in target.health.hediffSet.GetNotMissingParts()
				where x.def == BodyPartDefOf.Torso
				select x).FirstOrFallback();
			if (torso != null)
			{
				List<Apparel> list = target.apparel?.WornApparel?.FindAll((Apparel x) => x.def.apparel.CoversBodyPart(torso));
				float num2 = target.GetStatValue(StatDefOf.ArmorRating_Sharp);
				if (!list.NullOrEmpty())
				{
					foreach (Apparel item in list)
					{
						if (item != null)
						{
							num2 += item.GetStatValue(StatDefOf.ArmorRating_Sharp);
						}
					}
				}
				if (num < num2)
				{
					return BodyPartHeight.Top;
				}
			}
			return BodyPartHeight.Middle;
		}
		if (PawnParent.skills.GetSkill(SkillDefOf.Melee).Level >= 16)
		{
			foreach (BodyPartDef bpd in priorityList)
			{
				targetBodyPart = bpd;
				BodyPartRecord bp = (from y in target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Top)
					where y.def == bpd
					select y).FirstOrFallback();
				if (bp != null)
				{
					Apparel apparel = target.apparel?.WornApparel?.Find((Apparel x) => x.def.apparel.CoversBodyPart(bp));
					if (apparel != null && num < apparel.GetStatValue(StatDefOf.ArmorRating_Sharp))
					{
						targetBodyPart = null;
						return BodyPartHeight.Bottom;
					}
					targetBodyPart = bp.def;
				}
			}
			return BodyPartHeight.Top;
		}
		return BodyPartHeight.Undefined;
	}

	public float SkillBodyPartAttackChance(BodyPartRecord PartToHit)
	{
		if (PartToHit == null)
		{
			return 0f;
		}
		float coverage = PartToHit.coverage;
		return coverage * (((float)PawnParent.skills.GetSkill(SkillDefOf.Melee).Level - 15f) * PawnParent.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (parent is Corpse || !PawnParent.IsColonist || PawnParent.InAggroMentalState)
		{
			yield break;
		}
		if (SkillReqA)
		{
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Buttons/TargettingMelee/" + heightInt),
				action = ChangeCurrentHeight,
				defaultLabel = "CE_MeleeTargetting_CurHeight".Translate() + " " + heightString
			};
		}
		if (!SkillReqBP || heightInt == BodyPartHeight.Undefined)
		{
			yield break;
		}
		yield return new Command_Action
		{
			icon = ContentFinder<Texture2D>.Get("UI/Buttons/TargettingMelee/Undefined"),
			defaultLabel = "CE_MeleeTargetting_CurPart".Translate() + " " + (targetBodyPart?.label ?? ((string)"CE_NoBP".Translate())),
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				List<BodyPartDef> list2 = new List<BodyPartDef>();
				foreach (BodyPartRecord allPart in BodyDefOf.Human.AllParts)
				{
					if (!list2.Contains(allPart.def) && allPart.depth == BodyPartDepth.Outside && allPart.height == heightInt && !allPart.def.label.Contains("toe") && !allPart.def.label.Contains("finger") && !allPart.def.label.Contains("utility"))
					{
						list2.Add(allPart.def);
					}
				}
				foreach (BodyPartDef def in list2)
				{
					list.Add(new FloatMenuOption(def.label, delegate
					{
						ChangeCurrentPart(def);
					}));
				}
				list.Add(new FloatMenuOption("CE_NoBP".Translate(), delegate
				{
					ChangeCurrentPart(null);
				}));
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public override void PostExposeData()
	{
		Scribe_Defs.Look(ref targetBodyPart, "TargetBodyPart");
		Scribe_Values.Look(ref heightInt, "heightInt", BodyPartHeight.Undefined);
	}

	[Multiplayer.SyncMethod]
	private void ChangeCurrentHeight()
	{
		switch (heightInt)
		{
		case BodyPartHeight.Bottom:
			heightInt = BodyPartHeight.Middle;
			targetBodyPart = null;
			break;
		case BodyPartHeight.Middle:
			heightInt = BodyPartHeight.Top;
			targetBodyPart = null;
			break;
		case BodyPartHeight.Top:
			heightInt = BodyPartHeight.Undefined;
			targetBodyPart = null;
			break;
		case BodyPartHeight.Undefined:
			heightInt = BodyPartHeight.Bottom;
			targetBodyPart = null;
			break;
		}
	}

	[Multiplayer.SyncMethod]
	private void ChangeCurrentPart(BodyPartDef def)
	{
		targetBodyPart = def;
	}
}
