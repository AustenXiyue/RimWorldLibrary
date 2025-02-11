using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class GenSightCE
{
	private static List<IntVec3> points = new List<IntVec3>();

	public static IEnumerable<IntVec3> PointsOnLineOfSight(Vector3 startPos, Vector3 endPos)
	{
		if (startPos.x == endPos.x || startPos.z == endPos.z)
		{
			foreach (IntVec3 item in PointsOnStraightLineOfSight(startPos, endPos))
			{
				yield return item;
			}
			yield break;
		}
		Vector3 losRay = endPos - startPos;
		Vector3 currentPos = new Vector3(startPos.x, startPos.y, startPos.z);
		int stepX = ((losRay.x >= 0f) ? 1 : (-1));
		int stepZ = ((losRay.z >= 0f) ? 1 : (-1));
		float xzRatio = losRay.x / losRay.z;
		yield return startPos.ToIntVec3();
		while (IsPositionWithinVector(currentPos, endPos, stepX, stepZ))
		{
			float nextXPointDistance = stepX;
			float currentPosXDecimalPortion = currentPos.x % 1f;
			if (!NearlyEqual(currentPosXDecimalPortion, 0f) && !NearlyEqual(Mathf.Abs(currentPosXDecimalPortion), 1f))
			{
				nextXPointDistance = ((currentPosXDecimalPortion >= 0.5f) ? (1f - currentPosXDecimalPortion) : currentPosXDecimalPortion) * (float)stepX;
			}
			Vector3 nextXCell = new Vector3(currentPos.x + nextXPointDistance, currentPos.y, currentPos.z + nextXPointDistance / xzRatio);
			float nextZPointDistance = stepZ;
			float currentPosZDecimalPortion = currentPos.z % 1f;
			if (!NearlyEqual(currentPosZDecimalPortion, 0f) && !NearlyEqual(Mathf.Abs(currentPosZDecimalPortion), 1f))
			{
				nextZPointDistance = ((currentPosZDecimalPortion >= 0.5f) ? (1f - currentPosZDecimalPortion) : currentPosZDecimalPortion) * (float)stepZ;
			}
			Vector3 nextZCell = new Vector3(currentPos.x + nextZPointDistance * xzRatio, currentPos.y, currentPos.z + nextZPointDistance);
			currentPos = ((GetDistanceSqr(currentPos, nextXCell) < GetDistanceSqr(currentPos, nextZCell)) ? nextXCell : nextZCell);
			if (IsPositionWithinVector(currentPos, endPos, stepX, stepZ))
			{
				yield return new IntVec3((int)(currentPos.x + 0.0001f * (float)stepX), (int)currentPos.y, (int)(currentPos.z + 0.0001f * (float)stepZ));
				if (NearlyEqual(currentPos.x, Mathf.RoundToInt(currentPos.x)) && NearlyEqual(currentPos.z, Mathf.RoundToInt(currentPos.z)))
				{
					yield return new IntVec3((int)(currentPos.x + 0.0001f * (float)stepX - (float)stepX), (int)currentPos.y, (int)(currentPos.z + 0.0001f * (float)stepZ));
					yield return new IntVec3((int)(currentPos.x + 0.0001f * (float)stepX), (int)currentPos.y, (int)(currentPos.z + 0.0001f * (float)stepZ - (float)stepZ));
				}
			}
		}
	}

	public static List<IntVec3> AllPointsOnLineOfSight(IntVec3 startPos, IntVec3 dest)
	{
		points.Clear();
		points.AddRange(GenSight.PointsOnLineOfSight(startPos, dest));
		return points;
	}

	public static IEnumerable<IntVec3> PartialLineOfSights(this Pawn pawn, LocalTargetInfo targetFacing)
	{
		return pawn.Map.PartialLineOfSights(pawn, targetFacing);
	}

	public static IEnumerable<IntVec3> PartialLineOfSights(this Map map, LocalTargetInfo source, LocalTargetInfo targetFacing)
	{
		IntVec3 startPos = source.Cell;
		IntVec3 endPos = targetFacing.Cell;
		foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(startPos, new IntVec3((int)((float)(startPos.x * 3 + endPos.x) / 4f), (int)((float)(startPos.y * 3 + endPos.y) / 4f), (int)((float)(startPos.z * 3 + endPos.z) / 4f))))
		{
			Thing cover = cell.GetCover(map);
			if (cover == null || cover.def.Fillage != FillCategory.Full)
			{
				yield return cell;
				continue;
			}
			yield break;
		}
	}

	private static IEnumerable<IntVec3> PointsOnStraightLineOfSight(Vector3 startPos, Vector3 endPos)
	{
		IntVec3 currentCell = startPos.ToIntVec3();
		if (startPos.x == endPos.x)
		{
			int stepZ = ((endPos.z - startPos.z >= 0f) ? 1 : (-1));
			for (float currentZ = startPos.z; (stepZ > 0) ? (currentZ <= endPos.z) : (currentZ >= endPos.z); currentZ += (float)stepZ)
			{
				currentCell.z = (int)currentZ;
				yield return currentCell;
			}
		}
		else if (startPos.z == endPos.z)
		{
			int stepX = ((endPos.x - startPos.x >= 0f) ? 1 : (-1));
			for (float currentX = startPos.x; (stepX > 0) ? (currentX <= endPos.x) : (currentX >= endPos.x); currentX += (float)stepX)
			{
				currentCell.x = (int)currentX;
				yield return currentCell;
			}
		}
		else
		{
			Log.Error("[CE] GenSightCE.PointsOnStraightLineOfSight was given a diagonal vector, should use GenSightCE.PointsOnLineOfSight instead.");
		}
	}

	private static bool IsPositionWithinVector(Vector3 currentPos, Vector3 endPos, int stepX, int stepZ)
	{
		if ((stepX > 0 && currentPos.x >= endPos.x) || (stepX < 0 && currentPos.x <= endPos.x))
		{
			return false;
		}
		if ((stepZ > 0 && currentPos.z >= endPos.z) || (stepZ < 0 && currentPos.z <= endPos.z))
		{
			return false;
		}
		return true;
	}

	private static double GetDistanceSqr(Vector3 v1, Vector3 v2)
	{
		return Math.Pow(v2.x - v1.x, 2.0) + Math.Pow(v2.z - v1.z, 2.0);
	}

	private static bool NearlyEqual(float a, float b, float tolerance = 0.0001f)
	{
		if (a == b)
		{
			return true;
		}
		return Math.Abs(a - b) < tolerance;
	}
}
