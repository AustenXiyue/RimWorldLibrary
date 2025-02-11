using System.Collections;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(DamageWorker), "Apply")]
internal static class Harmony_DamageWorker_Apply
{
	public static bool Prefix(DamageWorker __instance, DamageInfo dinfo, Thing victim)
	{
		if (!Controller.settings.FragmentsFromWalls)
		{
			return true;
		}
		if (victim.def.useHitPoints && dinfo.Def.harmsHealth && dinfo.tool == null && dinfo.Def != DamageDefOf.Mining && victim.def.category == ThingCategory.Building)
		{
			if (dinfo.Def == CE_DamageDefOf.Demolish)
			{
				return true;
			}
			bool flag = dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp;
			Vector3 pos = victim.Position.ToVector3Shifted();
			float amount = dinfo.Amount;
			amount *= dinfo.Def.buildingDamageFactor;
			amount = ((victim.def.passability != Traversability.Impassable) ? (amount * dinfo.Def.buildingDamageFactorPassable) : (amount * dinfo.Def.buildingDamageFactorImpassable));
			float num = victim.HitPoints;
			float num2 = victim.MaxHitPoints;
			bool flag2 = false;
			if (amount > num)
			{
				flag2 = true;
			}
			int num3 = (int)Mathf.Max(amount / 10f, Mathf.Clamp01(amount / num) * amount);
			if (flag)
			{
				num3 /= 2;
			}
			int num4 = num3 / 37;
			int num5 = num3 % 37 / 9;
			num5 += 4 * (num4 / 2 + num4 % 2);
			num4 /= 2;
			FloatRange fragXZAngleRange = new FloatRange(dinfo.Angle + 90f, dinfo.Angle + 270f);
			FloatRange fragXZAngleRange2 = new FloatRange(dinfo.Angle - 60f, dinfo.Angle + 60f);
			Map map = victim.Map;
			float randomInRange = new FloatRange(0f, new CollisionVertical(victim).Max).RandomInRange;
			if (flag2)
			{
				fragXZAngleRange2 = new FloatRange(dinfo.Angle - 90f, dinfo.Angle + 90f);
			}
			if (num5 > 0)
			{
				int num6 = (int)((float)num5 * (num / num2));
				num5 -= num6;
				if (num6 > 0)
				{
					IEnumerator enumerator = CompFragments.FragRoutine(pos, map, randomInRange, victim, new ThingDefCountClass(CE_ThingDefOf.Fragment_Small, num5), 1f, 0.2f, new FloatRange(0.5f, 5f), fragXZAngleRange, 1f, canTargetSelf: false);
					while (enumerator.MoveNext())
					{
					}
				}
				IEnumerator enumerator2 = CompFragments.FragRoutine(pos, map, randomInRange, victim, new ThingDefCountClass(CE_ThingDefOf.Fragment_Small, num5), 1f, 0.2f, new FloatRange(0.5f, 5f), fragXZAngleRange2, 1f, canTargetSelf: false);
				while (enumerator2.MoveNext())
				{
				}
			}
			if (num4 > 0)
			{
				IEnumerator enumerator3 = CompFragments.FragRoutine(pos, map, randomInRange, victim, new ThingDefCountClass(CE_ThingDefOf.Fragment_Large, num4), 1f, 0.2f, new FloatRange(0.5f, 5f), fragXZAngleRange2, 1f, canTargetSelf: false);
				while (enumerator3.MoveNext())
				{
				}
			}
		}
		return true;
	}
}
