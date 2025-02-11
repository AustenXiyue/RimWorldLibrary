using Verse;

namespace CombatExtended.Compatibility;

public class SOS2 : IPatch
{
	private const string ModName = "Save Our Ship 2";

	bool IPatch.CanInstall()
	{
		Log.Message("Combat Extended :: Checking SOS2");
		if (!ModLister.HasActiveModWithName("Save Our Ship 2"))
		{
			return false;
		}
		return true;
	}

	public void Install()
	{
		Log.Message("Combat Extended :: Installing SOS2");
	}
}
