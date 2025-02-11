using Verse;

namespace CombatExtended;

public class MedicineFilthExtension : DefModExtension
{
	public ThingDef filthDefName;

	public float filthSpawnChance = 1f;

	public IntRange filthSpawnQuantity = IntRange.one;
}
