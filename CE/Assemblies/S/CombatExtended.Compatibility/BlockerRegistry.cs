using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended.Compatibility;

public static class BlockerRegistry
{
	private static bool enabledCB;

	private static bool enabledCFC;

	private static bool enabledIS;

	private static bool enabledBCW;

	private static bool enabledPUF;

	private static bool enabledSZ;

	private static List<Func<ProjectileCE, Vector3, Vector3, bool>> checkForCollisionBetweenCallbacks;

	private static List<Func<ProjectileCE, IntVec3, Thing, bool>> checkCellForCollisionCallbacks;

	private static List<Func<ProjectileCE, Thing, bool>> impactSomethingCallbacks;

	private static List<Func<ProjectileCE, Thing, bool>> beforeCollideWithCallbacks;

	private static List<Func<Pawn, IntVec3, bool>> pawnUnsuppressableFromCallback;

	private static List<Func<Thing, IEnumerable<IEnumerable<IntVec3>>>> shieldZonesCallback;

	private static void EnableCB()
	{
		enabledCB = true;
		checkForCollisionBetweenCallbacks = new List<Func<ProjectileCE, Vector3, Vector3, bool>>();
	}

	private static void EnableIS()
	{
		enabledIS = true;
		impactSomethingCallbacks = new List<Func<ProjectileCE, Thing, bool>>();
	}

	private static void EnableCFC()
	{
		enabledCFC = true;
		checkCellForCollisionCallbacks = new List<Func<ProjectileCE, IntVec3, Thing, bool>>();
	}

	private static void EnableSZ()
	{
		enabledSZ = true;
		shieldZonesCallback = new List<Func<Thing, IEnumerable<IEnumerable<IntVec3>>>>();
	}

	private static void EnablePUF()
	{
		enabledPUF = true;
		pawnUnsuppressableFromCallback = new List<Func<Pawn, IntVec3, bool>>();
	}

	private static void EnableBCW()
	{
		enabledBCW = true;
		beforeCollideWithCallbacks = new List<Func<ProjectileCE, Thing, bool>>();
	}

	public static void RegisterCheckForCollisionBetweenCallback(Func<ProjectileCE, Vector3, Vector3, bool> f)
	{
		if (!enabledCB)
		{
			EnableCB();
		}
		checkForCollisionBetweenCallbacks.Add(f);
	}

	public static void RegisterCheckForCollisionCallback(Func<ProjectileCE, IntVec3, Thing, bool> f)
	{
		if (!enabledCFC)
		{
			EnableCFC();
		}
		checkCellForCollisionCallbacks.Add(f);
	}

	public static void RegisterImpactSomethingCallback(Func<ProjectileCE, Thing, bool> f)
	{
		if (!enabledIS)
		{
			EnableIS();
		}
		impactSomethingCallbacks.Add(f);
	}

	public static void RegisterBeforeCollideWithCallback(Func<ProjectileCE, Thing, bool> f)
	{
		if (!enabledBCW)
		{
			EnableBCW();
		}
		beforeCollideWithCallbacks.Add(f);
	}

	public static bool CheckForCollisionBetweenCallback(ProjectileCE projectile, Vector3 from, Vector3 to)
	{
		if (!enabledCB)
		{
			return false;
		}
		foreach (Func<ProjectileCE, Vector3, Vector3, bool> checkForCollisionBetweenCallback in checkForCollisionBetweenCallbacks)
		{
			if (checkForCollisionBetweenCallback(projectile, from, to))
			{
				return true;
			}
		}
		return false;
	}

	public static void RegisterShieldZonesCallback(Func<Thing, IEnumerable<IEnumerable<IntVec3>>> f)
	{
		if (!enabledSZ)
		{
			EnableSZ();
		}
		shieldZonesCallback.Add(f);
	}

	public static void RegisterUnsuppresableFromCallback(Func<Pawn, IntVec3, bool> f)
	{
		if (!enabledPUF)
		{
			EnablePUF();
		}
		pawnUnsuppressableFromCallback.Add(f);
	}

	public static bool CheckCellForCollisionCallback(ProjectileCE projectile, IntVec3 cell, Thing launcher)
	{
		if (!enabledCFC)
		{
			return false;
		}
		foreach (Func<ProjectileCE, IntVec3, Thing, bool> checkCellForCollisionCallback in checkCellForCollisionCallbacks)
		{
			if (checkCellForCollisionCallback(projectile, cell, launcher))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ImpactSomethingCallback(ProjectileCE projectile, Thing launcher)
	{
		if (!enabledIS)
		{
			return false;
		}
		foreach (Func<ProjectileCE, Thing, bool> impactSomethingCallback in impactSomethingCallbacks)
		{
			if (impactSomethingCallback(projectile, launcher))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing thing)
	{
		if (!enabledSZ)
		{
			yield break;
		}
		foreach (Func<Thing, IEnumerable<IEnumerable<IntVec3>>> callback in shieldZonesCallback)
		{
			foreach (IEnumerable<IntVec3> item in callback(thing))
			{
				yield return item;
			}
		}
	}

	public static bool PawnUnsuppressableFromCallback(Pawn pawn, IntVec3 origin)
	{
		if (!enabledPUF)
		{
			return false;
		}
		foreach (Func<Pawn, IntVec3, bool> item in pawnUnsuppressableFromCallback)
		{
			if (item(pawn, origin))
			{
				return true;
			}
		}
		return false;
	}

	public static bool BeforeCollideWithCallback(ProjectileCE projectile, Thing collideWith)
	{
		if (!enabledBCW)
		{
			return false;
		}
		foreach (Func<ProjectileCE, Thing, bool> beforeCollideWithCallback in beforeCollideWithCallbacks)
		{
			if (beforeCollideWithCallback(projectile, collideWith))
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 GetExactPosition(Vector3 origin, Vector3 curPosition, Vector3 shieldPosition, float radiusSq)
	{
		Vector3 vector = curPosition - origin;
		double num = vector.sqrMagnitude;
		double num2 = 2f * (vector.x * (origin.x - shieldPosition.x) + vector.z * (origin.z - shieldPosition.z));
		double num3 = (shieldPosition - origin).sqrMagnitude - radiusSq;
		double num4 = num2 * num2 - 4.0 * num * num3;
		if (num4 < 0.0)
		{
			return curPosition;
		}
		float num5 = (float)(2.0 * num3 / (0.0 - num2 + Math.Sqrt(num4)));
		return vector * num5 + origin;
	}
}
