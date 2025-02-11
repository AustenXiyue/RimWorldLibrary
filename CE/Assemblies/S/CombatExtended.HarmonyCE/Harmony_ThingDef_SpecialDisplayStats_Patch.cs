using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
internal static class Harmony_ThingDef_SpecialDisplayStats_Patch
{
	public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
	{
		ApparelDefExtension modExtension = __instance.GetModExtension<ApparelDefExtension>();
		if (modExtension != null && modExtension.isRadioPack)
		{
			__result = __result.Concat(new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "CE_Long_Range_Radio".Translate(), "CE_Yes".Translate(), "CE_Long_Range_Radio_Desc".Translate(), 899));
		}
		ThingDef turretGunDef = __instance.building?.turretGunDef ?? null;
		if (turretGunDef == null)
		{
			return;
		}
		StatRequest statRequestGun = StatRequest.For(turretGunDef, null);
		IEnumerable<StatDrawEntry> cache = __result;
		IEnumerable<StatDrawEntry> second = from x in DefDatabase<StatDef>.AllDefs
			where x.category == StatCategoryDefOf.Weapon && x.Worker.ShouldShowFor(statRequestGun) && !x.Worker.IsDisabledFor(req.Thing) && !(x.Worker is StatWorker_MeleeStats)
			where !cache.Any((StatDrawEntry y) => y.stat == x)
			select new StatDrawEntry(StatCategoryDefOf.Weapon, x, turretGunDef.GetStatValueAbstract(x), statRequestGun, ToStringNumberSense.Undefined, null) into x
			where x.ShouldDisplay()
			select x;
		__result = __result.Concat(second);
	}
}
