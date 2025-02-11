using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.AI;

public static class AI_Utility
{
	private static readonly Dictionary<Pawn, CompTacticalManager> _compTactical = new Dictionary<Pawn, CompTacticalManager>(2048);

	public static CompTacticalManager GetTacticalManager(this Pawn pawn)
	{
		if (_compTactical.TryGetValue(pawn, out var value))
		{
			return value;
		}
		value = pawn.TryGetComp<CompTacticalManager>();
		if (value != null)
		{
			_compTactical[pawn] = value;
		}
		return value;
	}

	public static bool HiddingBehindCover(this Pawn pawn, LocalTargetInfo targetFacing)
	{
		Map map = pawn.Map;
		IntVec3 position = pawn.Position;
		IntVec3 cell = targetFacing.Cell;
		foreach (IntVec3 item in GenSight.PointsOnLineOfSight(position, new IntVec3((int)((float)(position.x * 3 + cell.x) / 4f), (int)((float)(position.y * 3 + cell.y) / 4f), (int)((float)(position.z * 3 + cell.z) / 4f))))
		{
			Thing cover = item.GetCover(map);
			if (cover != null && cover.def.Fillage == FillCategory.Partial)
			{
				return true;
			}
		}
		return false;
	}

	public static float CoverScoreFor(this Map map, IntVec3 startPos, IntVec3 endPos)
	{
		float num = endPos.x - startPos.x;
		float num2 = endPos.z - startPos.z;
		float num3 = Mathf.Max(Mathf.Abs(num), Mathf.Abs(num2));
		float num4 = 0f;
		if (num3 == 0f)
		{
			return 0f;
		}
		num /= num3;
		num2 /= num3;
		for (int i = 1; i < 4 && (float)startPos.x + num * (float)i <= (float)endPos.x && (float)startPos.z + num2 * (float)i <= (float)endPos.z; i++)
		{
			Thing cover = (startPos + new IntVec3((int)(num * (float)i), 0, (int)(num2 * (float)i))).GetCover(map);
			if (cover != null)
			{
				if (cover.def.Fillage == FillCategory.Partial)
				{
					num4 += 0.99999f;
				}
				else if (cover.def.Fillage == FillCategory.Full)
				{
					num4 += 0.33333f;
				}
			}
			map.debugDrawer.FlashCell(startPos + new IntVec3((int)(num * (float)i), 0, (int)(num2 * (float)i)), 1f);
		}
		return num4;
	}

	public static void TrySetFireMode(this CompFireModes fireModes, FireMode mode)
	{
		if (fireModes.CurrentFireMode == mode || fireModes.AvailableFireModes.Count == 1)
		{
			return;
		}
		int num = (int)mode;
		int num2 = 0;
		while (num2 < 3)
		{
			mode = (FireMode)((num + num2) % 3);
			num2++;
			if (fireModes.AvailableFireModes.Contains(mode))
			{
				fireModes.CurrentFireMode = mode;
				break;
			}
		}
	}

	public static void TrySetAimMode(this CompFireModes fireModes, AimMode mode)
	{
		if (fireModes.CurrentAimMode == mode || fireModes.AvailableAimModes.Count == 1)
		{
			return;
		}
		int num = (int)mode;
		int num2 = 0;
		while (num2 < 3)
		{
			mode = (AimMode)((num + num2) % 3);
			num2++;
			if (fireModes.AvailableAimModes.Contains(mode))
			{
				fireModes.CurrentAimMode = mode;
				break;
			}
		}
	}

	public static bool EdgingCloser(this Pawn pawn, Pawn other)
	{
		float num = other.Position.DistanceTo(pawn.Position);
		if (other.pather.moving && num > other.pather.destination.Cell.DistanceTo(pawn.Position))
		{
			return true;
		}
		if (pawn.pather.moving && num > pawn.pather.destination.Cell.DistanceTo(other.Position))
		{
			return true;
		}
		return false;
	}

	public static bool EdgingAway(this Pawn pawn, Pawn other)
	{
		float num = other.Position.DistanceTo(pawn.Position);
		if (other.pather.moving && num < other.pather.destination.Cell.DistanceTo(pawn.Position))
		{
			return true;
		}
		if (pawn.pather.moving && num < pawn.pather.destination.Cell.DistanceTo(other.Position))
		{
			return true;
		}
		return false;
	}

	public static bool VisibilityGoodAt(this Map map, Pawn shooter, IntVec3 target, float nightVisionEfficiency = -1f)
	{
		LightingTracker lightingTracker = map.GetLightingTracker();
		if (!lightingTracker.IsNight)
		{
			return true;
		}
		if (target.DistanceTo(shooter.Position) < 15f)
		{
			return true;
		}
		if (lightingTracker.CombatGlowAtFor(shooter.Position, target) >= 0.5f)
		{
			return true;
		}
		if ((double)((nightVisionEfficiency == -1f) ? shooter.GetStatValue(CE_StatDefOf.NightVisionEfficiency) : nightVisionEfficiency) > 0.6)
		{
			return true;
		}
		if (target.Roofed(map))
		{
			return true;
		}
		return false;
	}

	public static IntVec3 FindAttackedClusterCenter(Pawn attacker, IntVec3 targetPos, float verbRange, float radius, Func<IntVec3, bool> predicate = null)
	{
		IntVec3 position = attacker.Position;
		IntVec3 result = targetPos;
		foreach (IntVec3 item in from p in targetPos.HostilesInRange(attacker.Map, attacker.Faction, radius * 1.8f)
			select p.Position)
		{
			IntVec3 intVec = new IntVec3((int)(((float)result.x + (float)item.x) / 2f), (int)(((float)result.y + (float)item.y) / 2f), (int)(((float)result.z + (float)item.z) / 2f));
			if (verbRange >= intVec.DistanceTo(position) && predicate != null && predicate(intVec))
			{
				result = intVec;
			}
		}
		return result;
	}
}
