namespace CombatExtended.Compatibility;

public interface IPatch
{
	bool CanInstall();

	void Install();
}
