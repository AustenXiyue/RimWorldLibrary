using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public static class Utility_Loadouts
{
	private static float _labelSize = -1f;

	private static float _margin = 6f;

	private static Texture2D _overburdenedTex;

	public static float medianWeightCapacity = 0f;

	public static float medianBulkCapacity = 0f;

	public static float LabelSize
	{
		get
		{
			if (_labelSize < 0f)
			{
				_labelSize = _margin + Math.Max(Text.CalcSize("CE_Weight".Translate()).x, Text.CalcSize("CE_Bulk".Translate()).x);
			}
			return _labelSize;
		}
	}

	public static Texture2D OverburdenedTex
	{
		get
		{
			if (_overburdenedTex == null)
			{
				_overburdenedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
			}
			return _overburdenedTex;
		}
	}

	public static void DrawBar(Rect canvas, float current, float capacity, string label = "", string tooltip = "")
	{
		Rect rect = new Rect(canvas);
		Rect rect2 = new Rect(canvas);
		if (label != "")
		{
			rect2.xMin += LabelSize;
		}
		rect.width = LabelSize;
		if (label != "")
		{
			Widgets.Label(rect, label);
		}
		bool flag = current > capacity;
		float fillPercent = (flag ? 1f : (float.IsNaN(current / capacity) ? 1f : (current / capacity)));
		if (flag)
		{
			Widgets.FillableBar(rect2, fillPercent, OverburdenedTex);
			DrawBarThreshold(rect2, capacity / current);
		}
		else
		{
			Widgets.FillableBar(rect2, fillPercent);
		}
		if (tooltip != "")
		{
			TooltipHandler.TipRegion(canvas, tooltip);
		}
	}

	public static void DrawBarThreshold(Rect barRect, float pct, float curLevel = 1f)
	{
		float num = ((barRect.width <= 60f) ? 1 : 2);
		Rect position = new Rect(barRect.x + barRect.width * pct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
		Texture2D image;
		if (pct < curLevel)
		{
			image = BaseContent.BlackTex;
			GUI.color = new Color(1f, 1f, 1f, 0.9f);
		}
		else
		{
			image = BaseContent.GreyTex;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
		}
		GUI.DrawTexture(position, image);
		GUI.color = Color.white;
	}

	public static string GetBulkTip(this Loadout loadout)
	{
		float f = MassBulkUtility.WorkSpeedFactor(loadout.Bulk, medianBulkCapacity);
		return "CE_DetailedBaseBulkTip".Translate(CE_StatDefOf.CarryBulk.ValueToString(medianBulkCapacity, CE_StatDefOf.CarryBulk.toStringNumberSense), CE_StatDefOf.CarryBulk.ValueToString(loadout.Bulk, CE_StatDefOf.CarryBulk.toStringNumberSense), f.ToStringPercent());
	}

	public static string GetBulkTip(this Pawn pawn)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null)
		{
			return "CE_DetailedBulkTip".Translate(CE_StatDefOf.CarryBulk.ValueToString(compInventory.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense), CE_StatDefOf.CarryBulk.ValueToString(compInventory.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense), compInventory.workSpeedFactor.ToStringPercent());
		}
		return string.Empty;
	}

	public static string GetBulkTip(this Thing thing, int count = 1)
	{
		return "CE_Bulk".Translate() + ": " + CE_StatDefOf.Bulk.ValueToString(thing.GetStatValue(CE_StatDefOf.Bulk) * (float)count, CE_StatDefOf.Bulk.toStringNumberSense);
	}

	public static string GetBulkTip(this ThingDef def, int count = 1)
	{
		return "CE_Bulk".Translate() + ": " + CE_StatDefOf.Bulk.ValueToString(def.GetStatValueAbstract(CE_StatDefOf.Bulk) * (float)count, CE_StatDefOf.Bulk.toStringNumberSense);
	}

	public static Loadout GetLoadout(this Pawn pawn)
	{
		if (pawn == null)
		{
			throw new ArgumentNullException("pawn");
		}
		if (!LoadoutManager.AssignedLoadouts.TryGetValue(pawn, out var value))
		{
			LoadoutManager.AssignedLoadouts.Add(pawn, LoadoutManager.DefaultLoadout);
			return LoadoutManager.DefaultLoadout;
		}
		return value;
	}

	public static int GetLoadoutId(this Pawn pawn)
	{
		return pawn.GetLoadout().uniqueID;
	}

	public static string GetWeightAndBulkTip(this Loadout loadout)
	{
		return loadout.GetWeightTip() + "\n\n" + loadout.GetBulkTip();
	}

	public static string GetWeightAndBulkTip(this Pawn pawn)
	{
		return pawn.GetWeightTip() + "\n\n" + pawn.GetBulkTip();
	}

	public static string GetWeightAndBulkTip(this Thing thing)
	{
		return thing.GetWeightTip(thing.stackCount) + "\n" + thing.GetBulkTip(thing.stackCount);
	}

	public static string GetWeightAndBulkTip(this ThingDef def, int count = 1)
	{
		return def.LabelCap + ((count != 1) ? (" x" + count) : "") + "\n" + def.GetWeightTip(count) + "\n" + def.GetBulkTip(count);
	}

	public static string GetWeightAndBulkTip(this LoadoutGenericDef def, int count = 1)
	{
		return "CE_Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(def.mass * (float)count, StatDefOf.Mass.toStringNumberSense) + "\n" + "CE_Bulk".Translate() + ": " + CE_StatDefOf.Bulk.ValueToString(def.bulk * (float)count, CE_StatDefOf.Bulk.toStringNumberSense);
	}

	public static string GetWeightTip(this ThingDef def, int count = 1)
	{
		return "CE_Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(def.GetStatValueAbstract(StatDefOf.Mass) * (float)count, StatDefOf.Mass.toStringNumberSense);
	}

	public static string GetWeightTip(this Thing thing, int count = 1)
	{
		return "CE_Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(thing.GetStatValue(StatDefOf.Mass) * (float)count, StatDefOf.Mass.toStringNumberSense);
	}

	public static string GetWeightTip(this Loadout loadout)
	{
		float f = MassBulkUtility.MoveSpeedFactor(loadout.Weight, medianWeightCapacity);
		float f2 = MassBulkUtility.EncumberPenalty(loadout.Weight, medianWeightCapacity);
		return "CE_DetailedBaseWeightTip".Translate(CE_StatDefOf.CarryWeight.ValueToString(medianWeightCapacity, CE_StatDefOf.CarryWeight.toStringNumberSense), CE_StatDefOf.CarryWeight.ValueToString(loadout.Weight, CE_StatDefOf.CarryWeight.toStringNumberSense), f.ToStringPercent(), f2.ToStringPercent());
	}

	public static string GetWeightTip(this Pawn pawn)
	{
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory != null)
		{
			return "CE_DetailedWeightTip".Translate(CE_StatDefOf.CarryWeight.ValueToString(compInventory.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense), CE_StatDefOf.CarryWeight.ValueToString(compInventory.currentWeight, CE_StatDefOf.CarryWeight.toStringNumberSense), compInventory.moveSpeedFactor.ToStringPercent(), compInventory.encumberPenalty.ToStringPercent());
		}
		return "";
	}

	public static void SetLoadout(this Pawn pawn, Loadout loadout)
	{
		if (pawn == null)
		{
			throw new ArgumentNullException("pawn");
		}
		if (LoadoutManager.AssignedLoadouts.ContainsKey(pawn))
		{
			LoadoutManager.AssignedLoadouts[pawn] = loadout;
		}
		else
		{
			LoadoutManager.AssignedLoadouts.Add(pawn, loadout);
		}
	}

	public static void SetLoadoutById(this Pawn pawn, int loadoutId)
	{
		Loadout loadoutById = LoadoutManager.GetLoadoutById(loadoutId);
		if (loadoutById == null)
		{
			throw new ArgumentNullException("loadout");
		}
		pawn.SetLoadout(loadoutById);
	}

	public static void UpdateColonistCapacities()
	{
		Pawn[] array = PawnsFinder.AllMaps_FreeColonists.ToArray();
		if (array.Length != 0)
		{
			medianWeightCapacity = Median(array.Select((Pawn c) => c.GetStatValue(CE_StatDefOf.CarryWeight)).ToArray());
			medianBulkCapacity = Median(array.Select((Pawn c) => c.GetStatValue(CE_StatDefOf.CarryBulk)).ToArray());
		}
		else
		{
			medianWeightCapacity = ThingDefOf.Human.GetStatValueAbstract(CE_StatDefOf.CarryWeight);
			medianBulkCapacity = ThingDefOf.Human.GetStatValueAbstract(CE_StatDefOf.CarryBulk);
		}
	}

	private static float Median(float[] xs)
	{
		float[] array = xs.OrderBy((float x) => x).ToArray();
		if (array.Length == 0)
		{
			Log.Error("Combat Extended :: Utility_Loadouts :: Median: Nonzero-length array");
			return 0f;
		}
		if (array.Length % 2 == 0)
		{
			return (array[array.Length / 2 - 1] + array[array.Length / 2]) / 2f;
		}
		return array[Mathf.FloorToInt((float)array.Length / 2f)];
	}

	public static Loadout GenerateLoadoutFromPawn(this Pawn pawn)
	{
		string text = pawn.Name.ToStringShort + " " + "CE_DefaultLoadoutName".Translate();
		Regex regex = new Regex("^(.*?)\\d+$");
		if (regex.IsMatch(text))
		{
			text = regex.Replace(text, "$1");
		}
		text = LoadoutManager.GetUniqueLabel(text);
		Loadout loadout = new Loadout(text);
		loadout.defaultLoadout = false;
		loadout.canBeDeleted = true;
		LoadoutSlot loadoutSlot = null;
		if (pawn.equipment?.Primary != null)
		{
			loadoutSlot = new LoadoutSlot(pawn.equipment.Primary.def);
			loadout.AddSlot(loadoutSlot);
		}
		IEnumerable<LoadoutGenericDef> enumerable = DefDatabase<LoadoutGenericDef>.AllDefs.Where((LoadoutGenericDef gd) => gd.defaultCountType == LoadoutCountType.dropExcess);
		foreach (Thing item in pawn.inventory.innerContainer)
		{
			LoadoutGenericDef loadoutGenericDef = null;
			foreach (LoadoutGenericDef item2 in enumerable)
			{
				if (item2.lambda(item.def))
				{
					loadoutGenericDef = item2;
					break;
				}
			}
			loadoutSlot = ((loadoutGenericDef == null) ? new LoadoutSlot(item.def, item.stackCount) : new LoadoutSlot(loadoutGenericDef, item.stackCount));
			loadout.AddSlot(loadoutSlot);
		}
		foreach (LoadoutGenericDef generic in enumerable.Where((LoadoutGenericDef gd) => gd.isBasic))
		{
			loadoutSlot = loadout.Slots.FirstOrDefault((LoadoutSlot s) => s.genericDef == generic);
			if (loadoutSlot != null)
			{
				if (loadoutSlot.count < loadoutSlot.genericDef.defaultCount)
				{
					loadoutSlot.count = loadoutSlot.genericDef.defaultCount;
				}
			}
			else
			{
				loadoutSlot = new LoadoutSlot(generic);
				loadout.AddSlot(loadoutSlot);
			}
		}
		return loadout;
	}

	public static bool IsItemQuestLocked(this Pawn pawn, Thing thing)
	{
		if (pawn == null || thing == null)
		{
			return false;
		}
		int result;
		if (thing is Apparel apparel)
		{
			Pawn_ApparelTracker apparel2 = pawn.apparel;
			if (apparel2 != null && apparel2.IsLocked(apparel))
			{
				result = 1;
				goto IL_0058;
			}
		}
		result = ((thing.def.IsWeapon && pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanUnequip(thing, pawn)) ? 1 : 0);
		goto IL_0058;
		IL_0058:
		return (byte)result != 0;
	}

	public static bool IsItemMechanoidWeapon(this Pawn pawn, Thing thing)
	{
		if (pawn == null || thing == null)
		{
			return false;
		}
		return pawn.RaceProps.IsMechanoid && thing.def.IsWeapon;
	}
}
