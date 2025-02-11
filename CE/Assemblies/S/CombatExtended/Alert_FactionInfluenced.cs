using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Alert_FactionInfluenced : Alert
{
	private int _ticks = -1;

	private string message = "";

	private AlertReport _report = false;

	public static Alert_FactionInfluenced instance;

	protected override Color BGColor
	{
		public get
		{
			return Color.clear;
		}
	}

	public Alert_FactionInfluenced()
	{
		defaultLabel = "";
		defaultExplanation = "";
		instance = this;
	}

	public override string GetLabel()
	{
		return "CE_EnemyInfluenced".Translate();
	}

	public override TaggedString GetExplanation()
	{
		return message;
	}

	public override AlertReport GetReport()
	{
		if (GenTicks.TicksGame - _ticks < 15000)
		{
			return _report;
		}
		_ticks = GenTicks.TicksGame;
		_report = false;
		StringBuilder stringBuilder = new StringBuilder();
		WorldStrengthTracker component = Find.World.GetComponent<WorldStrengthTracker>();
		int num = 0;
		foreach (Faction allFaction in Find.World.factionManager.AllFactions)
		{
			FactionStrengthTracker strengthTracker = allFaction.GetStrengthTracker();
			if (strengthTracker != null && (strengthTracker.StrengthPointsMultiplier != 1f || !strengthTracker.CanRaid))
			{
				if (num > 0)
				{
					stringBuilder.AppendInNewLine("\t");
				}
				_report = AlertReport.Active;
				stringBuilder.AppendInNewLine(strengthTracker.GetExplanation());
				num++;
			}
		}
		message = stringBuilder.ToString();
		return _report;
	}

	public void Dirty()
	{
		_ticks = -1;
	}
}
