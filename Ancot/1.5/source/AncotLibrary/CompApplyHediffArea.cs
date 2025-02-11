using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AncotLibrary;

public class CompApplyHediffArea : ThingComp
{
	public List<IntVec3> tmpCells = new List<IntVec3>();

	private CompProperties_ApplyHediffArea Props => (CompProperties_ApplyHediffArea)props;

	public virtual float RadiusBase => Props.radius;

	public virtual HediffDef hediff => Props.hediff;

	public virtual float Severity => Props.severity;

	public override void CompTick()
	{
		if (!parent.Spawned || !parent.IsHashIntervalTick(Props.intervalTick))
		{
			return;
		}
		List<Thing> list = new List<Thing>();
		foreach (IntVec3 item in AffectedCells(parent.Position))
		{
			list.AddRange(item.GetThingList(parent.Map));
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Pawn pawn && IsPawnAffected(pawn))
			{
				HealthUtility.AdjustSeverity(pawn, hediff, Severity);
			}
		}
		if (Props.effecter != null)
		{
			Effecter effecter = new Effecter(Props.effecter);
			effecter.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
			effecter.Cleanup();
		}
	}

	public virtual bool IsPawnAffected(Pawn pawn)
	{
		if (Props.applyOnAllyOnly && pawn.Faction != parent.Faction)
		{
			return false;
		}
		if (!Props.applyOnAlly && pawn.Faction == parent.Faction)
		{
			return false;
		}
		if (!Props.applyOnMech && pawn.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (Props.ignoreCaster && pawn == parent)
		{
			return false;
		}
		return true;
	}

	public List<IntVec3> AffectedCells(IntVec3 target)
	{
		tmpCells.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(target, Props.radius, useCenter: true))
		{
			if (item.IsValid || item.InBounds(parent.Map))
			{
				tmpCells.Add(item);
			}
		}
		tmpCells = tmpCells.Distinct().ToList();
		tmpCells.RemoveAll((IntVec3 cell) => !CanUseCell(cell));
		return tmpCells;
		bool CanUseCell(IntVec3 c)
		{
			if (!c.InBounds(parent.Map))
			{
				return false;
			}
			return true;
		}
	}
}
