using Verse;

namespace CombatExtended;

public class CompProperties_AmmoUser : CompProperties
{
	public int magazineSize = 0;

	public int AmmoGenPerMagOverride = 0;

	public float reloadTime = 1f;

	public bool reloadOneAtATime = false;

	public bool throwMote = true;

	public AmmoSetDef ammoSet = null;

	public float loadedAmmoBulkFactor = 0f;

	public CompProperties_AmmoUser()
	{
		compClass = typeof(CompAmmoUser);
	}
}
