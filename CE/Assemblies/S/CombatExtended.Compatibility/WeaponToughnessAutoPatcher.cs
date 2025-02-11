using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.Compatibility;

[StaticConstructorOnStartup]
internal class WeaponToughnessAutoPatcher
{
	static WeaponToughnessAutoPatcher()
	{
		if (!Controller.settings.EnableWeaponToughnessAutopatcher)
		{
			return;
		}
		StatDef SHARP_ARMOR_STUFF_POWER = StatDefOf.ArmorRating_Sharp.GetStatPart<StatPart_Stuff>().stuffPowerStat;
		foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsWeapon && !x.IsApparel))
		{
			try
			{
				List<StatModifier> statBases = def.statBases;
				if (statBases == null || statBases.Any((StatModifier x) => x.stat == CE_StatDefOf.StuffEffectMultiplierToughness || x.stat == CE_StatDefOf.ToughnessRating))
				{
					continue;
				}
				float num = def.statBases.Find((StatModifier statMod) => statMod.stat == CE_StatDefOf.Bulk)?.value ?? 0f;
				if (num == 0f)
				{
					continue;
				}
				num = Mathf.Sqrt(num);
				switch (def.techLevel)
				{
				case TechLevel.Spacer:
					num *= 2f;
					break;
				case TechLevel.Ultra:
					num *= 4f;
					break;
				case TechLevel.Archotech:
					num *= 8f;
					break;
				}
				if (!def.IsRangedWeapon)
				{
					List<Tool> tools = def.tools;
					if (tools != null && !tools.Any((Tool tool) => tool.VerbsProperties.Any((VerbProperties property) => property.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)))
					{
						num *= 2f;
					}
				}
				if (def.MadeFromStuff)
				{
					def.statBases.Add(new StatModifier
					{
						stat = CE_StatDefOf.StuffEffectMultiplierToughness,
						value = num
					});
					continue;
				}
				RecipeDef recipeDef2 = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault((RecipeDef recipeDef) => recipeDef.products?.Any((ThingDefCountClass productDef) => productDef.thingDef == def) ?? false);
				IngredientCount ingredientCount2 = null;
				bool? obj;
				if (recipeDef2 == null)
				{
					obj = null;
				}
				else
				{
					List<IngredientCount> ingredients = recipeDef2.ingredients;
					obj = ((ingredients != null) ? new bool?(!ingredients.Empty()) : ((bool?)null));
				}
				bool? flag = obj;
				if (flag == true)
				{
					ingredientCount2 = recipeDef2.ingredients.MaxBy((IngredientCount ingredientCount) => ingredientCount.count);
				}
				float num2 = 1f;
				if (ingredientCount2 != null && ingredientCount2.IsFixedIngredient)
				{
					num2 = ingredientCount2.FixedIngredient.statBases.Find((StatModifier statMod) => statMod.stat == SHARP_ARMOR_STUFF_POWER)?.value ?? 0f;
					num2 *= ingredientCount2.FixedIngredient.GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier ?? 1f;
				}
				else
				{
					num2 = ingredientCount2?.filter?.thingDefs?.Max((ThingDef thingDef) => (thingDef.statBases?.Find((StatModifier statMod) => statMod.stat == SHARP_ARMOR_STUFF_POWER)?.value).GetValueOrDefault() * (thingDef.GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier ?? 1f)) ?? 1f;
				}
				def.statBases.Add(new StatModifier
				{
					stat = CE_StatDefOf.ToughnessRating,
					value = num * num2
				});
			}
			catch (Exception arg)
			{
				Log.Error($"[CE] Failed to autopatch toughness for {def} with error {arg}");
			}
		}
	}
}
