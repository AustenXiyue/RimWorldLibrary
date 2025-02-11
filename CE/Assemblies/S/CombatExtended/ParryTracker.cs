using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ParryTracker : MapComponent
{
	private struct ParryCounter
	{
		public int parries;

		private int timeoutTick;

		public ParryCounter(int timeoutTicks)
		{
			parries = 0;
			timeoutTick = Find.TickManager.TicksGame + timeoutTicks;
		}

		public bool ShouldTimeout()
		{
			return Find.TickManager.TicksGame >= timeoutTick;
		}
	}

	private const int SkillPerParry = 4;

	private const int TicksToTimeout = 120;

	private Dictionary<Pawn, ParryCounter> parryTracker = new Dictionary<Pawn, ParryCounter>();

	public ParryTracker(Map map)
		: base(map)
	{
	}

	private int GetUsedParriesFor(Pawn pawn)
	{
		if (!parryTracker.TryGetValue(pawn, out var value))
		{
			return 0;
		}
		return value.parries;
	}

	public bool CheckCanParry(Pawn pawn)
	{
		if (pawn == null)
		{
			Log.Error("CE tried checking CanParry with Null-Pawn");
			return false;
		}
		int num = Mathf.RoundToInt(pawn.skills.GetSkill(SkillDefOf.Melee).Level / 4) - GetUsedParriesFor(pawn);
		return num > 0;
	}

	public void RegisterParryFor(Pawn pawn, int timeoutTicks)
	{
		if (!parryTracker.TryGetValue(pawn, out var value))
		{
			value = new ParryCounter(timeoutTicks);
			parryTracker.Add(pawn, value);
		}
		value.parries++;
	}

	public void ResetParriesFor(Pawn pawn)
	{
		parryTracker.Remove(pawn);
	}

	public override void MapComponentTick()
	{
		if (Find.TickManager.TicksGame % 10 == 0)
		{
			KeyValuePair<Pawn, ParryCounter>[] array = parryTracker.Where((KeyValuePair<Pawn, ParryCounter> kvp) => kvp.Value.ShouldTimeout()).ToArray();
			foreach (KeyValuePair<Pawn, ParryCounter> keyValuePair in array)
			{
				parryTracker.Remove(keyValuePair.Key);
			}
		}
	}

	public override void ExposeData()
	{
	}
}
