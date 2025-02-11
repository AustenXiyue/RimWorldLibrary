using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Milira;

public class CompResonator : ThingComp
{
	private bool shutDown;

	private CompProperties_Resonator Props => (CompProperties_Resonator)props;

	public bool IsPawnAffected(Pawn target)
	{
		if (shutDown)
		{
			return false;
		}
		if (target.Dead || target.health == null)
		{
			return false;
		}
		if (target.Faction != parent.Faction)
		{
			return false;
		}
		if (Props.onlyTargetMechs)
		{
			return target.RaceProps.IsMechanoid;
		}
		return true;
	}

	public override void CompTick()
	{
		if (shutDown || !parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
		{
			return;
		}
		List<Building> list = new List<Building>();
		foreach (Thing allThing in parent.Map.listerThings.AllThings)
		{
			if (allThing is Building building && building.def.building.buildingTags.Contains(Props.resonatorTag) && building.Faction == parent.Faction)
			{
				list.Add(building);
			}
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (!IsPawnAffected(item))
			{
				continue;
			}
			Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
			BodyPartRecord bodyPartRecord = item.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def.defName == "Milian_Brain");
			if (firstHediffOfDef != null)
			{
				if (list.Count == 1 && firstHediffOfDef.Severity < 0.25f)
				{
					firstHediffOfDef.Severity = 0.25f;
				}
				if (list.Count == 2 && firstHediffOfDef.Severity < 0.5f)
				{
					firstHediffOfDef.Severity = 0.5f;
				}
				if (list.Count == 3 && firstHediffOfDef.Severity < 0.75f)
				{
					firstHediffOfDef.Severity = 0.75f;
				}
				if (list.Count >= 4)
				{
					firstHediffOfDef.Severity = 1f;
				}
			}
		}
	}

	public override void PostDraw()
	{
		if (!Find.Selector.SelectedObjectsListForReading.Contains(parent))
		{
			return;
		}
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (IsPawnAffected(item))
			{
				GenDraw.DrawLineBetween(item.DrawPos, parent.DrawPos);
			}
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		foreach (Pawn item in previousMap.mapPawns.AllPawnsSpawned)
		{
			if (IsPawnAffected(item))
			{
				Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
				if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.25f)
				{
					firstHediffOfDef.Severity -= 0.25f;
				}
				else if (firstHediffOfDef != null && (double)firstHediffOfDef.Severity == 0.25)
				{
					firstHediffOfDef.Severity = 0.01f;
				}
			}
		}
	}

	public override void Notify_LordDestroyed()
	{
		base.Notify_LordDestroyed();
		shutDown = true;
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Props.appendString);
		return stringBuilder.ToString();
	}
}
