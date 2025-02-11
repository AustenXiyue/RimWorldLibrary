using Verse;

namespace CombatExtended.Compatibility;

public class Vehicles : IPatch
{
	public bool CanInstall()
	{
		Log.Message("Combat Extended :: Checking Vehicle Framework");
		if (!ModLister.HasActiveModWithName("Vehicle Framework"))
		{
			return false;
		}
		return true;
	}

	public void Install()
	{
		Log.Message("Combat Extended :: Installing Vehicle Framework");
	}
}
