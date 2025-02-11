using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class AmmoGeneralizer
{
	static AmmoGeneralizer()
	{
		if (!Controller.settings.GenericAmmo)
		{
			return;
		}
		IEnumerable<AmmoSetDef> enumerable = DefDatabase<AmmoSetDef>.AllDefs.Where((AmmoSetDef x) => x.similarTo != null);
		foreach (AmmoSetDef item in enumerable)
		{
			List<AmmoLink> list = new List<AmmoLink>();
			AmmoSetDef similarTo = item.similarTo;
			foreach (AmmoLink link in item.ammoTypes)
			{
				AmmoLink ammoLink = similarTo.ammoTypes.Find((AmmoLink x) => x.ammo.ammoClass == link.ammo.ammoClass);
				if (ammoLink != null)
				{
					string text = ((link.projectile.projectile is ProjectilePropertiesCE { genericLabelOverride: not null } projectilePropertiesCE) ? projectilePropertiesCE.genericLabelOverride : similarTo.label);
					link.projectile.label = text + " " + "CE_GenericBullet".Translate() + " (" + link.ammo.ammoClass.labelShort + ")";
					list.Add(new AmmoLink
					{
						ammo = ammoLink.ammo,
						projectile = link.projectile
					});
				}
				RecipeDef recipeDef = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.defName == "Make" + link.ammo.defName).FirstOrFallback();
				link.ammo.SetMenuHidden(value: true);
				if (recipeDef != null)
				{
					if (CE_ThingDefOf.AmmoBench.AllRecipes.Contains(recipeDef))
					{
						CE_ThingDefOf.AmmoBench.AllRecipes.Remove(recipeDef);
					}
					if ((recipeDef.AllRecipeUsers.Count() > 0) | ((recipeDef.recipeUsers?.Count() ?? 0) > 0))
					{
						recipeDef.recipeUsers = new List<ThingDef>();
					}
				}
				link.ammo.AmmoSetDefs.Add(similarTo);
			}
			item.label = similarTo.label;
			item.ammoTypes = list;
		}
		IEnumerable<ScenarioDef> enumerable2 = DefDatabase<ScenarioDef>.AllDefs.Where((ScenarioDef x) => x.scenario.AllParts.Any((ScenPart y) => y is ScenPart_ScatterThings || y is ScenPart_StartingThing_Defined));
		foreach (ScenarioDef item2 in enumerable2)
		{
			IEnumerable<ScenPart_StartingThing_Defined> enumerable3 = from y in item2.scenario.AllParts
				where y is ScenPart_StartingThing_Defined && ((ScenPart_ThingCount)(ScenPart_StartingThing_Defined)y).thingDef is AmmoDef
				select y into x
				select x as ScenPart_StartingThing_Defined;
			foreach (ScenPart_StartingThing_Defined item3 in enumerable3)
			{
				AmmoDef ammodef = (AmmoDef)((ScenPart_ThingCount)item3).thingDef;
				AmmoDef ammoDef = ammodef.AmmoSetDefs?.FirstOrFallback()?.ammoTypes?.Find((AmmoLink x) => x.ammo.ammoClass == ammodef.ammoClass)?.ammo ?? null;
				if (ammoDef != null)
				{
					((ScenPart_ThingCount)item3).thingDef = ammoDef;
				}
			}
		}
		IEnumerable<ThingDef> enumerable4 = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.comps?.Any((CompProperties x) => x is CompProperties_ApparelReloadable) ?? false);
		foreach (ThingDef item4 in enumerable4)
		{
			CompProperties_ApparelReloadable compProperties = item4.GetCompProperties<CompProperties_ApparelReloadable>();
			ThingDef ammoDef2 = compProperties.ammoDef;
			AmmoDef am = ammoDef2 as AmmoDef;
			if (am == null)
			{
				continue;
			}
			if (am.AmmoSetDefs.NullOrEmpty())
			{
				Log.Warning($"Apparel {item4} has CE AmmoDef but with no AmmoSetDef");
				continue;
			}
			AmmoSetDef ammoSetDef = am.AmmoSetDefs.Find((AmmoSetDef x) => x.similarTo == null);
			if (ammoSetDef != null)
			{
				Log.Message(item4.label + " switching to " + ammoSetDef.label);
				AmmoDef ammoDef3 = ammoSetDef.ammoTypes.Find((AmmoLink x) => (x.ammo?.ammoClass ?? null) == am.ammoClass)?.ammo ?? null;
				if (ammoDef3 == null)
				{
					ammoDef3 = ammoSetDef.ammoTypes[0].ammo;
				}
				compProperties.ammoDef = ammoDef3;
			}
			else if (!am.AmmoSetDefs.NullOrEmpty())
			{
				Log.Warning($"Apparel {item4} has CE AmmoDef {am.AmmoSetDefs[0]} but no similarTo tag");
			}
			else
			{
				Log.Warning($"Apparel {item4} has CE AmmoDef with no AmmoSetDefs");
			}
		}
	}
}
