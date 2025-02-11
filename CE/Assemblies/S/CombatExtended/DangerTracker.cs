using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class DangerTracker : MapComponent
{
	private const int DANGER_TICKS_SMOKE_STEP = 150;

	private const float PROJECTILE_DANGER_FACTOR = 0.5f;

	public const int DANGER_TICKS_MAX = 300;

	private const float WEIGHTS_DIG = 0.8f;

	private const float WEIGHTS_COL = 0.5f;

	private const float WEIGHTSSUM = 6.2f;

	private static readonly IntVec3[] AdjCells;

	private static readonly float[] AdjWeights;

	private int[] dangerArray;

	static DangerTracker()
	{
		AdjCells = new IntVec3[9];
		AdjWeights = new float[9];
		AdjCells[0] = new IntVec3(0, 0, 0);
		AdjWeights[0] = 1f;
		AdjCells[1] = new IntVec3(1, 0, 1);
		AdjWeights[1] = 0.5f;
		AdjCells[2] = new IntVec3(-1, 0, 1);
		AdjWeights[2] = 0.5f;
		AdjCells[3] = new IntVec3(1, 0, -1);
		AdjWeights[3] = 0.5f;
		AdjCells[4] = new IntVec3(-1, 0, -1);
		AdjWeights[4] = 0.5f;
		AdjCells[5] = new IntVec3(1, 0, 0);
		AdjWeights[5] = 0.8f;
		AdjCells[6] = new IntVec3(-1, 0, 0);
		AdjWeights[6] = 0.8f;
		AdjCells[7] = new IntVec3(0, 0, 1);
		AdjWeights[7] = 0.8f;
		AdjCells[8] = new IntVec3(0, 0, -1);
		AdjWeights[8] = 0.8f;
	}

	public DangerTracker(Map map)
		: base(map)
	{
		dangerArray = new int[map.cellIndices.NumGridCells];
	}

	public void Notify_BulletAt(IntVec3 pos, float dangerAmount)
	{
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = pos + AdjCells[i];
			if (intVec.InBounds(map))
			{
				IncreaseAt(intVec, (int)Mathf.Ceil(AdjWeights[i] * dangerAmount * 0.5f));
			}
		}
		if (Controller.settings.DebugDisplayDangerBuildup)
		{
			FlashCells(pos);
		}
	}

	public void Notify_DangerRadiusAt(IntVec3 pos, float radius, float dangerAmount, bool reduceOverDistance = true)
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(pos, radius, useCenter: true))
		{
			if (!item.InBounds(map) && !GenSight.LineOfSight(pos, item, map))
			{
				continue;
			}
			if (reduceOverDistance)
			{
				dangerAmount *= Mathf.Clamp01(1f - (pos.ToVector3() - item.ToVector3()).sqrMagnitude / (radius * radius) * 0.1f);
			}
			IncreaseAt(item, (int)Mathf.Ceil(dangerAmount));
			if (Controller.settings.DebugDisplayDangerBuildup)
			{
				float num = DangerAt(item);
				if (num > 0f)
				{
					map.debugDrawer.FlashCell(item, num, $"{num}");
				}
			}
		}
	}

	public void Notify_SmokeAt(IntVec3 pos, float concentration)
	{
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = pos + AdjCells[i];
			if (intVec.InBounds(map))
			{
				SetAt(intVec, (int)(150f * AdjWeights[i] * concentration));
			}
		}
		if (Controller.settings.DebugDisplayDangerBuildup)
		{
			FlashCells(pos);
		}
	}

	public float DangerAt(IntVec3 pos)
	{
		if (pos.InBounds(map))
		{
			return (float)Mathf.Clamp(dangerArray[map.cellIndices.CellToIndex(pos)] - GenTicks.TicksGame, 0, 300) / 300f;
		}
		return 0f;
	}

	public float DangerAt(int index)
	{
		return DangerAt(map.cellIndices.IndexToCell(index));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		List<int> list = dangerArray.ToList();
		Scribe_Collections.Look(ref list, "dangerList", LookMode.Value);
		if (list == null || list.Count != map.cellIndices.NumGridCells)
		{
			dangerArray = new int[map.cellIndices.NumGridCells];
		}
	}

	private void FlashCells(IntVec3 pos)
	{
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = pos + AdjCells[i];
			if (intVec.InBounds(map))
			{
				float num = DangerAt(intVec);
				if (num > 0f)
				{
					map.debugDrawer.FlashCell(intVec, num, $"{num}");
				}
			}
		}
	}

	private void IncreaseAt(IntVec3 pos, int amount)
	{
		int num = map.cellIndices.CellToIndex(pos);
		int num2 = dangerArray[num];
		int ticksGame = GenTicks.TicksGame;
		dangerArray[num] = Mathf.Clamp(num2 + amount, ticksGame + amount, ticksGame + 300);
	}

	private void SetAt(IntVec3 pos, int amount)
	{
		int num = map.cellIndices.CellToIndex(pos);
		int min = dangerArray[num];
		int ticksGame = GenTicks.TicksGame;
		dangerArray[num] = Mathf.Clamp(ticksGame + amount, min, ticksGame + 300);
	}
}
