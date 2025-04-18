using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_PassOutMany : QuestPartCanDisable
{
	public string inSignal;

	public List<string> outSignals = new List<string>();

	protected override void ProcessQuestSignal(Signal signal)
	{
		if (signal.tag == inSignal)
		{
			for (int i = 0; i < outSignals.Count; i++)
			{
				Find.SignalManager.SendSignal(new Signal(outSignals[i], signal.args));
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref outSignals, "outSignals", LookMode.Value);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		for (int i = 0; i < 3; i++)
		{
			outSignals.Add("DebugSignal" + Rand.Int);
		}
	}
}
