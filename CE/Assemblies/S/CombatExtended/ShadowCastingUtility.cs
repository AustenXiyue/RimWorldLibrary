using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class ShadowCastingUtility
{
	private struct VisibleRow
	{
		public float startSlope;

		public float endSlope;

		public int visibilityCarry;

		public int depth;

		public int quartor;

		public int maxDepth;

		public static VisibleRow First
		{
			get
			{
				VisibleRow result = default(VisibleRow);
				result.startSlope = -1f;
				result.endSlope = 1f;
				result.depth = 1;
				result.visibilityCarry = 1;
				return result;
			}
		}

		public IEnumerable<Vector3> Tiles()
		{
			int min = (int)Mathf.Floor(startSlope * (float)depth + 0.5f);
			int max = (int)Mathf.Ceil(endSlope * (float)depth - 0.5f);
			for (int i = min; i <= max; i++)
			{
				yield return new Vector3(depth, 0f, i);
			}
		}

		public IntVec3 Transform(IntVec3 oldOffset)
		{
			return _transformationFuncs[quartor](oldOffset);
		}

		public VisibleRow Next()
		{
			VisibleRow result = default(VisibleRow);
			result.endSlope = endSlope;
			result.startSlope = startSlope;
			result.depth = depth + 1;
			result.maxDepth = maxDepth;
			result.quartor = quartor;
			result.visibilityCarry = visibilityCarry;
			return result;
		}
	}

	private const int VISIBILITY_CARRY_MAX = 5;

	private static int carryLimit;

	private static readonly Func<IntVec3, IntVec3>[] _transformationFuncs;

	private static readonly Func<IntVec3, IntVec3>[] _transformationInverseFuncs;

	private static readonly Func<Vector3, Vector3>[] _transformationFuncsV3;

	private static readonly Func<Vector3, Vector3>[] _transformationInverseFuncsV3;

	private static readonly Vector3 InvalidOffset;

	private static int cellsScanned;

	private static IntVec3 source;

	private static List<VisibleRow> rowQueue;

	private static Map map;

	private static Action<IntVec3, int> setAction;

	private static readonly IntVec3 _offsetH;

	private static readonly IntVec3 _offsetV;

	static ShadowCastingUtility()
	{
		carryLimit = 5;
		InvalidOffset = new Vector3(0f, -1f, 0f);
		rowQueue = new List<VisibleRow>();
		_offsetH = new IntVec3(1, 0, 0);
		_offsetV = new IntVec3(0, 0, 1);
		_transformationFuncs = new Func<IntVec3, IntVec3>[4];
		_transformationFuncs[0] = (IntVec3 cell) => cell;
		_transformationFuncs[1] = (IntVec3 cell) => new IntVec3(cell.z, 0, -1 * cell.x);
		_transformationFuncs[2] = (IntVec3 cell) => new IntVec3(-1 * cell.x, 0, -1 * cell.z);
		_transformationFuncs[3] = (IntVec3 cell) => new IntVec3(-1 * cell.z, 0, cell.x);
		_transformationInverseFuncs = new Func<IntVec3, IntVec3>[4];
		_transformationInverseFuncs[0] = (IntVec3 cell) => cell;
		_transformationInverseFuncs[1] = (IntVec3 cell) => new IntVec3(-1 * cell.z, 0, cell.x);
		_transformationInverseFuncs[2] = (IntVec3 cell) => new IntVec3(-1 * cell.x, 0, -1 * cell.z);
		_transformationInverseFuncs[3] = (IntVec3 cell) => new IntVec3(cell.z, 0, -1 * cell.x);
		_transformationFuncsV3 = new Func<Vector3, Vector3>[4];
		_transformationFuncsV3[0] = (Vector3 cell) => cell;
		_transformationFuncsV3[1] = (Vector3 cell) => new Vector3(cell.z, 0f, -1f * cell.x);
		_transformationFuncsV3[2] = (Vector3 cell) => new Vector3(-1f * cell.x, 0f, -1f * cell.z);
		_transformationFuncsV3[3] = (Vector3 cell) => new Vector3(-1f * cell.z, 0f, cell.x);
		_transformationInverseFuncsV3 = new Func<Vector3, Vector3>[4];
		_transformationInverseFuncsV3[0] = (Vector3 cell) => cell;
		_transformationInverseFuncsV3[1] = (Vector3 cell) => new Vector3(-1f * cell.z, 0f, cell.x);
		_transformationInverseFuncsV3[2] = (Vector3 cell) => new Vector3(-1f * cell.x, 0f, -1f * cell.z);
		_transformationInverseFuncsV3[3] = (Vector3 cell) => new Vector3(cell.z, 0f, -1f * cell.x);
	}

	private static bool IsValid(Vector3 point)
	{
		return point.y >= 0f;
	}

	private static float GetSlope(Vector3 point)
	{
		return (2f * point.z - 1f) / (2f * point.x);
	}

	private static int GetQurator(IntVec3 cell)
	{
		int x = cell.x;
		int z = cell.z;
		if (x > 0 && Math.Abs(z) < x)
		{
			return 0;
		}
		if (x < 0 && Math.Abs(z) < Math.Abs(x))
		{
			return 2;
		}
		if (z > 0 && Math.Abs(x) <= z)
		{
			return 3;
		}
		return 1;
	}

	private static void TryWeightedScan(VisibleRow start)
	{
		rowQueue.Clear();
		rowQueue.Add(start);
		while (rowQueue.Count > 0)
		{
			VisibleRow visibleRow = rowQueue.Pop();
			if (visibleRow.depth > visibleRow.maxDepth || (float)visibleRow.visibilityCarry <= 1E-05f)
			{
				continue;
			}
			Vector3 point = InvalidOffset;
			bool flag = false;
			bool flag2 = false;
			foreach (Vector3 item2 in visibleRow.Tiles())
			{
				IntVec3 intVec = source + visibleRow.Transform(item2.ToIntVec3());
				bool flag3 = !intVec.InBounds(map) || !intVec.CanBeSeenOver(map);
				int num;
				if (!flag3)
				{
					Thing cover = intVec.GetCover(map);
					num = ((cover != null && cover.def.Fillage == FillCategory.Partial) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
				bool flag4 = (byte)num != 0;
				if (flag3 || (item2.z >= (float)visibleRow.depth * visibleRow.startSlope && item2.z <= (float)visibleRow.depth * visibleRow.endSlope))
				{
					setAction(intVec, visibleRow.visibilityCarry);
				}
				if (flag4 && visibleRow.visibilityCarry >= carryLimit)
				{
					flag4 = false;
					flag3 = true;
				}
				if (IsValid(point))
				{
					if (flag == flag3)
					{
						if (flag4 != flag2 && flag == flag3)
						{
							VisibleRow item = visibleRow.Next();
							item.endSlope = GetSlope(item2);
							if (flag2)
							{
								item.visibilityCarry++;
							}
							rowQueue.Add(item);
							visibleRow.startSlope = GetSlope(item2);
						}
					}
					else if (!flag3 && flag)
					{
						visibleRow.startSlope = GetSlope(item2);
					}
					else if (flag3 && !flag)
					{
						VisibleRow item = visibleRow.Next();
						item.endSlope = GetSlope(item2);
						if (flag2)
						{
							item.visibilityCarry++;
						}
						rowQueue.Add(item);
					}
				}
				cellsScanned++;
				point = item2;
				flag = flag3;
				flag2 = flag4;
			}
			if (point.y >= 0f && !flag)
			{
				VisibleRow item = visibleRow.Next();
				if (flag2)
				{
					item.visibilityCarry++;
				}
				rowQueue.Add(item);
			}
		}
	}

	private static void TryVisibilityScan(VisibleRow start)
	{
		rowQueue.Clear();
		rowQueue.Add(start);
		while (rowQueue.Count > 0)
		{
			VisibleRow visibleRow = rowQueue.Pop();
			if (visibleRow.depth > visibleRow.maxDepth)
			{
				continue;
			}
			Vector3 point = InvalidOffset;
			bool flag = false;
			foreach (Vector3 item2 in visibleRow.Tiles())
			{
				IntVec3 intVec = source + visibleRow.Transform(item2.ToIntVec3());
				bool flag2 = !intVec.InBounds(map) || !intVec.CanBeSeenOverFast(map);
				if (flag2 || (item2.z >= (float)visibleRow.depth * visibleRow.startSlope && item2.z <= (float)visibleRow.depth * visibleRow.endSlope))
				{
					setAction(intVec, 1);
				}
				if (IsValid(point))
				{
					if (!flag2 && flag)
					{
						visibleRow.startSlope = GetSlope(item2);
					}
					else if (flag2 && !flag)
					{
						VisibleRow item = visibleRow.Next();
						item.endSlope = GetSlope(item2);
						rowQueue.Add(item);
					}
				}
				cellsScanned++;
				point = item2;
				flag = flag2;
			}
			if (point.y >= 0f && !flag)
			{
				rowQueue.Add(visibleRow.Next());
			}
		}
	}

	private static void TryCastVisibility(float startSlope, float endSlope, int quartor, int maxDepth)
	{
		TryCast(TryVisibilityScan, startSlope, endSlope, quartor, maxDepth);
	}

	private static void TryCastWeighted(float startSlope, float endSlope, int quartor, int maxDepth)
	{
		TryCast(TryWeightedScan, startSlope, endSlope, quartor, maxDepth);
	}

	private static void TryCast(Action<VisibleRow> castAction, float startSlope, float endSlope, int quartor, int maxDepth)
	{
		if (endSlope > 1f || startSlope < -1f || startSlope > endSlope)
		{
			throw new InvalidOperationException($"CE: Scan quartor {quartor} endSlope and start slop must be between (-1, 1) but got start:{startSlope}\tend:{endSlope}");
		}
		VisibleRow first = VisibleRow.First;
		first.startSlope = startSlope;
		first.endSlope = endSlope;
		first.visibilityCarry = 1;
		first.maxDepth = maxDepth;
		first.quartor = quartor;
		castAction(first);
	}

	private static IntVec3 GetBaseOffset(int quartor)
	{
		return (quartor == 0 || quartor == 2) ? _offsetV : _offsetH;
	}

	private static int GetNextQuartor(int quartor)
	{
		return (quartor + 1) % 4;
	}

	private static int GetPreviousQuartor(int quartor)
	{
		return (quartor - 1 < 0) ? 3 : (quartor - 1);
	}

	public static void CastWeighted(Map map, IntVec3 source, Action<IntVec3, int> setAction, int radius)
	{
		Cast(map, TryCastWeighted, setAction, source, radius);
	}

	public static void CastWeighted(Map map, IntVec3 source, Action<IntVec3, int> action, int radius, int carryLimit, out int cellCount)
	{
		ShadowCastingUtility.carryLimit = carryLimit;
		CastWeighted(map, source, action, radius);
		cellCount = cellsScanned;
		ShadowCastingUtility.carryLimit = 5;
	}

	public static void CastVisibility(Map map, IntVec3 source, Action<IntVec3> action, int radius)
	{
		Cast(map, TryCastVisibility, delegate(IntVec3 cell, int _)
		{
			action(cell);
		}, source, radius);
	}

	public static void CastVisibility(Map map, IntVec3 source, Action<IntVec3> action, int radius, out int cellCount)
	{
		CastVisibility(map, source, action, radius);
		cellCount = cellsScanned;
	}

	private static void Cast(Map map, Action<float, float, int, int> castingAction, Action<IntVec3, int> action, IntVec3 source, int radius)
	{
		ShadowCastingUtility.map = map;
		ShadowCastingUtility.source = source;
		setAction = action;
		cellsScanned = 0;
		int arg = (int)((float)radius * 1.43f);
		for (int i = 0; i < 4; i++)
		{
			castingAction(-1f, 1f, i, arg);
		}
		ShadowCastingUtility.map = null;
		ShadowCastingUtility.source = IntVec3.Invalid;
	}

	public static void CastVisibility(Map map, IntVec3 source, Vector3 direction, Action<IntVec3> action, float radius, float baseWidth)
	{
		Cast(map, TryCastVisibility, delegate(IntVec3 cell, int _)
		{
			action(cell);
		}, source, (source.ToVector3() + direction.normalized * radius).ToIntVec3(), baseWidth);
	}

	public static void CastVisibility(Map map, IntVec3 source, Vector3 direction, Action<IntVec3> action, float radius, float baseWidth, out int cellCount)
	{
		CastVisibility(map, source, direction, action, radius, baseWidth);
		cellCount = cellsScanned;
	}

	public static void CastWeighted(Map map, IntVec3 source, Vector3 direction, Action<IntVec3, int> action, float radius, float baseWidth)
	{
		Cast(map, TryCastWeighted, action, source, (source.ToVector3() + direction.normalized * radius).ToIntVec3(), baseWidth);
	}

	public static void CastWeighted(Map map, IntVec3 source, Vector3 direction, Action<IntVec3, int> action, float radius, float baseWidth, int carryLimit, out int cellCount)
	{
		ShadowCastingUtility.carryLimit = carryLimit;
		CastWeighted(map, source, direction, action, radius, baseWidth);
		cellCount = cellsScanned;
		ShadowCastingUtility.carryLimit = 5;
	}

	private static void Cast(Map map, Action<float, float, int, int> castingAction, Action<IntVec3, int> setAction, IntVec3 source, IntVec3 target, float baseWidth)
	{
		if (!(target.DistanceTo(source) < 2f))
		{
			ShadowCastingUtility.map = map;
			ShadowCastingUtility.source = source;
			ShadowCastingUtility.setAction = setAction;
			cellsScanned = 0;
			int qurator = GetQurator(target - source);
			Vector3 vector = _transformationInverseFuncsV3[qurator]((target - source).ToVector3());
			Vector3 point = vector + new Vector3(0f, 0f, (0f - baseWidth) / 2f);
			Vector3 point2 = vector + new Vector3(0f, 0f, baseWidth / 2f);
			int arg = (int)source.DistanceTo(target);
			float num = GetSlope(point);
			float num2 = GetSlope(point2);
			if (num < -1f)
			{
				float arg2 = Mathf.Max(num + 2f, 0f);
				castingAction(arg2, 1f, GetNextQuartor(qurator), arg);
				num = -1f;
			}
			if (num2 > 1f)
			{
				float arg3 = Mathf.Min(num2 - 2f, 0f);
				castingAction(-1f, arg3, GetPreviousQuartor(qurator), arg);
				num2 = 1f;
			}
			castingAction(num, num2, qurator, arg);
			ShadowCastingUtility.map = null;
			ShadowCastingUtility.source = IntVec3.Invalid;
		}
	}
}
