using Verse;

namespace CombatExtended;

public class VerbPropertiesCE : VerbProperties
{
	public RecoilPattern recoilPattern = RecoilPattern.None;

	public int ammoConsumedPerShotCount = 1;

	public float recoilAmount = 0f;

	public float indirectFirePenalty = 0f;

	public float circularError = 0f;

	public int ticksToTruePosition = 5;

	public bool ejectsCasings = true;

	public bool ignorePartialLoSBlocker = false;

	public bool interruptibleBurst = true;
}
