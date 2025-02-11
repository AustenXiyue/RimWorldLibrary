using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class BurnCompAdder
{
	static BurnCompAdder()
	{
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.race != null))
		{
			if (item.comps == null)
			{
				item.comps = new List<CompProperties>();
			}
			item.comps.Add(new CompProperties
			{
				compClass = typeof(Comp_BurnDamageCalc)
			});
		}
	}
}
