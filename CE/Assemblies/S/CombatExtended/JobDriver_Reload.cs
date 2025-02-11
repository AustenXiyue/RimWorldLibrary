using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_Reload : JobDriver
{
	private CompAmmoUser _compReloader = null;

	private ThingWithComps initEquipment = null;

	private Thing initAmmo = null;

	private bool reloadingInventory = false;

	private bool reloadingEquipment = false;

	private string errorBase => GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ";

	private TargetIndex indReloader => TargetIndex.A;

	private Pawn holder => base.TargetThingA as Pawn;

	private TargetIndex indWeapon => TargetIndex.B;

	private ThingWithComps weapon => base.TargetThingB as ThingWithComps;

	private bool weaponEquipped => pawn?.equipment?.Primary == weapon;

	private bool weaponInInventory => pawn?.inventory?.innerContainer.Contains(weapon) == true;

	private CompAmmoUser compReloader
	{
		get
		{
			if (_compReloader == null)
			{
				_compReloader = weapon.TryGetComp<CompAmmoUser>();
			}
			return _compReloader;
		}
	}

	public override string GetReport()
	{
		string reportString = CE_JobDefOf.ReloadWeapon.reportString;
		string newValue = "";
		if (reloadingEquipment)
		{
			newValue = "CE_ReloadingEquipment".Translate();
		}
		if (reloadingInventory)
		{
			newValue = "CE_ReloadingInventory".Translate();
		}
		reportString = reportString.Replace("FlagSource", newValue);
		reportString = reportString.Replace("TargetB", weapon.def.label);
		if (Controller.settings.EnableAmmoSystem && initAmmo != null)
		{
			return reportString.Replace("AmmoType", initAmmo.LabelNoCount);
		}
		return reportString.Replace("AmmoType", "CE_ReloadingGenericAmmo".Translate());
	}

	private bool HasNoGunOrAmmo()
	{
		if ((reloadingEquipment && (pawn?.equipment?.Primary == null || pawn.equipment.Primary != weapon)) || (reloadingInventory && (pawn.inventory == null || !pawn.inventory.innerContainer.Contains(weapon))) || initEquipment != pawn?.equipment?.Primary)
		{
			return true;
		}
		return compReloader == null || !compReloader.HasAndUsesAmmoOrMagazine;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		if (holder == null)
		{
			Log.Error(errorBase + "TargetThingA is null.  A Pawn is required to perform a reload.");
			yield return null;
		}
		if (weapon == null)
		{
			Log.Error(errorBase + "TargetThingB is null.  A weapon (ThingWithComps) is required to perform a reload.");
			yield return null;
		}
		if (compReloader == null)
		{
			Log.Error(errorBase + pawn?.ToString() + " tried to do reload job on " + weapon.LabelCap + " which doesn't have a required CompAmmoUser.");
			yield return null;
		}
		if (holder != pawn)
		{
			Log.Error(errorBase + "TargetThingA (" + holder.Name?.ToString() + ") is not the same Pawn (" + pawn.Name?.ToString() + ") that was given the job.");
			yield return null;
		}
		reloadingInventory = weaponInInventory;
		reloadingEquipment = weaponEquipped;
		if (!reloadingInventory && !reloadingEquipment)
		{
			Log.Error(errorBase + "Unable to find the weapon to be reloaded (" + weapon.LabelCap + ") in the inventory nor equipment of " + pawn.Name);
			yield return null;
		}
		if (reloadingInventory && reloadingEquipment)
		{
			Log.Error(errorBase + "Something went spectacularly wrong as the weapon to be reloaded was found in both the Pawn's equipment AND inventory at the same time.");
			yield return null;
		}
		initEquipment = pawn.equipment?.Primary;
		if (compReloader.UseAmmo)
		{
			this.FailOn(() => !compReloader.TryFindAmmoInInventory(out initAmmo));
		}
		this.FailOnDespawnedOrNull(indReloader);
		if (pawn.MentalStateDef != MentalStateDefOf.Berserk && pawn.MentalStateDef != MentalStateDefOf.BerserkMechanoid)
		{
			this.FailOnMentalState(indReloader);
		}
		this.FailOnDestroyedOrNull(indWeapon);
		this.FailOn(HasNoGunOrAmmo);
		if (compReloader.ShouldThrowMote && holder.Map != null)
		{
			MoteMakerCE.ThrowText(pawn.Position.ToVector3Shifted(), holder.Map, string.Format("CE_ReloadingMote".Translate(), weapon.def.LabelCap));
		}
		Toil waitToil = new Toil
		{
			actor = pawn
		};
		bool hasCasing = true;
		waitToil.initAction = delegate
		{
			waitToil.actor.pather.StopDead();
		};
		waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
		waitToil.defaultDuration = Mathf.CeilToInt((float)weapon.GetStatValue(CE_StatDefOf.ReloadTime).SecondsToTicks() / pawn.GetStatValue(CE_StatDefOf.ReloadSpeed));
		waitToil.AddPreTickAction(delegate
		{
			if (hasCasing && (waitToil.actor.jobs.curDriver.ticksLeftThisToil == waitToil.defaultDuration - 30 || waitToil.actor.jobs.curDriver.ticksLeftThisToil == 1))
			{
				compReloader.DropCasing(compReloader.Props.magazineSize);
				hasCasing = false;
			}
		});
		yield return waitToil.WithProgressBarToilDelay(indReloader);
		Toil reloadToil = new Toil();
		reloadToil.AddFinishAction(delegate
		{
			compReloader.LoadAmmo(initAmmo);
		});
		yield return reloadToil;
		yield return Toils_Jump.JumpIf(condition: () => compReloader.Props.reloadOneAtATime && compReloader.CurMagCount < compReloader.MagSize && (!compReloader.UseAmmo || compReloader.TryFindAmmoInInventory(out initAmmo)), jumpTarget: waitToil);
		yield return new Toil
		{
			defaultCompleteMode = ToilCompleteMode.Instant
		};
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}
}
