using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public static class AmmoInjector
{
	public const string destroyWithAmmoDisabledTag = "CE_AmmoInjector";

	private const string enableTradeTag = "CE_AutoEnableTrade";

	private const string enableCraftingTag = "CE_AutoEnableCrafting";

	public static bool gunRecipesShowCaliber;

	static AmmoInjector()
	{
		gunRecipesShowCaliber = false;
		Inject();
		AddRemoveCaliberFromGunRecipes();
	}

	public static void Inject()
	{
		if (InjectAmmos())
		{
			Log.Message("Combat Extended :: Ammo " + (Controller.settings.EnableAmmoSystem ? "injected" : "removed"));
		}
		else
		{
			Log.Error("Combat Extended :: Ammo injector failed to get injected");
		}
		ThingSetMakerUtility.Reset();
	}

	public static bool InjectAmmos()
	{
		bool enableAmmoSystem = Controller.settings.EnableAmmoSystem;
		CE_Utility.allWeaponDefs.Clear();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
		{
			if (item.IsWeapon && (item.generateAllowChance > 0f || item.tradeability.TraderCanSell() || (item.weaponTags != null && item.weaponTags.Contains("TurretGun"))))
			{
				CE_Utility.allWeaponDefs.Add(item);
			}
		}
		if (CE_Utility.allWeaponDefs.NullOrEmpty())
		{
			Log.Warning("CE Ammo Injector found no weapon defs");
			return true;
		}
		AddRemoveCaliberFromGunRecipes();
		HashSet<ThingDef> hashSet = new HashSet<ThingDef>();
		foreach (ThingDef allWeaponDef in CE_Utility.allWeaponDefs)
		{
			CompProperties_AmmoUser compProperties = allWeaponDef.GetCompProperties<CompProperties_AmmoUser>();
			if (compProperties != null && compProperties.ammoSet != null && !compProperties.ammoSet.ammoTypes.NullOrEmpty())
			{
				hashSet.UnionWith(((IEnumerable<AmmoLink>)compProperties.ammoSet.ammoTypes).Select((Func<AmmoLink, ThingDef>)((AmmoLink x) => x.ammo)));
			}
			CompProperties_UnderBarrel compProperties2 = allWeaponDef.GetCompProperties<CompProperties_UnderBarrel>();
			if (compProperties2?.propsUnderBarrel != null && compProperties2.propsUnderBarrel.ammoSet != null && !compProperties2.propsUnderBarrel.ammoSet.ammoTypes.NullOrEmpty())
			{
				hashSet.UnionWith(((IEnumerable<AmmoLink>)compProperties2.propsUnderBarrel.ammoSet.ammoTypes).Select((Func<AmmoLink, ThingDef>)((AmmoLink x) => x.ammo)));
			}
		}
		hashSet.UnionWith(Patches.GetUsedAmmo());
		foreach (AmmoDef item2 in hashSet)
		{
			item2.AddDescriptionParts();
			bool flag = enableAmmoSystem || item2.isMortarAmmo;
			if (item2.tradeTags == null)
			{
				continue;
			}
			if (item2.tradeTags.Contains("CE_AmmoInjector"))
			{
				item2.menuHidden = !flag;
				item2.destroyOnDrop = !flag;
			}
			if (item2.IsWeapon)
			{
				continue;
			}
			IEnumerable<string> source = item2.tradeTags.Where((string t) => t.StartsWith("CE_AutoEnableTrade"));
			if (source.Any())
			{
				string text = source.First();
				if (text == "CE_AutoEnableTrade")
				{
					item2.tradeability = (flag ? Tradeability.All : Tradeability.None);
				}
				else if (text.Length <= "CE_AutoEnableTrade".Length + 1)
				{
					Log.Error("Combat Extended :: AmmoInjector trying to inject " + item2.ToString() + " but " + text + " is not a valid trading tag, valid formats are: CE_AutoEnableTrade and CE_AutoEnableTrade_levelOfTradeability");
				}
				else
				{
					string value = text.Remove(0, "CE_AutoEnableTrade".Length + 1);
					item2.tradeability = (flag ? ((Tradeability)Enum.Parse(typeof(Tradeability), value, ignoreCase: true)) : Tradeability.None);
				}
			}
			IEnumerable<string> enumerable = item2.tradeTags.Where((string t) => t.StartsWith("CE_AutoEnableCrafting"));
			if (!enumerable.Any())
			{
				continue;
			}
			RecipeDef named = DefDatabase<RecipeDef>.GetNamed("Make" + item2.defName, errorOnFail: false);
			if (named == null)
			{
				Log.Error("CE ammo injector found no recipe named Make" + item2.defName);
				continue;
			}
			foreach (string item3 in enumerable)
			{
				ThingDef thingDef = null;
				ThingDef thingDef2;
				if (item3 == "CE_AutoEnableCrafting")
				{
					thingDef2 = CE_ThingDefOf.AmmoBench;
				}
				else
				{
					if (item3.Length <= "CE_AutoEnableCrafting".Length + 1)
					{
						Log.Error("Combat Extended :: AmmoInjector trying to inject " + item2.ToString() + " but " + item3 + " is not a valid crafting tag, valid formats are: CE_AutoEnableCrafting and CE_AutoEnableCrafting_defNameOfCraftingBench");
						continue;
					}
					string text2 = item3.Remove(0, "CE_AutoEnableCrafting".Length + 1);
					thingDef2 = DefDatabase<ThingDef>.GetNamed(text2, errorOnFail: false);
					if (thingDef2 == null)
					{
						Log.Error("Combat Extended :: AmmoInjector trying to inject " + item2.ToString() + " but no crafting bench with defName=" + text2 + " could be found for tag " + item3);
						continue;
					}
					if (ModLister.HasActiveModWithName("Vanilla Furniture Expanded - Production"))
					{
						string text3 = null;
						if (item3 == "CE_AutoEnableCrafting_ElectricSmithy" || item3 == "CE_AutoEnableCrafting_FueledSmithy")
						{
							text3 = "VFE_TableSmithyLarge";
						}
						if (item3 == "CE_AutoEnableCrafting_DrugLab")
						{
							text3 = "VFE_TableDrugLabElectric";
						}
						if (item3 == "CE_AutoEnableCrafting_TableMachining")
						{
							text3 = "VFE_TableMachiningLarge";
						}
						if (text3 != null)
						{
							thingDef = DefDatabase<ThingDef>.GetNamed(text3, errorOnFail: false);
							if (thingDef == null)
							{
								Log.Error("Combat Extended :: AmmoInjector trying to inject " + item2.ToString() + " but no VFE crafting bench with defName=" + text3 + " could be found for tag " + item3);
								continue;
							}
						}
					}
				}
				ToggleRecipeOnBench(named, thingDef2, flag);
				if (thingDef != null)
				{
					ToggleRecipeOnBench(named, thingDef, flag);
				}
			}
		}
		return true;
	}

	private static void ToggleRecipeOnBench(RecipeDef recipeDef, ThingDef benchDef, bool ammoEnabled)
	{
		if (ammoEnabled)
		{
			if (recipeDef.recipeUsers == null)
			{
				recipeDef.recipeUsers = new List<ThingDef>();
			}
			recipeDef.recipeUsers.Add(benchDef);
		}
		else
		{
			recipeDef.recipeUsers?.RemoveAll((ThingDef x) => x.defName == benchDef.defName);
			if (Current.Game != null)
			{
				IEnumerable<Building> enumerable = Find.Maps.SelectMany((Map x) => x.listerBuildings.AllBuildingsColonistOfDef(benchDef));
				foreach (Building item in enumerable)
				{
					if (!(item is IBillGiver billGiver))
					{
						continue;
					}
					for (int i = 0; i < billGiver.BillStack.Count; i++)
					{
						Bill bill = billGiver.BillStack[i];
						if (!benchDef.AllRecipes.Exists((RecipeDef r) => bill.recipe == r))
						{
							billGiver.BillStack.Delete(bill);
						}
					}
				}
			}
		}
		benchDef.allRecipesCached = null;
	}

	public static void AddRemoveCaliberFromGunRecipes()
	{
		bool shouldHaveLabels = Controller.settings.EnableAmmoSystem && Controller.settings.ShowCaliberOnGuns;
		if (gunRecipesShowCaliber == shouldHaveLabels)
		{
			return;
		}
		CE_Utility.allWeaponDefs.ForEach(delegate(ThingDef x)
		{
			AmmoSetDef ammoSetDef = x.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet;
			if (ammoSetDef != null)
			{
				RecipeDef named = DefDatabase<RecipeDef>.GetNamed("Make" + x.defName, errorOnFail: false);
				if (named != null)
				{
					string text = x.label + (shouldHaveLabels ? string.Concat(" (", ammoSetDef.LabelCap, ")") : "");
					named.UpdateLabel("RecipeMake".Translate(text));
					named.jobString = "RecipeMakeJobString".Translate(text);
				}
			}
		});
		gunRecipesShowCaliber = shouldHaveLabels;
	}
}
