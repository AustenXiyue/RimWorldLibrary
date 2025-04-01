using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_Psycast : AbilityExtension_AbilityMod
{
	public float entropyGain = 0f;

	public List<StatModifier> entropyGainStatFactors = new List<StatModifier>();

	public int level;

	public int order;

	public PsycasterPathDef path;

	public List<AbilityDef> prerequisites = new List<AbilityDef>();

	public bool psychic;

	public float psyfocusCost = 0f;

	public bool showCastBubble = true;

	public bool spaceAfter;

	public override bool ShowGizmoOnPawn(Pawn pawn)
	{
		return !pawn.Psycasts().previousUnlockedPaths.Contains(path);
	}

	public bool PrereqsCompleted(Pawn pawn)
	{
		return PrereqsCompleted(((ThingWithComps)pawn).GetComp<CompAbilities>());
	}

	public bool PrereqsCompleted(CompAbilities compAbilities)
	{
		return prerequisites.NullOrEmpty() || compAbilities.LearnedAbilities.Any((Ability ab) => prerequisites.Contains(ab.def));
	}

	public void UnlockWithPrereqs(CompAbilities compAbilities)
	{
		foreach (AbilityDef prerequisite in prerequisites)
		{
			AbilityExtension_Psycast modExtension = ((Def)(object)prerequisite).GetModExtension<AbilityExtension_Psycast>();
			if (modExtension != null)
			{
				modExtension.UnlockWithPrereqs(compAbilities);
			}
			else
			{
				compAbilities.GiveAbility(prerequisite);
			}
		}
		compAbilities.GiveAbility(base.abilityDef);
	}

	public float GetPsyfocusUsedByPawn(Pawn pawn)
	{
		return psyfocusCost * pawn.GetStatValue(VPE_DefOf.VPE_PsyfocusCostFactor);
	}

	public float GetEntropyUsedByPawn(Pawn pawn)
	{
		return entropyGainStatFactors.Aggregate(entropyGain, (float current, StatModifier statFactor) => current * (pawn.GetStatValue(statFactor.stat) * statFactor.value));
	}

	public override bool IsEnabledForPawn(Ability ability, out string reason)
	{
		if (!path.CanPawnUnlock(ability.pawn) && !path.ignoreLockRestrictionsForNeurotrainers)
		{
			reason = path.lockedReason;
			return false;
		}
		Hediff_PsycastAbilities hediff_PsycastAbilities = ability?.pawn?.Psycasts();
		if (hediff_PsycastAbilities != null)
		{
			if (ability.pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
			{
				reason = "CommandPsycastZeroPsychicSensitivity".Translate();
				return false;
			}
			float psyfocusUsedByPawn = GetPsyfocusUsedByPawn(ability.pawn);
			if (!hediff_PsycastAbilities.SufficientPsyfocusPresent(psyfocusUsedByPawn))
			{
				reason = "CommandPsycastNotEnoughPsyfocus".Translate(psyfocusUsedByPawn.ToStringPercent("#.0"), ability.pawn.psychicEntropy.CurrentPsyfocus.ToStringPercent("#.0"), ((Def)(object)ability.def).label.Named("PSYCASTNAME"), ability.pawn.Named("CASTERNAME"));
				return false;
			}
			if (ability.pawn.psychicEntropy.WouldOverflowEntropy(GetEntropyUsedByPawn(ability.pawn)))
			{
				reason = "CommandPsycastWouldExceedEntropy".Translate(((Def)(object)ability.def).label);
				return false;
			}
			if (hediff_PsycastAbilities.CurrentlyChanneling != null)
			{
				reason = "VPE.CurrentChanneling".Translate(((Def)(object)hediff_PsycastAbilities.CurrentlyChanneling.def).LabelCap);
				return false;
			}
			if (ability.pawn.Downed)
			{
				reason = "IsIncapped".Translate(ability.pawn.LabelShort, ability.pawn);
				return false;
			}
			reason = string.Empty;
			return true;
		}
		reason = "VPE.NotPsycaster".Translate();
		return false;
	}

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		Hediff_PsycastAbilities hediff_PsycastAbilities = (Hediff_PsycastAbilities)(object)ability.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant);
		hediff_PsycastAbilities.UseAbility(GetPsyfocusUsedByPawn(ability.pawn), GetEntropyUsedByPawn(ability.pawn));
		if (ability is IChannelledPsycast psycast)
		{
			hediff_PsycastAbilities.BeginChannelling(psycast);
		}
	}

	public override string GetDescription(Ability ability)
	{
		StringBuilder stringBuilder = new StringBuilder();
		float psyfocusUsedByPawn = GetPsyfocusUsedByPawn(ability.pawn);
		if (psyfocusUsedByPawn > float.Epsilon)
		{
			stringBuilder.AppendInNewLine(string.Format("{0}: {1}", "AbilityPsyfocusCost".Translate(), psyfocusUsedByPawn.ToStringPercent()));
		}
		float entropyUsedByPawn = GetEntropyUsedByPawn(ability.pawn);
		if (entropyUsedByPawn > float.Epsilon)
		{
			stringBuilder.AppendInNewLine(string.Format("{0}: {1}", "AbilityEntropyGain".Translate(), entropyUsedByPawn));
		}
		return stringBuilder.ToString().Colorize(Color.cyan);
	}

	public override void WarmupToil(Toil toil)
	{
		((AbilityExtension_AbilityMod)this).WarmupToil(toil);
		if (showCastBubble)
		{
			toil.AddPreInitAction(delegate
			{
				MoteCastBubble moteCastBubble = (MoteCastBubble)ThingMaker.MakeThing(VPE_DefOf.VPE_Mote_Cast);
				moteCastBubble.Setup(toil.actor, ((ThingWithComps)toil.actor).GetComp<CompAbilities>().currentlyCasting);
				GenSpawn.Spawn(moteCastBubble, toil.actor.Position, toil.actor.Map);
			});
		}
	}

	public override void TargetingOnGUI(LocalTargetInfo target, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).TargetingOnGUI(target, ability);
		if (!psychic)
		{
			return;
		}
		List<GlobalTargetInfo> list = ability.currentTargets.Where((GlobalTargetInfo t) => t.IsValid && t.Map != null).ToList();
		GlobalTargetInfo[] array = new GlobalTargetInfo[list.Count + 1];
		list.CopyTo(array, 0);
		array[array.Length - 1] = target.ToGlobalTargetInfo(list?.LastOrDefault().Map ?? ability.pawn.Map);
		ability.ModifyTargets(ref array);
		GlobalTargetInfo[] array2 = array;
		foreach (GlobalTargetInfo globalTargetInfo in array2)
		{
			if (globalTargetInfo.Thing is Pawn pawn)
			{
				float statValue = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
				if (statValue < float.Epsilon)
				{
					Vector3 drawPos = pawn.DrawPos;
					drawPos.z += 1f;
					GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), "Ineffective".Translate(), Color.red);
				}
				else
				{
					Vector3 drawPos2 = pawn.DrawPos;
					drawPos2.z += 1f;
					GenMapUI.DrawText(new Vector2(drawPos2.x, drawPos2.z), StatDefOf.PsychicSensitivity.LabelCap + ": " + statValue.ToStringPercent(), (statValue > float.Epsilon) ? Color.white : Color.red);
				}
			}
		}
	}

	public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
	{
		bool flag = ((AbilityExtension_AbilityMod)this).Valid(targets, ability, throwMessages);
		if (flag)
		{
			string text = default(string);
			flag = ((AbilityExtension_AbilityMod)this).IsEnabledForPawn(ability, ref text);
			if (!flag && throwMessages)
			{
				Messages.Message(text, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		return flag;
	}

	public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
	{
		if (psychic)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null && pawn.GetStatValue(StatDefOf.PsychicSensitivity) < float.Epsilon)
			{
				if (throwMessages)
				{
					Messages.Message("Ineffective".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
		}
		return true;
	}
}
