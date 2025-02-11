using System.Collections.Generic;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace CombatExtended;

public class CompAmmoResupplyOnWakeup : ThingComp
{
	private const float MaxResupplyRadius = 40f;

	private const float AmmoSearchRadius = 40f;

	private const int ticksBetweenChecks = 600;

	private Lord lord;

	public CompProperties_AmmoResupplyOnWakeup Props => (CompProperties_AmmoResupplyOnWakeup)props;

	public bool IsActive
	{
		get
		{
			if (!Controller.settings.EnableAmmoSystem)
			{
				return false;
			}
			CompCanBeDormant compCanBeDormant = parent.TryGetComp<CompCanBeDormant>();
			if (compCanBeDormant != null && !compCanBeDormant.Awake)
			{
				return false;
			}
			return lord?.AnyActivePawn ?? false;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		Faction faction = parent.Faction;
		foreach (Lord lord in parent.Map.lordManager.lords)
		{
			if (lord.faction == faction && lord.LordJob is LordJob_MechanoidDefendBase { isMechCluster: not false } lordJob_MechanoidDefendBase && lordJob_MechanoidDefendBase.things.Contains(parent))
			{
				this.lord = lord;
				break;
			}
		}
	}

	public bool EnoughAmmoAround(Building_TurretGunCE turret)
	{
		if (turret.GetReloading() || !turret.ShouldReload())
		{
			return true;
		}
		CompAmmoUser compAmmo = turret.CompAmmo;
		int num = GenRadial.NumCellsInRadius(40f);
		int num2 = compAmmo.CurMagCount;
		int num3 = Mathf.CeilToInt((float)compAmmo.MagSize * 0.5f);
		Map map = parent.Map;
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = parent.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> list = map.thingGrid.ThingsListAtFast(c);
			for (int j = 0; j < list.Count; j++)
			{
				if (compAmmo.CurrentAmmo == list[j].def)
				{
					num2 += list[j].stackCount;
					if (num2 > num3)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(600))
		{
			TickRareWorker();
		}
	}

	public void TickRareWorker()
	{
		if (!IsActive)
		{
			return;
		}
		TurretTracker component = parent.Map.GetComponent<TurretTracker>();
		Faction faction = parent.Faction;
		IntVec3 position = parent.Position;
		foreach (Building_Turret turret in component.Turrets)
		{
			if (turret is Building_TurretGunCE building_TurretGunCE && building_TurretGunCE.Faction == faction && !building_TurretGunCE.def.building.IsMortar)
			{
				CompAmmoUser compAmmo = building_TurretGunCE.CompAmmo;
				if (compAmmo != null && compAmmo.UseAmmo && building_TurretGunCE.Position.InHorDistOf(position, 40f) && !EnoughAmmoAround(building_TurretGunCE) && building_TurretGunCE.CompAmmo.CurrentAmmo != null)
				{
					DropSupplies(building_TurretGunCE.CompAmmo.CurrentAmmo, Mathf.CeilToInt(0.5f * (float)building_TurretGunCE.CompAmmo.MagSize), building_TurretGunCE.Position);
				}
			}
		}
	}

	private void DropSupplies(ThingDef thingDef, int count, IntVec3 cell)
	{
		List<Thing> list = new List<Thing>();
		Thing thing = ThingMaker.MakeThing(thingDef);
		thing.stackCount = count;
		list.Add(thing);
		DropPodUtility.DropThingsNear(cell, parent.Map, list, 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: true, allowFogged: true, parent.Faction);
	}
}
