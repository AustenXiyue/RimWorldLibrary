using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended.AI;

public static class Toils_CombatCE
{
	public static Toil ReloadEquipedWeapon(IJobDriver_Tactical driver, TargetIndex progressIndex, Thing ammo = null)
	{
		CompAmmoUser compAmmo = null;
		int reloadingTime = 0;
		int startTick = 0;
		Toil toil = new Toil
		{
			actor = driver.pawn
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 300;
		toil.AddPreInitAction(delegate
		{
			if (driver.pawn.equipment?.Primary == null)
			{
				driver.EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				compAmmo = driver.pawn.equipment?.Primary.TryGetComp<CompAmmoUser>();
				if (compAmmo == null || !compAmmo.HasAmmoOrMagazine)
				{
					driver.EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					startTick = GenTicks.TicksGame;
					reloadingTime = Mathf.CeilToInt((float)compAmmo.parent.GetStatValue(CE_StatDefOf.ReloadTime).SecondsToTicks() / driver.pawn.GetStatValue(CE_StatDefOf.ReloadSpeed));
				}
			}
		});
		toil.tickAction = delegate
		{
			if (GenTicks.TicksGame - startTick >= reloadingTime)
			{
				if (compAmmo == null)
				{
					driver.EndJobWith(JobCondition.Incompletable);
				}
				else if (ammo == null && !compAmmo.TryFindAmmoInInventory(out ammo))
				{
					driver.EndJobWith(JobCondition.Incompletable);
				}
				else if (!compAmmo.EmptyMagazine && !compAmmo.TryUnload())
				{
					driver.EndJobWith(JobCondition.Incompletable);
				}
				else
				{
					compAmmo.LoadAmmo(ammo);
					driver.ReadyForNextToil();
				}
			}
		};
		return toil.WithProgressBarToilDelay(progressIndex);
	}

	public static IEnumerable<Toil> AttackStatic(IJobDriver_Tactical driver, TargetIndex targetIndex)
	{
		Toil init = new Toil();
		bool startedIncapacitated = false;
		init.initAction = delegate
		{
			if (((JobDriver)driver).TargetThingB is Pawn pawn)
			{
				startedIncapacitated = pawn.Downed;
			}
			driver.pawn.pather.StopDead();
		};
		init.tickAction = delegate
		{
			LocalTargetInfo target = driver.job.GetTarget(targetIndex);
			if (!driver.job.GetTarget(targetIndex).IsValid)
			{
				driver.EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				if (target.HasThing)
				{
					Pawn pawn2 = target.Thing as Pawn;
					if (target.Thing.Destroyed || (pawn2 != null && !startedIncapacitated && pawn2.Downed) || (pawn2 != null && !(pawn2.GetInvisibilityComp()?.ForcedVisible ?? false)))
					{
						driver.EndJobWith(JobCondition.Succeeded);
						return;
					}
				}
				if (driver.numAttacksMade >= driver.job.maxNumStaticAttacks && !driver.pawn.stances.FullBodyBusy)
				{
					driver.EndJobWith(JobCondition.Succeeded);
				}
				else if (driver.pawn.TryStartAttack(target))
				{
					driver.numAttacksMade++;
				}
				else if (!driver.pawn.stances.FullBodyBusy)
				{
					driver.EndJobWith(JobCondition.Incompletable);
				}
			}
		};
		init.defaultCompleteMode = ToilCompleteMode.Never;
		init.activeSkill = () => Toils_Combat.GetActiveSkillForToil(init);
		yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
		yield return init;
	}
}
