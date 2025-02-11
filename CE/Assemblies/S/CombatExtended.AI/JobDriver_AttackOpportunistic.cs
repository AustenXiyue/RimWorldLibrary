using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended.AI;

public class JobDriver_AttackOpportunistic : IJobDriver_Tactical
{
	private ThingWithComps oldWeapon;

	private ThingWithComps _jobWeapon;

	private Thing _jobAmmo;

	public ThingWithComps JobWeapon
	{
		get
		{
			if (_jobWeapon == null)
			{
				_jobWeapon = (ThingWithComps)base.TargetA.Thing;
			}
			return _jobWeapon;
		}
	}

	public Thing JobAmmo
	{
		get
		{
			if (_jobAmmo == null)
			{
				_jobAmmo = base.TargetC.Thing;
			}
			return _jobAmmo;
		}
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		if (pawn.equipment == null)
		{
			Log.Warning("CE: JobDriver_AttackStaticOnce recived a pawn with equipment = null");
			yield break;
		}
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOn(() => pawn.equipment == null);
		yield return Toils_General.Do(delegate
		{
			if (pawn.equipment.Primary != null)
			{
				oldWeapon = pawn.equipment.Primary;
				pawn.equipment.TryTransferEquipmentToContainer(oldWeapon, CompInventory.container);
			}
			ThingWithComps thingWithComps = JobWeapon;
			if (thingWithComps.stackCount > 1)
			{
				thingWithComps = (ThingWithComps)thingWithComps.SplitOff(1);
			}
			pawn.equipment.equipment.TryAddOrTransfer(thingWithComps);
			ReadyForNextToil();
		}).FailOn(() => JobWeapon == null || JobWeapon.Destroyed);
		if (JobWeapon.stackCount == 1)
		{
			CompAmmoUser compAmmo = JobWeapon.TryGetComp<CompAmmoUser>();
			if (compAmmo != null && compAmmo.UseAmmo && (compAmmo.EmptyMagazine || JobAmmo != null))
			{
				yield return Toils_CombatCE.ReloadEquipedWeapon(this, TargetIndex.A, JobAmmo);
			}
		}
		foreach (Toil item in Toils_CombatCE.AttackStatic(this, TargetIndex.B))
		{
			yield return item;
		}
		AddFinishAction(delegate
		{
			if (oldWeapon != null && oldWeapon != pawn.equipment?.Primary && !oldWeapon.Destroyed)
			{
				if (pawn.equipment?.Primary != null)
				{
					pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment?.Primary, CompInventory.container);
				}
				pawn.equipment.equipment.TryAddOrTransfer(oldWeapon);
			}
		});
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref oldWeapon, "oldWeapon");
	}
}
