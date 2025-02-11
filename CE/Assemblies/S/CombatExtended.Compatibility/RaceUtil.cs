using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended.Compatibility;

public static class RaceUtil
{
	public static SimpleCurve SharpCurve => new SimpleCurve
	{
		new CurvePoint(0.2f, 1f),
		new CurvePoint(2f, 20f)
	};

	public static SimpleCurve BluntCurve => new SimpleCurve
	{
		new CurvePoint(0.2f, 2f),
		new CurvePoint(2f, 40f)
	};

	public static ToolCE ConvertTool(this Tool tool)
	{
		ToolCE toolCE = new ToolCE
		{
			capacities = tool.capacities,
			armorPenetrationSharp = tool.armorPenetration,
			armorPenetrationBlunt = tool.armorPenetration,
			cooldownTime = tool.cooldownTime,
			chanceFactor = tool.chanceFactor,
			power = tool.power,
			label = tool.label,
			linkedBodyPartsGroup = tool.linkedBodyPartsGroup
		};
		if (toolCE.cooldownTime <= 0f)
		{
			toolCE.cooldownTime = 2f;
		}
		if (toolCE.armorPenetrationSharp <= 0f)
		{
			toolCE.armorPenetrationSharp = 0.5f;
			toolCE.armorPenetrationBlunt = 2f;
		}
		return toolCE;
	}

	public static void PatchHARs()
	{
		IEnumerable<ThingDef> enumerable = from x in DefDatabase<ThingDef>.AllDefs
			where x.GetType().ToString() == "AlienRace.ThingDef_AlienRace"
			where x.tools != null && x.tools.Any((Tool y) => y != null && !(y is ToolCE))
			select x;
		int num = 0;
		foreach (ThingDef item2 in enumerable)
		{
			if (item2.modExtensions == null)
			{
				item2.modExtensions = new List<DefModExtension>();
			}
			item2.modExtensions.Add(new RacePropertiesExtensionCE
			{
				bodyShape = CE_BodyShapeDefOf.Humanoid
			});
			List<Tool> list = new List<Tool>();
			foreach (Tool tool in item2.tools)
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
			item2.tools = list;
			if (item2.comps == null)
			{
				item2.comps = new List<CompProperties>();
			}
			item2.comps.Add(new CompProperties_Inventory());
			item2.comps.Add(new CompProperties_Suppressable());
			item2.comps.Add(new CompProperties
			{
				compClass = typeof(CompPawnGizmo)
			});
			StatModifier statModifier = item2.statBases.Find((StatModifier y) => y.stat == StatDefOf.ArmorRating_Sharp);
			if (statModifier != null)
			{
				statModifier.value = SharpCurve.Evaluate(statModifier.value);
				StatModifier item = new StatModifier
				{
					stat = CE_StatDefOf.BodyPartSharpArmor,
					value = statModifier.value
				};
				item2.statBases.Add(item);
			}
			else
			{
				statModifier = new StatModifier
				{
					stat = StatDefOf.ArmorRating_Sharp,
					value = 0.125f
				};
				item2.statBases.Add(statModifier);
			}
			StatModifier statModifier2 = item2.statBases.Find((StatModifier y) => y.stat == StatDefOf.ArmorRating_Blunt);
			if (statModifier2 != null)
			{
				statModifier2.value = BluntCurve.Evaluate(statModifier2.value);
			}
			else
			{
				statModifier2 = new StatModifier
				{
					stat = StatDefOf.ArmorRating_Blunt,
					value = 1f
				};
				item2.statBases.Add(statModifier2);
			}
			num++;
		}
		Log.Message("CE successfully patched " + num + " humanoid alien races");
	}
}
