using Verse;

namespace CombatExtended;

public class CompProperties_AmmoResupplyOnWakeup : CompProperties
{
	public bool dropInPods;

	public CompProperties_AmmoResupplyOnWakeup()
	{
		compClass = typeof(CompAmmoResupplyOnWakeup);
	}
}
