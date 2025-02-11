using RimWorld;

namespace CombatExtended.WorldObjects;

public class WorldObjectCompProperties_Hostility : WorldObjectCompProperties
{
	public bool? AbleToRaidResponse;

	public bool? AbleToShellingResponse;

	public WorldObjectCompProperties_Hostility()
	{
		compClass = typeof(HostilityComp);
	}
}
