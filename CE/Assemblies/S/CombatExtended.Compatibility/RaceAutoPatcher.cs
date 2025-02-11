using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended.Compatibility;

[StaticConstructorOnStartup]
public class RaceAutoPatcher
{
	static RaceAutoPatcher()
	{
		if (!Controller.settings.EnableRaceAutopatcher)
		{
			return;
		}
		IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.race != null && x.race.Animal && x.tools != null && x.tools.Any((Tool y) => y != null && !(y is ToolCE)));
		int num = 0;
		string text = "";
		foreach (ThingDef item5 in enumerable)
		{
			if (item5.modExtensions == null)
			{
				item5.modExtensions = new List<DefModExtension>();
			}
			item5.modExtensions.Add(new RacePropertiesExtensionCE
			{
				bodyShape = CE_BodyShapeDefOf.Quadruped
			});
			List<Tool> list = new List<Tool>();
			foreach (Tool tool in item5.tools)
			{
				if (!(tool is ToolCE))
				{
					list.Add(tool.ConvertTool());
				}
				else
				{
					list.Add(tool);
				}
			}
			item5.tools = list;
			StatModifier statModifier = item5.statBases.Find((StatModifier y) => y.stat == StatDefOf.ArmorRating_Sharp);
			if (statModifier != null)
			{
				statModifier.value = RaceUtil.SharpCurve.Evaluate(statModifier.value);
				StatModifier item = new StatModifier
				{
					stat = CE_StatDefOf.BodyPartSharpArmor,
					value = statModifier.value
				};
				item5.statBases.Add(item);
			}
			else
			{
				statModifier = new StatModifier
				{
					stat = StatDefOf.ArmorRating_Sharp,
					value = 0.125f
				};
				item5.statBases.Add(statModifier);
				StatModifier item2 = new StatModifier
				{
					stat = CE_StatDefOf.BodyPartSharpArmor,
					value = 1f
				};
				item5.statBases.Add(item2);
			}
			StatModifier statModifier2 = item5.statBases.Find((StatModifier y) => y.stat == StatDefOf.ArmorRating_Blunt);
			if (statModifier2 != null)
			{
				statModifier2.value = RaceUtil.BluntCurve.Evaluate(statModifier2.value);
				StatModifier item3 = new StatModifier
				{
					stat = CE_StatDefOf.BodyPartBluntArmor,
					value = statModifier2.value
				};
				item5.statBases.Add(item3);
			}
			else
			{
				statModifier2 = new StatModifier
				{
					stat = StatDefOf.ArmorRating_Blunt,
					value = 1f
				};
				item5.statBases.Add(statModifier2);
				StatModifier item4 = new StatModifier
				{
					stat = CE_StatDefOf.BodyPartBluntArmor,
					value = 1f
				};
				item5.statBases.Add(item4);
			}
			num++;
			text = text + item5.ToString() + "\n";
		}
		if (num > 0)
		{
			Log.Message("CE successfully patched " + num + " animals.\nAnimal Defs autopatched:" + text);
		}
		if (ModLister.HasActiveModWithName("Humanoid Alien Races"))
		{
			RaceUtil.PatchHARs();
		}
	}
}
