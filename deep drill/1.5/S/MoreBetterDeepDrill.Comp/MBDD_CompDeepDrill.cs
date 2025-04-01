using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MoreBetterDeepDrill.Comp;

public class MBDD_CompDeepDrill : ThingComp
{
	protected CompPowerTrader powerComp;

	protected float portionProgress;

	protected float portionYieldPct;

	protected float drillPower;

	protected int lastUsedTick = -99999;

	protected const float WorkPerPortionBase = 10000f;

	protected List<Pawn> drillers = new List<Pawn>();

	public bool CanDrillNow;

	public float PortionYieldPct
	{
		get
		{
			return portionYieldPct;
		}
		protected set
		{
			if (value > 0f)
			{
				portionYieldPct = value;
			}
			else
			{
				portionYieldPct = 0f;
			}
		}
	}

	public float DrillPower
	{
		get
		{
			return drillPower;
		}
		protected set
		{
			if (value > 0f)
			{
				drillPower = value;
			}
			else
			{
				drillPower = 0f;
			}
		}
	}

	public float ProgressToNextPortionPercent => portionProgress / 10000f;

	public bool IsDrillingNow => drillers.Count != 0;

	public override void CompTick()
	{
		if (Current.Game.tickManager.TicksGame % 300 == 0)
		{
			UpdateCanDrillState();
		}
		if (CanDrillNow)
		{
			base.CompTick();
			if (drillers.Count > 0)
			{
				DrillWork();
			}
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		powerComp = parent.TryGetComp<CompPowerTrader>();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref portionProgress, "portionProgress", 0f);
		Scribe_Values.Look(ref portionYieldPct, "portionYieldPct", 0f);
		Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", 0);
	}

	public virtual void DrillJoinWork(Pawn driller)
	{
		foreach (Pawn driller2 in drillers)
		{
			if (driller == driller2)
			{
				return;
			}
		}
		float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed);
		DrillPower += statValue;
		drillers.Add(driller);
	}

	public virtual void DrillLeaveWork(Pawn driller)
	{
		float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed);
		DrillPower -= statValue;
		drillers.Remove(driller);
	}

	public virtual void DrillWork()
	{
		portionProgress += DrillPower;
		foreach (Pawn driller in drillers)
		{
			float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed);
			PortionYieldPct += (float)drillers.Count * statValue * driller.GetStatValue(StatDefOf.MiningYield) / 10000f;
		}
		lastUsedTick = Find.TickManager.TicksGame;
		if (portionProgress > 10000f)
		{
			TryProducePortion(PortionYieldPct);
			portionProgress = 0f;
			PortionYieldPct = 0f;
		}
	}

	public override void PostDeSpawn(Map map)
	{
		portionProgress = 0f;
		PortionYieldPct = 0f;
		lastUsedTick = -99999;
	}

	protected virtual void TryProducePortion(float yieldPct, Pawn driller = null)
	{
	}

	protected virtual void UpdateCanDrillState()
	{
	}

	public virtual bool UsedLastTick()
	{
		return lastUsedTick >= Find.TickManager.TicksGame - 1;
	}

	public virtual void DrillWorkForPRF(float progress, float yieldPct, int lastUsedTick)
	{
		portionProgress += progress;
		PortionYieldPct += yieldPct;
		this.lastUsedTick = lastUsedTick;
		if (portionProgress > 10000f)
		{
			TryProducePortion(PortionYieldPct);
			portionProgress = 0f;
			PortionYieldPct = 0f;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: Produce portion (100% yield)";
			command_Action.action = delegate
			{
				TryProducePortion(1f);
			};
			yield return command_Action;
		}
	}
}
