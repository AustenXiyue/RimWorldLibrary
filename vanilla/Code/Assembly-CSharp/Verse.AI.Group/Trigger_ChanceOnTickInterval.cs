namespace Verse.AI.Group;

public class Trigger_ChanceOnTickInterval : Trigger
{
	private float chancePerInterval;

	private int interval;

	public Trigger_ChanceOnTickInterval(int interval, float chancePerInterval)
	{
		this.chancePerInterval = chancePerInterval;
		this.interval = interval;
	}

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % interval == 0)
		{
			return Rand.Value < chancePerInterval;
		}
		return false;
	}
}
