using System.Collections.Generic;
using Verse;

namespace CombatExtended.Compatibility;

[StaticConstructorOnStartup]
public class PawnKindPatcher
{
	static PawnKindPatcher()
	{
		if (!Controller.settings.EnablePawnKindAutopatcher)
		{
			return;
		}
		List<PawnKindDef> list = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(delegate(PawnKindDef i)
		{
			bool flag = i.modExtensions?.Any((DefModExtension tt) => tt is LoadoutPropertiesExtension) ?? false;
			bool flag2 = !i.RaceProps.Animal;
			bool valueOrDefault = i.race?.comps?.Any((CompProperties t) => t is CompProperties_Inventory) == true;
			return !flag && flag2 && valueOrDefault;
		});
		foreach (PawnKindDef item in list)
		{
			if (item.modExtensions == null)
			{
				item.modExtensions = new List<DefModExtension>();
			}
			item.modExtensions.Add(new LoadoutPropertiesExtension
			{
				primaryMagazineCount = new FloatRange
				{
					min = 2f,
					max = 5f
				}
			});
		}
	}
}
