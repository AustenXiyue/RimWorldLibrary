using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobDriver_RepairNaturalArmor : JobDriver
{
	private Pawn actor => GetActor();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		bool flag = base.TargetC.Thing == null;
		CompArmorDurability compArmorDurability = (base.TargetA.Thing as ThingWithComps).TryGetComp<CompArmorDurability>();
		if (!flag)
		{
			flag = actor.CanReserveAndReach(base.TargetC, PathEndMode.ClosestTouch, Danger.Some, 1, compArmorDurability.durabilityProps.RepairIngredients.Last().count);
		}
		bool flag2 = actor.CanReserveAndReach(base.TargetA, PathEndMode.ClosestTouch, Danger.Some, 1, 1) && actor.CanReserveAndReach(base.TargetB, PathEndMode.ClosestTouch, Danger.Some, 1, compArmorDurability.durabilityProps.RepairIngredients.First().count);
		return flag2 && flag;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		CompArmorDurability natArmor = (base.TargetA.Thing as ThingWithComps).TryGetComp<CompArmorDurability>();
		Thing targetB = base.TargetThingB;
		Thing targetC = base.TargetThingC;
		int countB = -1;
		int countC = -1;
		if (targetB != null)
		{
			countB = natArmor.durabilityProps.RepairIngredients.First().count;
			if (countB > 0)
			{
				pawn.Reserve(base.TargetB, job, 1, countB);
			}
		}
		if (targetC != null)
		{
			countC = natArmor.durabilityProps.RepairIngredients.Last().count;
			if (countC > 0)
			{
				pawn.Reserve(base.TargetC, job, 1, countC);
			}
		}
		if (targetB != null && countB > 0)
		{
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_General.Do(delegate
			{
				if (countB <= targetB.stackCount)
				{
					Thing thing = targetB.SplitOff(countB);
					thing.stackCount = countB;
					pawn.inventory.TryAddItemNotForSale(thing);
				}
				else
				{
					Log.Error("Ingredient stack count lower than needed. This shouldn't be possible to happen. Returning.");
				}
			});
		}
		if (targetC != null && countC > 0)
		{
			yield return Toils_Goto.GotoCell(base.TargetC.Cell, PathEndMode.ClosestTouch);
			yield return Toils_General.Do(delegate
			{
				if (countC <= targetC.stackCount)
				{
					Thing thing2 = targetC.SplitOff(countC);
					thing2.stackCount = countC;
					pawn.inventory.TryAddItemNotForSale(thing2);
				}
				else
				{
					Log.Error("Ingredient stack count lower than needed. This shouldn't be possible to happen. Returning.");
				}
			});
		}
		yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);
		Toil toilWait = Toils_General.WaitWith(TargetIndex.A, natArmor.durabilityProps.RepairTime, useProgressBar: true, maintainPosture: true, maintainSleep: true, TargetIndex.A);
		toilWait.AddFinishAction(delegate
		{
			bool flag = false;
			Thing thing3 = null;
			Thing thing4 = null;
			if (targetB != null && countB > 0)
			{
				thing3 = pawn.inventory.innerContainer.FirstOrDefault((Thing x) => x.def == targetB.def && x.stackCount >= countB);
				if (thing3 == null)
				{
					flag = true;
				}
			}
			if (!flag && targetC != null && countC > 0)
			{
				thing4 = pawn.inventory.innerContainer.FirstOrDefault((Thing x) => x.def == targetC.def && x.stackCount >= countC);
				if (thing4 == null)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				thing3?.SplitOff(countB).Destroy();
				thing4?.SplitOff(countC).Destroy();
				natArmor.curDurability += natArmor.durabilityProps.RepairValue;
				if (natArmor.durabilityProps.CanOverHeal)
				{
					if (natArmor.curDurability > natArmor.durabilityProps.MaxOverHeal + natArmor.maxDurability)
					{
						natArmor.curDurability = natArmor.maxDurability + natArmor.durabilityProps.MaxOverHeal;
					}
					else
					{
						natArmor.curDurability += natArmor.durabilityProps.RepairValue;
					}
				}
				else if (natArmor.curDurability > natArmor.maxDurability)
				{
					natArmor.curDurability = natArmor.maxDurability;
				}
				else
				{
					natArmor.curDurability += natArmor.durabilityProps.RepairValue;
				}
			}
		});
		yield return toilWait;
	}
}
