using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class CompAmmoGiver : ThingComp
{
	public int ammoAmountToGive;

	public Pawn dad => parent as Pawn;

	public CompAmmoUser user => dad.equipment.Primary?.TryGetComp<CompAmmoUser>() ?? null;

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (user == null || selPawn == dad || selPawn.Faction.HostileTo(dad.Faction) || !selPawn.CanReach(dad, PathEndMode.ClosestTouch, Danger.Deadly) || selPawn.Downed || !selPawn.inventory.innerContainer.Where((Thing x) => x is AmmoThing).Any((Thing x) => ((AmmoDef)x.def).AmmoSetDefs.Contains(user.Props.ammoSet)))
		{
			yield break;
		}
		yield return new FloatMenuOption("CE_GiveAmmoToThing".Translate(dad.Name?.ToStringShort ?? dad.def.label), delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (AmmoThing ammo in selPawn.TryGetComp<CompInventory>().ammoList)
			{
				if (ammo.AmmoDef.AmmoSetDefs.Contains(user.Props.ammoSet))
				{
					int outAmmoCount = 0;
					CompInventory compInventory = dad.TryGetComp<CompInventory>();
					if (compInventory != null && compInventory.CanFitInInventory(ammo, out outAmmoCount) && outAmmoCount >= ammo.stackCount)
					{
						list.Add(new FloatMenuOption("CE_Give".Translate() + " " + ammo.Label + " (" + "All".Translate() + ")", delegate
						{
							GiveAmmo(selPawn, ammo, ammo.stackCount);
						}));
					}
					if (outAmmoCount > 0)
					{
						list.Add(new FloatMenuOption("CE_Give".Translate() + " " + ammo.def.label + "...", delegate
						{
							Find.WindowStack.Add(new Window_GiveAmmoAmountSlider
							{
								dad = dad,
								sourceAmmo = ammo,
								selPawn = selPawn,
								sourceComp = this,
								maxAmmoCount = outAmmoCount
							});
						}));
					}
					else
					{
						list.Add(new FloatMenuOption("CE_TargetInventoryFull".Translate(), null));
					}
				}
			}
			if (!list.Any())
			{
				list.Add(new FloatMenuOption("CE_NoAmmoToGive".Translate(), null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		});
	}

	[Multiplayer.SyncMethod]
	public void GiveAmmo(Pawn selPawn, Thing ammo, int amount)
	{
		ammoAmountToGive = amount;
		JobDef giveAmmo = CE_JobDefOf.GiveAmmo;
		Job newJob = new Job
		{
			def = giveAmmo,
			targetA = dad,
			targetB = ammo
		};
		selPawn.jobs.StartJob(newJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
	}
}
