using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
internal static class Harmony_ThingDef
{
	private static readonly string[] StatsToCull = new string[3] { "ArmorPenetration", "StoppingPower", "Damage" };

	private const string BurstShotStatName = "BurstShotCount";

	private const string CoverStatName = "CoverEffectiveness";

	private static Type type;

	private static FieldRef<object, VerbProperties> weaponField;

	private static FieldRef<object, ThingDef> thisField;

	private static FieldRef<object, StatDrawEntry> currentField;

	private static MethodBase TargetMethod()
	{
		type = typeof(ThingDef).GetNestedTypes(AccessTools.all).FirstOrDefault((Type x) => x.Name.Contains("<SpecialDisplayStats>"));
		weaponField = AccessTools.FieldRefAccess<VerbProperties>(type, AccessTools.GetFieldNames(type).FirstOrDefault((string x) => x.Contains("<verb>")));
		thisField = AccessTools.FieldRefAccess<ThingDef>(type, AccessTools.GetFieldNames(type).FirstOrDefault((string x) => x.Contains("this")));
		currentField = AccessTools.FieldRefAccess<StatDrawEntry>(type, AccessTools.GetFieldNames(type).FirstOrDefault((string x) => x.Contains("current")));
		return AccessTools.Method(type, "MoveNext", (Type[])null, (Type[])null);
	}

	public static void Postfix(IEnumerator<StatDrawEntry> __instance, ref bool __result)
	{
		if (!__result)
		{
			return;
		}
		StatDrawEntry current = __instance.Current;
		if (current.LabelCap.Contains("BurstShotCount".Translate().CapitalizeFirst()))
		{
			ThingDef thingDef = thisField.Invoke((object)__instance);
			CompProperties_FireModes compProperties = thingDef.GetCompProperties<CompProperties_FireModes>();
			if (compProperties != null)
			{
				int aimedBurstShotCount = compProperties.aimedBurstShotCount;
				int burstShotCount = weaponField.Invoke((object)__instance).burstShotCount;
				if (aimedBurstShotCount != burstShotCount)
				{
					current.valueStringInt = $"{aimedBurstShotCount} / {burstShotCount}";
				}
			}
		}
		else if (current.LabelCap.Contains("CoverEffectiveness".Translate().CapitalizeFirst()))
		{
			ThingDef thingDef2 = thisField.Invoke((object)__instance);
			PlantProperties plant = thingDef2.plant;
			if (plant == null || !plant.IsTree)
			{
				float num = ((thingDef2.Fillage == FillCategory.Full) ? 2f : thingDef2.fillPercent);
				num *= 1.75f;
				StatDrawEntry statDrawEntry = new StatDrawEntry(current.category, "CE_CoverHeight".Translate(), num.ToStringByStyle(ToStringStyle.FloatMaxTwo) + " m", "CE_CoverHeightExplanation".Translate(), current.DisplayPriorityWithinCategory);
				currentField.Invoke((object)__instance) = statDrawEntry;
			}
		}
		else if (StatsToCull.Select((string s) => s.Translate().CapitalizeFirst()).Contains(current.LabelCap))
		{
			__result = __instance.MoveNext();
		}
	}
}
