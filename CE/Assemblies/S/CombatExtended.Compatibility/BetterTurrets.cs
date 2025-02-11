using Verse;

namespace CombatExtended.Compatibility;

public class BetterTurrets : IPatch
{
	public bool CanInstall()
	{
		Log.Message("Combat Extended :: Checking Better Turrets");
		return ModLister.HasActiveModWithName("Misc Turret Base Rearmed");
	}

	public void Install()
	{
		Log.Message("Combat Extended :: Installing Better Turrets");
	}
}
