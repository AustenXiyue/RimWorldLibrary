using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class MeleeTargettingAdd
{
	static MeleeTargettingAdd()
	{
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef y) => y.race != null && y.race.Humanlike))
		{
			if (item.comps == null)
			{
				item.comps = new List<CompProperties>();
			}
			item.comps.Add(new CompProperties
			{
				compClass = typeof(CompMeleeTargettingGizmo)
			});
		}
	}
}
