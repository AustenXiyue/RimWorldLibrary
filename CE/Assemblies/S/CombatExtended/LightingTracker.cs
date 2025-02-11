using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class LightingTracker : MapComponent
{
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	private struct MuzzleRecord : IExposable
	{
		private int createdAt;

		private float intensity;

		public float Intensity => intensity * (1f - Mathf.Min((float)GenTicks.TicksGame - (float)createdAt, 500f) / 500f);

		public bool Recent => createdAt >= 0 && GenTicks.TicksGame - createdAt <= 500;

		public int Age => GenTicks.TicksGame - createdAt;

		public MuzzleRecord(float intensity = 0f)
		{
			if (intensity > 0.01f)
			{
				createdAt = GenTicks.TicksGame;
				this.intensity = Mathf.Clamp01(intensity);
			}
			else
			{
				createdAt = -1;
				this.intensity = 0f;
			}
		}

		public static MuzzleRecord operator +(MuzzleRecord first, MuzzleRecord second)
		{
			if (!first.Recent || !second.Recent)
			{
				return new MuzzleRecord(Mathf.Max(first.intensity, second.intensity, 0f));
			}
			int ticksGame = GenTicks.TicksGame;
			float num = 1f - (float)Mathf.Min(ticksGame - first.createdAt, 500) / 500f;
			float num2 = 1f - (float)Mathf.Min(ticksGame - second.createdAt, 500) / 500f;
			return new MuzzleRecord(num * first.intensity + num2 * second.intensity);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref createdAt, "createdAt", 0);
			Scribe_Values.Look(ref intensity, "intensity", 0f);
			createdAt = ((intensity != 0f) ? GenTicks.TicksGame : int.MinValue);
		}
	}

	private const int FLASHAGE = 500;

	private const float WEIGHTS_DIG = 0.8f;

	private const float WEIGHTS_COL = 0.5f;

	private const float WEIGHTSSUM = 6.2f;

	private static readonly IntVec3[] AdjCells;

	private static readonly float[] AdjWeights;

	private readonly float[] glowGridCache;

	private int lastTick = 0;

	private const float MUZZLEFLASH_MEAN = 13f;

	private float curSkyGlow = -1f;

	private MuzzleRecord[] muzzle_grid;

	private readonly List<IntVec3> _removal = new List<IntVec3>();

	private readonly HashSet<IntVec3> _recent = new HashSet<IntVec3>();

	private readonly CellIndices _cellIndices;

	public int NumGridCells => map.cellIndices.NumGridCells;

	public bool IsNight => map.skyManager.CurSkyGlow < 0.5f;

	public float CurSkyGlow
	{
		get
		{
			if (curSkyGlow == -1f)
			{
				curSkyGlow = map.skyManager.CurSkyGlow;
			}
			return curSkyGlow;
		}
	}

	static LightingTracker()
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

	public LightingTracker(Map map)
		: base(map)
	{
		_cellIndices = map.cellIndices;
		muzzle_grid = new MuzzleRecord[NumGridCells];
		glowGridCache = new float[map.Size.x * map.Size.y];
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Expose_MuzzleGrid();
	}

	public override void MapComponentUpdate()
	{
		base.MapComponentUpdate();
		curSkyGlow = map.skyManager.CurSkyGlow;
	}

	public override void MapComponentOnGUI()
	{
		if (Controller.settings.DebuggingMode && Controller.settings.DebugMuzzleFlash)
		{
			_removal.Clear();
			foreach (IntVec3 item in _recent)
			{
				if (!item.InBounds(map))
				{
					_removal.Add(item);
					continue;
				}
				MuzzleRecord muzzleRecord = muzzle_grid[CellToIndex(item)];
				if (!muzzleRecord.Recent)
				{
					_removal.Add(item);
				}
				float num = CombatGlowAt(item);
				map.debugDrawer.FlashCell(item, num, num.ToStringByStyle(ToStringStyle.FloatOne), 15);
			}
			foreach (IntVec3 item2 in _removal)
			{
				_recent.Remove(item2);
			}
		}
		else if (_recent.Count > 0)
		{
			_recent.Clear();
		}
		base.MapComponentOnGUI();
	}

	public void Notify_ShotsFiredAt(IntVec3 position, float intensity = 0.8f)
	{
		if (!position.InBounds(map) || intensity <= 0.001f)
		{
			return;
		}
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = position + AdjCells[i];
			if (intVec.InBounds(map))
			{
				muzzle_grid[CellToIndex(intVec)] += new MuzzleRecord(intensity * AdjWeights[i] / 13f);
			}
		}
		if (Controller.settings.DebuggingMode && Controller.settings.DebugMuzzleFlash)
		{
			for (int j = 0; j < 9; j++)
			{
				_recent.Add(position + AdjCells[j]);
			}
		}
	}

	public float CombatGlowAt(IntVec3 position)
	{
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			num += AdjWeights[i] * GetGlowForCell(position + AdjCells[i]) / 6.2f;
		}
		return Mathf.Min(num, IsNight ? 0.5f : 1f);
	}

	private float GameGlowAt(IntVec3 source)
	{
		int ticksAbs = Find.TickManager.TicksAbs;
		if (ticksAbs != lastTick)
		{
			Array.Fill(glowGridCache, -1f);
			lastTick = ticksAbs;
		}
		float num = glowGridCache[source.x * map.Size.y + source.y];
		if (num == -1f)
		{
			num = (glowGridCache[source.x * map.Size.y + source.y] = map.glowGrid.GroundGlowAt(source));
		}
		return num;
	}

	public float CombatGlowAtFor(IntVec3 source, IntVec3 target)
	{
		float num = GameGlowAt(source);
		if (num > 0.5f)
		{
			return Mathf.Max(CombatGlowAt(target), num / 2f);
		}
		return CombatGlowAt(target);
	}

	public float GetGlowForCell(IntVec3 position)
	{
		if (position.InBounds(map))
		{
			float num = GameGlowAt(position);
			MuzzleRecord muzzleRecord = muzzle_grid[CellToIndex(position)];
			if (!muzzleRecord.Recent)
			{
				return num;
			}
			return Mathf.Clamp01(Mathf.Max(num, muzzleRecord.Intensity * 0.75f));
		}
		return 0f;
	}

	private int CellToIndex(IntVec3 position)
	{
		return _cellIndices.CellToIndex(position);
	}

	private void Expose_MuzzleGrid()
	{
		List<MuzzleRecord> list = muzzle_grid?.ToList() ?? new List<MuzzleRecord>();
		Scribe_Collections.Look(ref list, "muzzle_grid", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			if (list == null || list.Count < NumGridCells)
			{
				muzzle_grid = new MuzzleRecord[NumGridCells];
			}
			else
			{
				muzzle_grid = list.ToArray();
			}
		}
		list?.Clear();
	}
}
