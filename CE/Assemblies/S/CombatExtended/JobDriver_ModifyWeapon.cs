using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_ModifyWeapon : JobDriver
{
	public WeaponPlatform EquipedWeapon => (WeaponPlatform)pawn.equipment.Primary;

	public WeaponPlatform Weapon => (WeaponPlatform)job.targetQueueA[0].Thing;

	public Building GunsmithingTable => base.TargetA.Thing as Building;

	public AttachmentDef AttachmentDef => (job as JobCE).targetThingDefs[0] as AttachmentDef;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		Thing thing = job.GetTarget(TargetIndex.A).Thing;
		if (Weapon.Spawned && !pawn.Reserve(thing, job, 1, 1, null, errorOnFailed))
		{
			return false;
		}
		if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (thing != null && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed))
		{
			return false;
		}
		for (int i = 0; i < job.targetQueueB.Count; i++)
		{
			if (!pawn.Reserve(job.targetQueueB[i], job, 1, job.countQueue[i], null, errorOnFailed))
			{
				return false;
			}
		}
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		return true;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		AddEndCondition(delegate
		{
			Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
			return (!(thing is Building) || thing.Spawned) ? JobCondition.Ongoing : JobCondition.Incompletable;
		});
		this.FailOn(() => Weapon.Destroyed);
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);
		Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
		foreach (Toil item in CollectIngredientsToils())
		{
			yield return item;
		}
		yield return gotoBillGiver;
		yield return new Toil
		{
			defaultDuration = 30,
			defaultCompleteMode = ToilCompleteMode.Delay
		}.WithProgressBarToilDelay(TargetIndex.A);
		yield return new Toil
		{
			defaultCompleteMode = ToilCompleteMode.Instant,
			initAction = delegate
			{
				if (TryModifyWeapon())
				{
					EndJobWith(JobCondition.Succeeded);
				}
				else
				{
					EndJobWith(JobCondition.Incompletable);
				}
			}
		};
	}

	private IEnumerable<Toil> CollectIngredientsToils()
	{
		Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
		yield return extract;
		yield return Toils_Jump.JumpIf(extract, () => (GunsmithingTable as IBillGiver).IngredientStackCells.Contains(job.GetTarget(TargetIndex.B).Thing.Position));
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: true, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.A);
		Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
		yield return findPlaceTarget;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, storageMode: false);
		yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
	}

	private bool TryModifyWeapon()
	{
		AttachmentLink attachmentLink = Weapon.attachments.FirstOrDefault((AttachmentLink l) => l.attachment == AttachmentDef);
		if (attachmentLink != null)
		{
			if (!TryRefundIngredient(attachmentLink.attachment))
			{
				Log.Warning("CE: Refunding attachment cost failed " + attachmentLink.attachment.label);
			}
			Weapon.attachments.Remove(attachmentLink);
			Weapon.UpdateConfiguration();
			return true;
		}
		if (TryConsumeIngredient())
		{
			Weapon.attachments.Add(Weapon.Platform.attachmentLinks.First((AttachmentLink l) => l.attachment == AttachmentDef));
			Weapon.UpdateConfiguration();
			return true;
		}
		return false;
	}

	private bool TryRefundIngredient(AttachmentDef attachmentDef)
	{
		foreach (ThingDefCountClass cost in attachmentDef.costList)
		{
			Thing thing = ThingMaker.MakeThing(cost.thingDef, cost.thingDef.defaultStuff);
			thing.stackCount = cost.count;
			if (!GenPlace.TryFindPlaceSpotNear(pawn.Position, pawn.Rotation, pawn.Map, thing, allowStacking: true, out IntVec3 bestSpot, (Predicate<IntVec3>)null))
			{
				return false;
			}
			GenSpawn.Spawn(thing, bestSpot, pawn.Map);
		}
		return true;
	}

	private bool TryConsumeIngredient()
	{
		List<Thing> list = new List<Thing>();
		List<Thing>[] array = (from c in pawn.CellsAdjacent8WayAndInside()
			select c.GetThingList(pawn.Map).ToList()).ToArray();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (ThingDefCountClass cost in AttachmentDef.costList)
		{
			dictionary[cost.thingDef.index] = cost.count;
		}
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Thing item in array[i])
			{
				if (dictionary.TryGetValue(item.def.index, out var value) && value > 0)
				{
					int num = Math.Min(item.stackCount, value);
					list.Add((item.stackCount == num && item.stackCount != 1) ? item : item.SplitOff(num));
					dictionary[item.def.index] = value - num;
				}
			}
		}
		foreach (ThingDefCountClass cost2 in AttachmentDef.costList)
		{
			if (dictionary[cost2.thingDef.index] > 0)
			{
				return false;
			}
		}
		foreach (Thing item2 in list)
		{
			item2.Destroy();
		}
		return true;
	}
}
