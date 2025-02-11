using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompShield), "PostPreApplyDamage")]
internal static class CompShield_PatchCheckPreAbsorbDamage
{
	internal static bool Prefix(out bool absorbed, DamageInfo dinfo, CompShield __instance)
	{
		absorbed = false;
		if (__instance.ShieldState != 0)
		{
			return false;
		}
		float num = 1f;
		bool flag = dinfo.Def == DamageDefOf.EMP;
		if (dinfo.Weapon?.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			num = projectilePropertiesCE.empShieldBreakChance;
			flag = flag || projectilePropertiesCE.secondaryDamage?.FirstOrDefault((SecondaryDamage sd) => sd.def == DamageDefOf.EMP) != null;
		}
		if (flag && Rand.Chance(num))
		{
			__instance.energy = 0f;
			__instance.Break();
			absorbed = true;
			return false;
		}
		if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
		{
			absorbed = true;
			__instance.energy -= dinfo.Amount * __instance.Props.energyLossPerDamage * (flag ? (1f + num) : 1f);
			if (__instance.energy < 0f)
			{
				__instance.Break();
			}
			else
			{
				__instance.AbsorbedDamage(dinfo);
			}
		}
		return false;
	}
}
