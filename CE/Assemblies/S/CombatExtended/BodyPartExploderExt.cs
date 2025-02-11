using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class BodyPartExploderExt : DefModExtension
{
	public float triggerChance;

	public List<DamageDef> allowedDamageDefs;
}
