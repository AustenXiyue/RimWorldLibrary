using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobGiver_ModifyCarriedWeapon : ThinkNode_JobGiver
{
	private const int THROTTLE_COOLDOWN = 450;

	private static Dictionary<int, int> _throttle;

	static JobGiver_ModifyCarriedWeapon()
	{
		_throttle = new Dictionary<int, int>();
		CacheClearComponent.AddClearCacheAction(delegate
		{
			_throttle.Clear();
		});
	}

	public override float GetPriority(Pawn pawn)
	{
		if (_throttle.TryGetValue(pawn.thingIDNumber, out var value) && GenTicks.TicksGame - value < 450)
		{
			return -1f;
		}
		_throttle[pawn.thingIDNumber] = GenTicks.TicksGame;
		if (!pawn.RaceProps.Humanlike || (!pawn.IsColonist && !pawn.IsSlaveOfColony) || pawn?.equipment?.Primary == null)
		{
			return -1f;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Crafting))
		{
			return -1f;
		}
		Pawn_HealthTracker health = pawn.health;
		if (health == null || !health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return -1f;
		}
		if (pawn?.equipment?.Primary is WeaponPlatform weaponPlatform)
		{
			weaponPlatform.TrySyncPlatformLoadout(pawn);
			if (!weaponPlatform.ConfigApplied)
			{
				return 20f + (float)(weaponPlatform.AdditionList?.Count ?? 0) * 8f + (float)(weaponPlatform.RemovalList?.Count ?? 0) * 8f;
			}
		}
		return -1f;
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.RaceProps.Humanlike || (!pawn.IsColonist && !pawn.IsSlaveOfColony) || pawn?.equipment?.Primary == null)
		{
			return null;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Crafting))
		{
			return null;
		}
		Pawn_HealthTracker health = pawn.health;
		if (health == null || !health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return null;
		}
		if (pawn.equipment?.Primary is WeaponPlatform { ConfigApplied: false } weaponPlatform)
		{
			return WorkGiver_ModifyWeapon.TryGetModifyWeaponJob(pawn, weaponPlatform);
		}
		return null;
	}
}
