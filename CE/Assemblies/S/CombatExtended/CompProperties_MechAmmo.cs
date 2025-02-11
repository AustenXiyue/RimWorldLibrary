using Verse;

namespace CombatExtended;

public class CompProperties_MechAmmo : CompProperties
{
	[NoTranslate]
	public string gizmoIconSetMagCount;

	[NoTranslate]
	public string gizmoIconTakeAmmoNow;

	public CompProperties_MechAmmo()
	{
		compClass = typeof(CompMechAmmo);
	}
}
