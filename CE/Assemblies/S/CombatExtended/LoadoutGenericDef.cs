using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class LoadoutGenericDef : Def
{
	public LoadoutCountType defaultCountType = LoadoutCountType.dropExcess;

	public int defaultCount = 1;

	private Predicate<ThingDef> _lambda = (ThingDef td) => true;

	public ThingRequestGroup thingRequestGroup = ThingRequestGroup.HaulableEver;

	public bool isBasic = false;

	private float _bulk;

	private float _mass;

	private bool _cachedVars = false;

	public Predicate<ThingDef> lambda => _lambda;

	public float bulk
	{
		get
		{
			if (!_cachedVars)
			{
				updateVars();
			}
			return _bulk;
		}
	}

	public float mass
	{
		get
		{
			if (!_cachedVars)
			{
				updateVars();
			}
			return _mass;
		}
	}

	static LoadoutGenericDef()
	{
		IEnumerable<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefs;
		List<LoadoutGenericDef> list = new List<LoadoutGenericDef>();
		HashSet<ushort> takenHashes = new HashSet<ushort>();
		Type typeFromHandle = typeof(LoadoutGenericDef);
		LoadoutGenericDef generic = new LoadoutGenericDef();
		generic.defName = "GenericMeal";
		generic.description = "Generic Loadout for perishable meals.  Intended for compatibility with pawns automatically picking up a meal for themself.";
		generic.label = "CE_Generic_Meal".Translate();
		generic.defaultCountType = LoadoutCountType.pickupDrop;
		generic._lambda = delegate(ThingDef td)
		{
			int result;
			if (td != null && td.IsNutritionGivingIngestible && td.ingestible != null && (int)td.ingestible.preferability >= 7)
			{
				CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
				if (compProperties != null && compProperties.daysToRotStart <= 5f)
				{
					result = ((!td.IsDrug) ? 1 : 0);
					goto IL_0049;
				}
			}
			result = 0;
			goto IL_0049;
			IL_0049:
			return (byte)result != 0;
		};
		generic.isBasic = true;
		ShortHashGiver.GiveShortHash((Def)generic, typeFromHandle, takenHashes);
		list.Add(generic);
		float num = 0.85f;
		generic = new LoadoutGenericDef();
		generic.defName = "GenericRawFood";
		generic.description = "Generic Loadout for Raw Food.  Intended for compatibility with pawns automatically picking up raw food to train animals.";
		generic.label = "CE_Generic_RawFood".Translate();
		generic._lambda = (ThingDef td) => td != null && td.IsNutritionGivingIngestible && td.ingestible != null && (int)td.ingestible.preferability <= 5 && td.ingestible.HumanEdible && td.plant == null && !td.IsDrug && !td.IsCorpse;
		generic.defaultCount = Convert.ToInt32(Math.Floor(num / allDefs.Where((ThingDef td) => generic.lambda(td)).Average((ThingDef td) => td.ingestible.CachedNutrition)));
		generic.isBasic = false;
		ShortHashGiver.GiveShortHash((Def)generic, typeFromHandle, takenHashes);
		list.Add(generic);
		generic = new LoadoutGenericDef();
		generic.defName = "GenericDrugs";
		generic.defaultCount = 3;
		generic.description = "Generic Loadout for Drugs.  Intended for compatibility with pawns automatically picking up drugs in compliance with drug policies.";
		generic.label = "CE_Generic_Drugs".Translate();
		generic.thingRequestGroup = ThingRequestGroup.Drug;
		generic._lambda = (ThingDef td) => td?.IsDrug ?? false;
		generic.isBasic = true;
		ShortHashGiver.GiveShortHash((Def)generic, typeFromHandle, takenHashes);
		list.Add(generic);
		generic = new LoadoutGenericDef();
		generic.defName = "GenericMedicine";
		generic.defaultCount = 5;
		generic.defaultCountType = LoadoutCountType.pickupDrop;
		generic.description = "Generic Loadout for Medicine.  Intended for pawns which will handle triage activities.";
		generic.label = "CE_Generic_Medicine".Translate();
		generic.thingRequestGroup = ThingRequestGroup.Medicine;
		generic._lambda = (ThingDef td) => td?.IsMedicine ?? false;
		generic.isBasic = true;
		ShortHashGiver.GiveShortHash((Def)generic, typeFromHandle, takenHashes);
		list.Add(generic);
		List<ThingDef> list2 = allDefs.Where((ThingDef td) => !td.IsMenuHidden() && td.HasComp(typeof(CompAmmoUser)) && td.GetCompProperties<CompProperties_AmmoUser>() != null && td.Verbs.FirstOrDefault((VerbProperties v) => v is VerbPropertiesCE) != null).ToList();
		string format = "CE_Generic_Ammo".Translate();
		foreach (ThingDef gun in list2)
		{
			if (gun.GetCompProperties<CompProperties_AmmoUser>().ammoSet == null || gun.GetCompProperties<CompProperties_AmmoUser>().ammoSet.ammoTypes.Count <= 0)
			{
				continue;
			}
			generic = new LoadoutGenericDef();
			generic.defName = "GenericAmmo-" + gun.defName;
			generic.description = $"Generic Loadout ammo for {gun.LabelCap}. Intended for generic collection of ammo for given gun.";
			generic.label = string.Format(format, gun.LabelCap);
			generic.defaultCount = gun.GetCompProperties<CompProperties_AmmoUser>().magazineSize;
			generic.defaultCountType = LoadoutCountType.pickupDrop;
			generic.thingRequestGroup = ThingRequestGroup.HaulableEver;
			generic._lambda = (ThingDef td) => td != null && td is AmmoDef && gun.GetCompProperties<CompProperties_AmmoUser>().ammoSet.ammoTypes.Any((AmmoLink al) => al.ammo == td);
			ShortHashGiver.GiveShortHash((Def)generic, typeFromHandle, takenHashes);
			list.Add(generic);
		}
		DefDatabase<LoadoutGenericDef>.Add(list);
		DefDatabase<LoadoutGenericDef>.InitializeShortHashDictionary();
	}

	private void updateVars()
	{
		IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => lambda(td) && thingRequestGroup.Includes(td));
		_bulk = source.Max((ThingDef t) => t.GetStatValueAbstract(CE_StatDefOf.Bulk));
		_mass = source.Max((ThingDef t) => t.GetStatValueAbstract(StatDefOf.Mass));
		_cachedVars = true;
	}
}
