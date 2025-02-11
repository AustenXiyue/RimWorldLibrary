using System.Collections.Generic;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.AI;

public class CompUrgentWeaponPickup : ICompTactics
{
	private const int BULLETIMPACT_COOLDOWN = 800;

	private const int PRIMARY_OPTIMIZE_COOLDOWN = 1500;

	private int lastBulletImpact = -1;

	private int lastPrimaryOptimization = -1;

	public bool BulletImpactedRecently => GenTicks.TicksGame - lastBulletImpact < 800;

	public bool PrimaryOptimizatedRecently => GenTicks.TicksGame - lastPrimaryOptimization < 1500;

	public override int Priority => 20;

	public override void Notify_BulletImpactNearBy()
	{
		base.Notify_BulletImpactNearBy();
		if (!BulletImpactedRecently && !PrimaryOptimizatedRecently)
		{
			lastBulletImpact = GenTicks.TicksGame;
			CheckPrimaryEquipment();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastBulletImpact, "lastBulletImpact", -1);
		Scribe_Values.Look(ref lastPrimaryOptimization, "lastPrimaryOptimization", -1);
	}

	private void CheckPrimaryEquipment()
	{
		if (SelPawn.Faction.IsPlayerSafe() || SelPawn.RaceProps.IsMechanoid || !SelPawn.RaceProps.Humanlike || SelPawn.equipment == null || SelPawn.equipment.Primary != null || SelPawn.Downed || SelPawn.InMentalState)
		{
			return;
		}
		CompInventory compInventory = CompInventory;
		if (compInventory == null || compInventory.SwitchToNextViableWeapon(useFists: false, useAOE: true, stopJob: false) || CompInventory.rangedWeaponList == null || (SelPawn.story != null && SelPawn.WorkTagIsDisabled(WorkTags.Violent)))
		{
			return;
		}
		foreach (ThingWithComps rangedWeapon in CompInventory.rangedWeaponList)
		{
			CompAmmoUser compAmmoUser = rangedWeapon.TryGetComp<CompAmmoUser>();
			if (compAmmoUser == null || !compAmmoUser.TryPickupAmmo())
			{
				continue;
			}
			lastPrimaryOptimization = GenTicks.TicksGame;
			SelPawn.equipment.equipment.TryAddOrTransfer(rangedWeapon);
			return;
		}
		IEnumerable<AmmoThing> enumerable = (from t in SelPawn.Position.AmmoInRange(base.Map, 15f)
			where t != null
			select t) ?? new List<AmmoThing>();
		foreach (ThingWithComps item in from t in SelPawn.Position.WeaponsInRange(base.Map, 15f)
			orderby t.Position.DistanceTo(SelPawn.Position)
			select t)
		{
			if (!(item is ThingWithComps thingWithComps))
			{
				continue;
			}
			CompAmmoUser compAmmoUser2 = thingWithComps.TryGetComp<CompAmmoUser>();
			if (!SelPawn.CanReach(item, PathEndMode.InteractionCell, Danger.Unspecified) || !SelPawn.CanReserve(thingWithComps) || compAmmoUser2 == null)
			{
				continue;
			}
			IEnumerable<AmmoDef> enumerable2 = compAmmoUser2.Props?.ammoSet?.ammoTypes?.Select((AmmoLink a) => a.ammo) ?? null;
			if (enumerable2 == null)
			{
				continue;
			}
			foreach (AmmoThing item2 in enumerable)
			{
				if (!enumerable2.Contains(item2.AmmoDef) || !SelPawn.CanReach(item2, PathEndMode.InteractionCell, Danger.Unspecified) || !SelPawn.CanReserve(item2) || !CompInventory.CanFitInInventory(item2, out var count))
				{
					continue;
				}
				lastPrimaryOptimization = GenTicks.TicksGame;
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
				job.count = count;
				SelPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				Job j = JobMaker.MakeJob(JobDefOf.Equip, thingWithComps);
				SelPawn.jobs.jobQueue.EnqueueFirst(j, null);
				return;
			}
		}
		foreach (ThingWithComps item3 in SelPawn.Position.WeaponsInRange(base.Map, 15f))
		{
			if (!SelPawn.CanReach(item3, PathEndMode.InteractionCell, Danger.Unspecified) || !SelPawn.CanReserve(item3) || item3.def.IsRangedWeapon)
			{
				continue;
			}
			lastPrimaryOptimization = GenTicks.TicksGame;
			Job newJob = JobMaker.MakeJob(JobDefOf.Equip, item3);
			SelPawn.jobs.StartJob(newJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			break;
		}
	}
}
