using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using Verse;

namespace RimWorld;

public static class PawnWeaponGenerator
{
	private static List<ThingStuffPair> allWeaponPairs;

	private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

	private const float WeaponSelectFactor_NobleByIdeo = 100f;

	private const float WeaponSelectFactor_DespisedByIdeo = 0.001f;

	public static void Reset()
	{
		Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty();
		allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
		foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => isWeapon(td)))
		{
			float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
			if (num == 0f)
			{
				continue;
			}
			float num2 = thingDef.generateCommonality / num;
			if (num2 == 1f)
			{
				continue;
			}
			for (int i = 0; i < allWeaponPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = allWeaponPairs[i];
				if (thingStuffPair.thing == thingDef)
				{
					allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
				}
			}
		}
	}

	public static void TryGenerateWeaponFor(Pawn pawn, PawnGenerationRequest request)
	{
		workingWeapons.Clear();
		if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0 || !pawn.RaceProps.ToolUser || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return;
		}
		float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
		for (int i = 0; i < allWeaponPairs.Count; i++)
		{
			ThingStuffPair w2 = allWeaponPairs[i];
			if (!(w2.Price > randomInRange) && (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Any((string tag) => w2.thing.weaponTags.Contains(tag))) && (pawn.kindDef.weaponStuffOverride == null || w2.stuff == pawn.kindDef.weaponStuffOverride) && (!w2.thing.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) && (!(w2.thing.generateAllowChance < 1f) || Rand.ChanceSeeded(w2.thing.generateAllowChance, pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
			{
				workingWeapons.Add(w2);
			}
		}
		if (workingWeapons.Count == 0)
		{
			return;
		}
		pawn.equipment.DestroyAllEquipment();
		if (workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => GetCommonality(pawn, w), out var result))
		{
			ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
			PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
			CompEquippable compEquippable = thingWithComps.TryGetComp<CompEquippable>();
			if (compEquippable != null)
			{
				if (pawn.kindDef.weaponStyleDef != null)
				{
					compEquippable.parent.StyleDef = pawn.kindDef.weaponStyleDef;
				}
				else if (pawn.Ideo != null)
				{
					compEquippable.parent.StyleDef = pawn.Ideo.GetStyleFor(thingWithComps.def);
				}
			}
			float num = ((request.BiocodeWeaponChance > 0f) ? request.BiocodeWeaponChance : pawn.kindDef.biocodeWeaponChance);
			if (Rand.Value < num)
			{
				thingWithComps.TryGetComp<CompBiocodable>()?.CodeFor(pawn);
			}
			pawn.equipment.AddEquipment(thingWithComps);
		}
		workingWeapons.Clear();
	}

	private static float GetCommonality(Pawn pawn, ThingStuffPair pair)
	{
		return pair.Commonality * pair.Price * GetWeaponCommonalityFromIdeo(pawn, pair) * GetWeaponCommonalityFromXenotype(pawn, pair);
	}

	private static float GetWeaponCommonalityFromIdeo(Pawn pawn, ThingStuffPair pair)
	{
		if (pawn.Ideo == null)
		{
			return 1f;
		}
		return pawn.Ideo.GetDispositionForWeapon(pair.thing) switch
		{
			IdeoWeaponDisposition.Noble => 100f, 
			IdeoWeaponDisposition.Despised => 0.001f, 
			_ => 1f, 
		};
	}

	private static float GetWeaponCommonalityFromXenotype(Pawn pawn, ThingStuffPair pair)
	{
		if (pawn.genes?.Xenotype?.forbiddenWeaponClasses != null && !pair.thing.weaponClasses.NullOrEmpty())
		{
			foreach (WeaponClassDef forbiddenWeaponClass in pawn.genes.Xenotype.forbiddenWeaponClasses)
			{
				if (pair.thing.weaponClasses.Contains(forbiddenWeaponClass))
				{
					return 0f;
				}
			}
		}
		return 1f;
	}

	public static bool IsDerpWeapon(ThingDef thing, ThingDef stuff)
	{
		if (stuff == null)
		{
			return false;
		}
		if (thing.IsMeleeWeapon)
		{
			if (thing.tools.NullOrEmpty())
			{
				return false;
			}
			DamageDef damageDef = ThingUtility.PrimaryMeleeWeaponDamageType(thing);
			if (damageDef == null)
			{
				return false;
			}
			DamageArmorCategoryDef armorCategory = damageDef.armorCategory;
			if (armorCategory != null && armorCategory.multStat != null && stuff.GetStatValueAbstract(armorCategory.multStat) < 0.7f)
			{
				return true;
			}
		}
		return false;
	}

	public static float CheapestNonDerpPriceFor(ThingDef weaponDef)
	{
		float num = 9999999f;
		for (int i = 0; i < allWeaponPairs.Count; i++)
		{
			ThingStuffPair thingStuffPair = allWeaponPairs[i];
			if (thingStuffPair.thing == weaponDef && !IsDerpWeapon(thingStuffPair.thing, thingStuffPair.stuff) && thingStuffPair.Price < num)
			{
				num = thingStuffPair.Price;
			}
		}
		return num;
	}

	[DebugOutput]
	private static void WeaponPairs()
	{
		DebugTables.MakeTablesDialog(allWeaponPairs.OrderByDescending((ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? "" : p.stuff.defName), new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString()), new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => p.Commonality.ToString("F5")), new TableDataGetter<ThingStuffPair>("commMult", (ThingStuffPair p) => p.commonalityMultiplier.ToString("F5")), new TableDataGetter<ThingStuffPair>("generateCommonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F2")), new TableDataGetter<ThingStuffPair>("derp", (ThingStuffPair p) => (!IsDerpWeapon(p.thing, p.stuff)) ? "" : "D"));
	}

	[DebugOutput]
	private static void WeaponPairsByThing()
	{
		DebugOutputsGeneral.MakeTablePairsByThing(allWeaponPairs);
	}
}
