using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class ApparelAutoPatcher
{
	static ApparelAutoPatcher()
	{
		if (!Controller.settings.EnableApparelAutopatcher)
		{
			return;
		}
		HashSet<ThingDef> patched = new HashSet<ThingDef>();
		HashSet<string> blacklist = new HashSet<string>(from a in DefDatabase<ApparelModBlacklist>.AllDefs
			from b in a.modIDs
			select b.ToLower());
		HashSet<string> blacklistDefNames = new HashSet<string>(from a in DefDatabase<ApparelModBlacklist>.AllDefs
			from b in a.defNames
			select b);
		HashSet<ModContentPack> mods = new HashSet<ModContentPack>(LoadedModManager.RunningMods.Where((ModContentPack x) => !blacklist.Contains(x.PackageId)));
		IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsApparel && mods.Contains(x.modContentPack) && !blacklistDefNames.Contains(x.defName) && !x.statBases.Any((StatModifier x) => x.stat == CE_StatDefOf.Bulk) && !x.statBases.Any((StatModifier x) => x.stat == CE_StatDefOf.WornBulk));
		if (Controller.settings.DebugAutopatcherLogger)
		{
			foreach (ThingDef item in enumerable)
			{
				Log.Message($"Seeking patches for {item} from {item.modContentPack.PackageId}");
			}
		}
		foreach (ApparelPatcherPresetDef preset in DefDatabase<ApparelPatcherPresetDef>.AllDefs)
		{
			IEnumerable<ThingDef> enumerable2 = enumerable.Where((ThingDef x) => x.Matches(preset) && !patched.Contains(x));
			foreach (ThingDef item2 in enumerable2)
			{
				patched.Add(item2);
				if (Controller.settings.DebugAutopatcherLogger)
				{
					Log.Message("Autopatching " + item2.label + " from " + item2.modContentPack.PackageId);
				}
				if (item2.statBases == null)
				{
					item2.statBases = new List<StatModifier>();
				}
				item2.statBases.Add(new StatModifier
				{
					stat = CE_StatDefOf.Bulk,
					value = preset.Bulk
				});
				item2.statBases.Add(new StatModifier
				{
					stat = CE_StatDefOf.WornBulk,
					value = preset.BulkWorn
				});
				StatModifier[] collection = new StatModifier[2]
				{
					new StatModifier
					{
						value = preset.FinalRatingSharp(item2.GetStatValueDef(StatDefOf.ArmorRating_Sharp)),
						stat = StatDefOf.ArmorRating_Sharp
					},
					new StatModifier
					{
						value = preset.FinalRatingBlunt(item2.GetStatValueDef(StatDefOf.ArmorRating_Blunt)),
						stat = StatDefOf.ArmorRating_Blunt
					}
				};
				if (!item2.statBases.Any((StatModifier x) => x.stat == StatDefOf.ArmorRating_Sharp))
				{
					collection = new StatModifier[2]
					{
						new StatModifier
						{
							value = preset.FinalRatingSharp(item2.GetStatValueDef(StatDefOf.StuffEffectMultiplierArmor)),
							stat = StatDefOf.ArmorRating_Sharp
						},
						new StatModifier
						{
							value = preset.FinalRatingBlunt(item2.GetStatValueDef(StatDefOf.StuffEffectMultiplierArmor)),
							stat = StatDefOf.ArmorRating_Blunt
						}
					};
				}
				item2.statBases.RemoveAll((StatModifier x) => x.stat == StatDefOf.ArmorRating_Sharp || x.stat == StatDefOf.ArmorRating_Blunt || x.stat == StatDefOf.StuffEffectMultiplierArmor);
				item2.statBases.AddRange(collection);
				StatModifier statModifier = item2.statBases.Find((StatModifier x) => x.stat == StatDefOf.Mass);
				if (statModifier != null)
				{
					statModifier.value = preset.Mass;
				}
				else
				{
					item2.statBases.Add(new StatModifier
					{
						stat = StatDefOf.Mass,
						value = preset.Mass
					});
				}
				if (preset.partialStats != null)
				{
					item2.AddModExtension(new PartialArmorExt
					{
						stats = preset.partialStats.ListFullCopy()
					});
				}
				Log.Message("AutoPatched " + item2.label + "(" + item2.defName + ") as " + preset.label);
			}
		}
	}
}
