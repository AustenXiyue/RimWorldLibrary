using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class CompArmorDurability : ThingComp
{
	public float curDurability;

	public bool regens = false;

	public int timer;

	public CompProperties_ArmorDurability durabilityProps => props as CompProperties_ArmorDurability;

	public float maxDurability => durabilityProps.Durability;

	public float curDurabilityPercent => (float)Math.Round(curDurability / maxDurability, 2);

	public override void CompTick()
	{
		if (durabilityProps.Regenerates)
		{
			if ((float)timer >= durabilityProps.RegenInterval)
			{
				if (curDurability < maxDurability)
				{
					curDurability += Math.Min(durabilityProps.RegenValue, maxDurability - curDurability);
				}
				timer = 0;
			}
			timer++;
		}
		base.CompTick();
	}

	public override void PostPostMake()
	{
		curDurability = maxDurability;
		regens = durabilityProps.Regenerates;
		base.PostPostMake();
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		regens = durabilityProps.Regenerates;
		base.PostSpawnSetup(respawningAfterLoad);
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref curDurability, "curDurability", 0f);
		Scribe_Values.Look(ref timer, "timer", 0);
		base.PostExposeData();
	}

	public override string CompInspectStringExtra()
	{
		if (maxDurability != 500f)
		{
			return "CE_ArmorDurability".Translate() + curDurability.ToString() + "/" + maxDurability.ToString() + " (" + curDurabilityPercent.ToStringPercent() + ")";
		}
		return null;
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		base.PostPreApplyDamage(ref dinfo, out absorbed);
		if (curDurability > 0f && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(parent))
		{
			curDurability -= dinfo.Amount;
			if (curDurability < 0f)
			{
				curDurability = 0f;
			}
		}
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!durabilityProps.Repairable || parent.HostileTo(selPawn))
		{
			yield break;
		}
		if (curDurability >= maxDurability + durabilityProps.MaxOverHeal)
		{
			yield return new FloatMenuOption("CE_ArmorDurability_CannotRepairUndamaged".Translate(), null);
			yield break;
		}
		if (durabilityProps.RepairIngredients.NullOrEmpty())
		{
			yield return new FloatMenuOption("CE_RepairArmorDurability".Translate(), delegate
			{
				StartJob(selPawn);
			});
			yield break;
		}
		bool needsSecondIngredient = durabilityProps.RepairIngredients.Count >= 1;
		Thing firstIngredient = FindIngredient(selPawn, durabilityProps.RepairIngredients.First());
		Thing secondIngredient = (needsSecondIngredient ? FindIngredient(selPawn, durabilityProps.RepairIngredients.Last()) : null);
		if (firstIngredient != null && (!needsSecondIngredient || secondIngredient != null))
		{
			yield return new FloatMenuOption("CE_RepairArmorDurability".Translate(), delegate
			{
				StartJob(selPawn, firstIngredient, secondIngredient);
			});
		}
		else
		{
			yield return new FloatMenuOption("CE_ArmorDurability_CannonRepairNoResource".Translate(), null);
		}
	}

	private static Thing FindIngredient(Pawn selPawn, ThingDefCountClass ingredientDefCount)
	{
		return GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForDef(ingredientDefCount.thingDef), PathEndMode.ClosestTouch, TraverseParms.For(selPawn), 9999f, IsValidIngredient);
		bool IsValidIngredient(Thing ingredient)
		{
			return ingredient.stackCount >= ingredientDefCount.count && !ingredient.IsForbidden(selPawn);
		}
	}

	[Multiplayer.SyncMethod]
	private void StartJob(Pawn selPawn, Thing firstIngredient = null, Thing secondIngredient = null)
	{
		Job job = JobMaker.MakeJob(CE_JobDefOf.RepairNaturalArmor);
		job.targetA = parent;
		if (firstIngredient != null)
		{
			job.targetB = firstIngredient;
		}
		if (secondIngredient != null)
		{
			job.targetC = secondIngredient;
		}
		selPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
	}
}
