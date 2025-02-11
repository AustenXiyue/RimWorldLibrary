using System.Collections.Generic;
using System.Linq;
using CombatExtended.AI;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Smoke : Gas
{
	public const int UpdateIntervalTicks = 30;

	private const float InhalationPerSec = 0.0225f;

	private const float DensityDissipationThreshold = 3f;

	private const float MinSpreadDensity = 1f;

	private const float MaxDensity = 12800f;

	private const float BugConfusePercent = 0.15f;

	private const float LethalAirPPM = 10000f;

	private float density;

	private int updateTickOffset;

	private DangerTracker _dangerTracker = null;

	private DangerTracker DangerTracker => _dangerTracker ?? (_dangerTracker = base.Map.GetDangerTracker());

	public override string LabelNoCount => base.LabelNoCount + " (" + Mathf.RoundToInt(density / 10f) + " ppm)";

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		updateTickOffset = Rand.Range(0, 30);
		base.SpawnSetup(map, respawningAfterLoad);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref density, "density", 0f);
	}

	private bool CanMoveTo(IntVec3 pos)
	{
		int result;
		if (pos.InBounds(base.Map))
		{
			if (pos.Filled(base.Map))
			{
				Building_Door door = pos.GetDoor(base.Map);
				if (door == null || !door.Open)
				{
					result = ((pos.GetFirstThing<Building_Vent>(base.Map)?.TryGetComp<CompFlickable>().SwitchIsOn ?? false) ? 1 : 0);
					goto IL_005b;
				}
			}
			result = 1;
		}
		else
		{
			result = 0;
		}
		goto IL_005b;
		IL_005b:
		return (byte)result != 0;
	}

	public override void Tick()
	{
		if (density > 3f)
		{
			destroyTick++;
			if (Rand.Range(0, 10) == 5)
			{
				float num = density * 0.0001f;
				if (density > 300f && (float)Rand.Range(0, 12800) < num)
				{
					FilthMaker.TryMakeFilth(base.Position, base.Map, ThingDefOf.Filth_Ash);
				}
				density -= num;
			}
		}
		if ((GenTicks.TicksGame + updateTickOffset) % 30 == 0)
		{
			if (!CanMoveTo(base.Position))
			{
				SpreadToAdjacentCells();
				Destroy();
				return;
			}
			if (!base.Position.Roofed(base.Map))
			{
				UpdateDensityBy(-60f);
			}
			SpreadToAdjacentCells();
			ApplyHediffs();
		}
		if (this.IsHashIntervalTick(120))
		{
			DangerTracker?.Notify_SmokeAt(base.Position, density / 12800f);
		}
		base.Tick();
	}

	private void ApplyHediffs()
	{
		if (!base.Position.InBounds(base.Map))
		{
			return;
		}
		List<Thing> list = (from t in base.Position.GetThingList(base.Map)
			where t is Pawn
			select t).ToList();
		float num = Mathf.Pow(density / 10000f, 1.25f);
		float num2 = 0.0225f * density / 12800f;
		foreach (Pawn item in list)
		{
			if (item.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
			{
				if (density > 1920.0001f)
				{
					item.mindState.mentalStateHandler.TryStartMentalState(CE_MentalStateDefOf.WanderConfused);
				}
				continue;
			}
			item.TryGetComp<CompTacticalManager>()?.GetTacticalComp<CompGasMask>()?.Notify_ShouldEquipGasMask();
			float statValue = item.GetStatValue(CE_StatDefOf.SmokeSensitivity);
			float num3 = PawnCapacityUtility.CalculateCapacityLevel(item.health.hediffSet, PawnCapacityDefOf.Breathing);
			float num4 = item.health.hediffSet.GetFirstHediffOfDef(CE_HediffDefOf.SmokeInhalation)?.Severity ?? 0f;
			if (num3 < 0.01f)
			{
				num3 = 0.01f;
			}
			float num5 = statValue / num3 * num;
			if (num5 > 1.5f)
			{
				num5 = 1.5f;
			}
			float num6 = num5 - num4;
			bool downed = item.Downed;
			bool flag = item.Awake();
			float num7 = num2 * statValue / num3 * Mathf.Pow(num6, 1.5f);
			if (downed)
			{
				num7 /= 100f;
			}
			if (!flag)
			{
				num7 /= 2f;
				if ((double)num4 > 0.1)
				{
					RestUtility.WakeUp(item);
				}
			}
			if (num7 > 0f && num6 > 0f)
			{
				HealthUtility.AdjustSeverity(item, CE_HediffDefOf.SmokeInhalation, num7);
			}
		}
	}

	private void SpreadToAdjacentCells()
	{
		if (!(density >= 1f))
		{
			return;
		}
		List<IntVec3> list = GenAdjFast.AdjacentCellsCardinal(base.Position).InRandomOrder().Where(CanMoveTo)
			.ToList();
		foreach (IntVec3 item in list)
		{
			if (item.GetGas(base.Map) is Smoke smoke)
			{
				float num = density - smoke.density;
				TransferDensityTo(smoke, num / 2f);
			}
			else
			{
				Smoke target = (Smoke)GenSpawn.Spawn(CE_ThingDefOf.Gas_BlackSmoke, item, base.Map);
				TransferDensityTo(target, density / 2f);
			}
		}
	}

	public void UpdateDensityBy(float diff)
	{
		density = Mathf.Clamp(density + diff, 0f, 12800f);
	}

	private void TransferDensityTo(Smoke target, float value)
	{
		UpdateDensityBy(0f - value);
		target.UpdateDensityBy(value);
	}

	public float GetOpacity()
	{
		return 0.05f + 0.95f * (density / 12800f);
	}
}
