using Verse;

namespace AncotLibrary;

public class DamageBuffTracker : IExposable
{
	public float pct;

	public int validUntil = -1;

	public int charges = -1;

	public void ExposeData()
	{
		Scribe_Values.Look(ref pct, "pct", 0f);
		Scribe_Values.Look(ref validUntil, "validUntil", 0);
		Scribe_Values.Look(ref charges, "charges", 0);
	}
}
