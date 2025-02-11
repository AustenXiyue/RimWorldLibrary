using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class OneHandedAutopatcher
{
	static OneHandedAutopatcher()
	{
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.weaponTags?.Contains("CE_OneHandedWeapon") ?? false))
		{
			if (item.statBases == null)
			{
				item.statBases = new List<StatModifier>();
			}
			item.statBases.Add(new StatModifier
			{
				stat = CE_StatDefOf.OneHandedness,
				value = 1f
			});
		}
	}
}
