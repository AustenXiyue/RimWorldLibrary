using Verse;

namespace CombatExtended;

public class HoldRecord : IExposable
{
	private const int INVALIDTICKS = 60000;

	private ThingDef _def;

	public int count;

	public bool pickedUp;

	private int _tickJobIssued;

	public virtual bool isTimeValid => !pickedUp && GenTicks.TicksAbs - _tickJobIssued <= 60000;

	public virtual ThingDef thingDef => _def;

	public HoldRecord(ThingDef newThingDef, int newCount)
	{
		_def = newThingDef;
		count = newCount;
		pickedUp = false;
		_tickJobIssued = GenTicks.TicksAbs;
	}

	public HoldRecord()
	{
	}

	public override string ToString()
	{
		return string.Concat("HoldRecord for ", thingDef, " of count ", count, " which has", (!pickedUp) ? "n't" : "", " been picked up", (!pickedUp) ? (" with a job issue time of " + (GenTicks.TicksAbs - _tickJobIssued) + " ago and will go invalid in " + (GenTicks.TicksAbs + 60000 - _tickJobIssued)) : ".");
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref _def, "ThingDef");
		Scribe_Values.Look(ref count, "count", 0);
		Scribe_Values.Look(ref pickedUp, "pickedUp", defaultValue: false);
		if (!pickedUp)
		{
			Scribe_Values.Look(ref _tickJobIssued, "tickOfPickupJob", 0);
		}
	}
}
