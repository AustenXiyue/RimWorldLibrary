using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AncotLibrary;

public class Alert_MechAutoFight : Alert
{
	public List<Pawn> targets = new List<Pawn>();

	private void GetTargets()
	{
		targets.Clear();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Pawn> spawnedColonyMechs = maps[i].mapPawns.SpawnedColonyMechs;
			for (int j = 0; j < spawnedColonyMechs.Count; j++)
			{
				CompMechAutoFight compMechAutoFight = spawnedColonyMechs[j].TryGetComp<CompMechAutoFight>();
				if (compMechAutoFight != null && compMechAutoFight.autoFight)
				{
					targets.Add(spawnedColonyMechs[j]);
				}
			}
		}
	}

	public Alert_MechAutoFight()
	{
		defaultLabel = "Ancot.Alert_MechAutoFight".Translate();
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}

	public override TaggedString GetExplanation()
	{
		return "Ancot.Alert_MechAutoFightDescPrefix".Translate() + ":\n" + targets.Select((Pawn p) => p.LabelCap).ToLineList("  - ") + "\n\n" + "Ancot.Alert_MechAutoFightDesc".Translate();
	}
}
