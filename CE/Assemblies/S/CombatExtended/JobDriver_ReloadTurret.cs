using System.Collections.Generic;
using CombatExtended.CombatExtended.LoggerUtils;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_ReloadTurret : JobDriver
{
	private CompAmmoUser _compReloader;

	private string errorBase => GetType().Assembly.GetName().Name + " :: " + GetType().Name + " :: ";

	private Building_Turret turret => base.TargetThingA as Building_Turret;

	private AmmoThing ammo => base.TargetThingB as AmmoThing;

	private CompAmmoUser compReloader
	{
		get
		{
			if (_compReloader == null && turret != null)
			{
				_compReloader = turret.GetAmmo();
			}
			return _compReloader;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(base.TargetA, job))
		{
			CELogger.Message("Combat Extended: Could not reserve turret for reloading job.", showOutOfDebugMode: false, "TryMakePreToilReservations");
			return false;
		}
		CompAmmoUser compAmmoUser = turret?.GetAmmo();
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

	public override string GetReport()
	{
		string reportString = CE_JobDefOf.ReloadTurret.reportString;
		string newValue = ((turret.TryGetComp<CompMannable>() != null) ? "CE_MannedTurret" : "CE_AutoTurret").Translate();
		reportString = reportString.Replace("TurretType", newValue);
		reportString = reportString.Replace("TargetA", base.TargetThingA.def.label);
		return compReloader.UseAmmo ? reportString.Replace("TargetB", base.TargetThingB.def.label) : reportString.Replace("TargetB", "CE_ReloadingGenericAmmo".Translate());
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		if (turret == null)
		{
			Log.Error(errorBase + "TargetThingA isn't a Building_TurretGunCE");
			yield return null;
		}
		if (compReloader == null)
		{
			Log.Error(errorBase + "TargetThingA (Building_TurretGunCE) is missing its CompAmmoUser.");
			yield return null;
		}
		if (compReloader.UseAmmo && ammo == null)
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
		this.FailOn(() => compReloader.MissingToFullMagazine == 0);
		if (compReloader.UseAmmo)
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
		_ = turret.InteractionCell;
		if (true)
		{
			yield return Toils_Goto.GotoCell(turret.InteractionCell, PathEndMode.OnCell);
		}
		else
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		}
		AddFinishAction(delegate
		{
			turret.SetReloading(reloading: false);
		});
		Toil waitToil = new Toil
		{
			actor = pawn
		};
		waitToil.initAction = delegate
		{
			turret.SetReloading(reloading: true);
			waitToil.actor.pather.StopDead();
			if (compReloader.ShouldThrowMote)
			{
				MoteMakerCE.ThrowText(turret.Position.ToVector3Shifted(), turret.Map, string.Format("CE_ReloadingTurretMote".Translate(), base.TargetThingA.LabelCapNoCount));
			}
			AmmoDef currentAmmo = compReloader.CurrentAmmo;
			if (currentAmmo != ammo?.def)
			{
				compReloader.TryUnload(out var _);
			}
		};
		waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
		waitToil.defaultDuration = Mathf.CeilToInt((float)compReloader.Props.reloadTime.SecondsToTicks() / pawn.GetStatValue(CE_StatDefOf.ReloadSpeed));
		yield return waitToil.WithProgressBarToilDelay(TargetIndex.A);
		Toil reloadToil = new Toil
		{
			defaultCompleteMode = ToilCompleteMode.Instant,
			initAction = delegate
			{
				compReloader.LoadAmmo(ammo);
				turret.SetReloading(reloading: false);
			}
		};
		Toil jumpToil = Toils_Jump.JumpIf(condition: () => compReloader.Props.reloadOneAtATime && compReloader.CurMagCount < compReloader.MagSize && (!compReloader.UseAmmo || TryFindAmmo(pawn, compReloader, ammo)), jumpTarget: waitToil);
		yield return reloadToil;
		yield return jumpToil;
	}

	private bool TryFindAmmo(Pawn byPawn, CompAmmoUser comp, Thing ammoThing)
	{
		Thing thing = ((byPawn.carryTracker.CarriedThing?.def == ammoThing.def) ? byPawn.carryTracker.CarriedThing : null);
		CompInventory compInventory = byPawn.TryGetComp<CompInventory>();
		if (compInventory != null)
		{
			Thing thing2 = compInventory.ammoList.Find((Thing x) => x.def == ammoThing.def);
			if (thing2 != null)
			{
				Thing thing3 = thing2;
				compInventory.ammoList.Remove(thing2);
				byPawn.TryGetComp<CompInventory>().UpdateInventory();
				byPawn.carryTracker.TryStartCarry(thing2);
				thing = thing2;
			}
		}
		if (thing == null)
		{
			return false;
		}
		ammoThing = thing;
		return true;
	}
}
