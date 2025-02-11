using Verse;

namespace CombatExtended;

public class WorldObjectHealthExtension : DefModExtension
{
	public bool destoyedInstantly = false;

	public float healthModifier = 1f;

	public float chanceToNegateDamage = -1f;

	public bool techLevelNoImpact = false;
}
