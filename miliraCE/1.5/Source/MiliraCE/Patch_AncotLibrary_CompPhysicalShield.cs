using AncotLibrary;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace MiliraCE
{
    [HarmonyPatch(typeof(CompPhysicalShield))]
    [HarmonyPatch(nameof(CompPhysicalShield.CompAllowVerbCast))]
    public static class Patch_AncotLibrary_CompPhysicalShield_CompAllowVerbCast
    {
        internal static bool Prefix(ref bool __result, Verb verb, CompPhysicalShield __instance)
        {
            if (__instance.Props.blocksRangedWeapons)
            {
                __result = __instance.ShieldState != An_ShieldState.Active || verb is Verb_MarkForArtillery || (!(verb is Verb_LaunchProjectileCE) && !(verb is Verb_LaunchProjectile));
            }
            else
            {
                __result = true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CompPhysicalShield))]
    [HarmonyPatch("get_staminaConsumeFactor")]
    public static class Patch_AncotLibrary_CompPhysicalShield_get_staminaConsumeFactor
    {
        private static readonly SimpleCurve ArmorRatingToStaminaConsumeFactor = new SimpleCurve
        {
            new CurvePoint(0f, 2f),
            new CurvePoint(6f, 1.4f),
            new CurvePoint(24f, 0.5f),
            new CurvePoint(999f, 0.5f)
        };

        internal static bool Prefix(ref float __result, CompPhysicalShield __instance)
        {
            if (__instance.parent is Apparel thing)
            {
                __result = ArmorRatingToStaminaConsumeFactor.Evaluate(thing.GetStatValue(StatDefOf.ArmorRating_Sharp));
            }
            else
            {
                __result = 1f;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CompPhysicalShield))]
    [HarmonyPatch("BlockByShield")]
    public static class Patch_AncotLibrary_CompPhysicalShield_BlockByShield
    {
        private static readonly SimpleCurve ArmorPenetrationSharpNormalizationCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(40f, 1f),
            new CurvePoint(999f, 1f)
        };

        private static readonly SimpleCurve ArmorPenetrationBluntNormalizationCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(160f, 1f),
            new CurvePoint(999f, 1f)
        };

        delegate void ActionArmorPenetrationIntSetter(ref DamageInfo dinfo, float value);
        private static ActionArmorPenetrationIntSetter armorPenetrationIntSetter =
            PatchUtils.BuildSetterExpression<ActionArmorPenetrationIntSetter>(
                typeof(DamageInfo).MakeByRefType(), // Need to pass a ref type into setter, or we will just set the value to some temporary copy
                typeof(DamageInfo).GetField("armorPenetrationInt", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(float)
                );


        internal static void Prefix(ref DamageInfo dinfo, out float? __state)
        {
            if (dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp)
            {
                __state = dinfo.ArmorPenetrationInt;
                armorPenetrationIntSetter(ref dinfo, ArmorPenetrationSharpNormalizationCurve.Evaluate(dinfo.ArmorPenetrationInt));
            }
            else if (dinfo.Def.armorCategory == CE_DamageArmorCategoryDefOf.Blunt)
            {
                __state = dinfo.ArmorPenetrationInt;
                armorPenetrationIntSetter(ref dinfo, ArmorPenetrationBluntNormalizationCurve.Evaluate(dinfo.ArmorPenetrationInt));
            }
            else
            {
                __state = null;
            }
        }

        internal static void Postfix(ref DamageInfo dinfo, float? __state)
        {
            if (__state is float oldArmorPenetrationInt)
            {
                armorPenetrationIntSetter(ref dinfo, oldArmorPenetrationInt);
            }
        }
    }
}
