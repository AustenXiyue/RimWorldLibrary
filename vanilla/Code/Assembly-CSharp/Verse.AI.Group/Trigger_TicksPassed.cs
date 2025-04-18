using UnityEngine;

namespace Verse.AI.Group;

public class Trigger_TicksPassed : Trigger
{
	private int duration = 100;

	protected TriggerData_TicksPassed Data => (TriggerData_TicksPassed)data;

	public int TicksLeft => Mathf.Max(duration - Data.ticksPassed, 0);

	public Trigger_TicksPassed(int tickLimit)
	{
		data = new TriggerData_TicksPassed();
		duration = tickLimit;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick)
		{
			if (data == null || !(data is TriggerData_TicksPassed))
			{
				BackCompatibility.TriggerDataTicksPassedNull(this);
			}
			TriggerData_TicksPassed triggerData_TicksPassed = Data;
			triggerData_TicksPassed.ticksPassed++;
			return triggerData_TicksPassed.ticksPassed > duration;
		}
		return false;
	}

	public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
	{
		if (!transition.sources.Contains(previousToil))
		{
			Data.ticksPassed = 0;
		}
	}
}
