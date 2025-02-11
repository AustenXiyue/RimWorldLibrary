using Verse;

namespace CombatExtended.Compatibility;

public class MiscTurrets : IPatch
{
	public bool CanInstall()
	{
		Log.Message("Combat Extended :: Checking Misc Turrets");
		return ModLister.HasActiveModWithName("Misc. TurretBase, Objects");
	}

	public void Install()
	{
		Log.Message("Combat Extended :: Installing Misc Turrets");
	}
}
