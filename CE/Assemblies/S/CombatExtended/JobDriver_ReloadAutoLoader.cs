using System.Collections.Generic;
using CombatExtended.CombatExtended.LoggerUtils;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_ReloadAutoLoader : JobDriver
{
	private CompAmmoUser _compReloader;

	private string errorBase => GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ";

	private Building_AutoloaderCE AutoLoader => base.TargetThingA as Building_AutoloaderCE;

	private AmmoThing ammo => base.TargetThingB as AmmoThing;

	private CompAmmoUser AmmoUser
	{
		get
		{
			if (_compReloader == null && AutoLoader != null)
			{
				_compReloader = AutoLoader.CompAmmoUser;
			}
			return _compReloader;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(base.TargetA, job))
		{
			CELogger.Message("Combat Extended: Could not reserve ammo container for reloading job.", showOutOfDebugMode: false, "TryMakePreToilReservations");
			return false;
		}
		CompAmmoUser compAmmoUser = AutoLoader?.CompAmmoUser;
		if (compAmmoUser == null)
		{
			CELogger.Error($"{base.TargetA} has no CompAmmo, this should not have been reached.", showOutOfDebugMode: true, "TryMakePreToilReservations");
			return false;
		}
		if (!compAmmoUser.UseAmmo)
		{
			return true;
		}
		if (ammo == null)
		{
			CELogger.Message("Combat Extended: Ammo is null", showOutOfDebugMode: false, "TryMakePreToilReservations");
			return false;
		}
		if (!pawn.Reserve(base.TargetB, job, Mathf.Max(1, base.TargetThingB.stackCount - job.count), job.count))
		{
			CELogger.Message("Combat Extended: Could not reserve " + Mathf.Max(1, base.TargetThingB.stackCount - job.count) + " of ammo.", showOutOfDebugMode: false, "TryMakePreToilReservations");
			return false;
		}
		CELogger.Message("Combat Extended: Managed to reserve everything successfully.", showOutOfDebugMode: false, "TryMakePreToilReservations");
		return true;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		if (AutoLoader == null)
		{
			Log.Error(errorBase + "TargetThingA isn't a Building_AutoLoaderCE");
			yield return null;
		}
		if (AmmoUser == null)
		{
			Log.Error(errorBase + "TargetThingA (Building_AutoLoaderCE) is missing its CompAmmoUser.");
			yield return null;
		}
		if (AmmoUser.UseAmmo && ammo == null)
		{
			Log.Error(errorBase + "TargetThingB is either null or not an AmmoThing.");
			yield return null;
		}
		AddEndCondition(() => (!pawn.Downed && !pawn.Dead && !pawn.InMentalState && !pawn.IsBurning()) ? JobCondition.Ongoing : JobCondition.Incompletable);
		this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
		if (pawn.Faction != Faction.OfPlayer)
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
		}
		else
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
		}
		this.FailOn(() => AmmoUser.MissingToFullMagazine == 0);
		if (AmmoUser.UseAmmo)
		{
			Toil toilGoToCell = Toils_Goto.GotoCell(ammo.Position, PathEndMode.Touch).FailOnBurningImmobile(TargetIndex.B);
			Toil toilCarryThing = Toils_Haul.StartCarryThing(TargetIndex.B).FailOnBurningImmobile(TargetIndex.B);
			if (base.TargetThingB is AmmoThing)
			{
				toilGoToCell.AddEndCondition(() => (!(base.TargetThingB as AmmoThing).IsCookingOff) ? JobCondition.Ongoing : JobCondition.Incompletable);
				toilCarryThing.AddEndCondition(() => (!(base.TargetThingB as AmmoThing).IsCookingOff) ? JobCondition.Ongoing : JobCondition.Incompletable);
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				ammo.SetForbidden(value: true, warnOnFail: false);
				toilGoToCell.FailOnDestroyedOrNull(TargetIndex.B);
				toilCarryThing.FailOnDestroyedOrNull(TargetIndex.B);
			}
			else
			{
				toilGoToCell.FailOnDestroyedNullOrForbidden(TargetIndex.B);
				toilCarryThing.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			}
			yield return toilGoToCell;
			yield return toilCarryThing;
		}
		yield return Toils_Goto.GotoCell(AutoLoader.Position, PathEndMode.Touch);
		AddFinishAction(delegate
		{
			AutoLoader.isReloading = false;
		});
		Toil waitToil = new Toil
		{
			actor = pawn
		};
		waitToil.initAction = delegate
		{
			AutoLoader.isReloading = true;
			waitToil.actor.pather.StopDead();
			if (AmmoUser.ShouldThrowMote)
			{
				MoteMakerCE.ThrowText(AutoLoader.Position.ToVector3Shifted(), AutoLoader.Map, string.Format("CE_ReloadingTurretMote".Translate(), base.TargetThingA.LabelCapNoCount));
			}
			AmmoDef currentAmmo = AmmoUser.CurrentAmmo;
			if (currentAmmo != ammo?.def)
			{
				AutoLoader.DropAmmo();
			}
		};
		waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
		waitToil.defaultDuration = Mathf.CeilToInt((float)AmmoUser.Props.reloadTime.SecondsToTicks() / pawn.GetStatValue(CE_StatDefOf.ReloadSpeed));
		yield return waitToil.WithProgressBarToilDelay(TargetIndex.A);
		yield return new Toil
		{
			defaultCompleteMode = ToilCompleteMode.Instant,
			initAction = delegate
			{
				AmmoUser.LoadAmmo(ammo);
				AutoLoader.isReloading = false;
				AutoLoader.Notify_ColorChanged();
			}
		};
	}
}
