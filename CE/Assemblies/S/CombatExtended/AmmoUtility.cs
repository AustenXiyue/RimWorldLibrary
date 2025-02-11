using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public static class AmmoUtility
{
	private const float ExplosiveArmorPenetrationMultiplier = 0.4f;

	public static string GetProjectileReadout(this ThingDef projectileDef, Thing weapon)
	{
		if (!(projectileDef?.projectile is ProjectilePropertiesCE projectilePropertiesCE))
		{
			Log.Warning("CE tried getting projectile readout with null props");
			return "CE_UnpatchedWeaponShort".Translate();
		}
		float num = weapon?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
		StringBuilder stringBuilder = new StringBuilder();
		TaggedString taggedString = "   " + "CE_DescDamage".Translate() + ": ";
		if (!projectilePropertiesCE.secondaryDamage.NullOrEmpty())
		{
			stringBuilder.AppendLine(taggedString);
			stringBuilder.AppendLine("   " + GenText.ToStringByStyle(projectilePropertiesCE.GetDamageAmount(weapon), ToStringStyle.Integer) + " (" + projectilePropertiesCE.damageDef.LabelCap + ")");
			foreach (SecondaryDamage item in projectilePropertiesCE.secondaryDamage)
			{
				string arg = ((item.chance >= 1f) ? "" : string.Format("({0} {1})", item.chance.ToStringByStyle(ToStringStyle.PercentZero), "CE_Chance".Translate()));
				stringBuilder.AppendLine($"   {GenText.ToStringByStyle(item.amount, ToStringStyle.Integer)} ({item.def.LabelCap}) {arg}");
			}
		}
		else
		{
			stringBuilder.AppendLine(taggedString + GenText.ToStringByStyle(projectilePropertiesCE.GetDamageAmount(weapon), ToStringStyle.Integer) + " (" + projectilePropertiesCE.damageDef.LabelCap + ")");
		}
		if (projectilePropertiesCE.explosionRadius > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_DescExplosionRadius".Translate() + ": " + projectilePropertiesCE.explosionRadius.ToStringByStyle(ToStringStyle.FloatOne));
		}
		if ((projectilePropertiesCE.damageDef.armorCategory == CE_DamageArmorCategoryDefOf.Heat || projectilePropertiesCE.damageDef.armorCategory == CE_DamageArmorCategoryDefOf.Electric) && projectilePropertiesCE.damageDef.defaultArmorPenetration > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_DescAmbientPenetration".Translate() + ": " + projectilePropertiesCE.damageDef.defaultArmorPenetration.ToStringByStyle(ToStringStyle.PercentZero));
		}
		if (projectilePropertiesCE.damageDef.armorCategory != CE_DamageArmorCategoryDefOf.Heat && projectilePropertiesCE.damageDef.armorCategory != CE_DamageArmorCategoryDefOf.Electric && projectilePropertiesCE.damageDef != DamageDefOf.Stun && projectilePropertiesCE.damageDef != DamageDefOf.Extinguish && projectilePropertiesCE.damageDef != DamageDefOf.Smoke && projectilePropertiesCE.GetDamageAmount(weapon) != 0)
		{
			if (projectilePropertiesCE.explosionRadius > 0f)
			{
				stringBuilder.AppendLine(string.Concat("   " + "CE_DescBluntPenetration".Translate() + ": ", projectilePropertiesCE.GetExplosionArmorPenetration().ToString(), " ") + "CE_MPa".Translate());
			}
			else
			{
				stringBuilder.AppendLine("   " + "CE_DescSharpPenetration".Translate() + ": " + (projectilePropertiesCE.armorPenetrationSharp * num).ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_mmRHA".Translate());
				stringBuilder.AppendLine("   " + "CE_DescBluntPenetration".Translate() + ": " + (projectilePropertiesCE.armorPenetrationBlunt * num).ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_MPa".Translate());
			}
		}
		CompProperties_ExplosiveCE compProperties = projectileDef.GetCompProperties<CompProperties_ExplosiveCE>();
		if (compProperties != null && compProperties.explosiveRadius > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_DescSecondaryExplosion".Translate() + ":");
			stringBuilder.AppendLine("      " + "CE_DescDamage".Translate() + ": " + compProperties.damageAmountBase.ToStringByStyle(ToStringStyle.Integer) + " (" + compProperties.explosiveDamageType.LabelCap + ")");
			stringBuilder.AppendLine("      " + "CE_DescExplosionRadius".Translate() + ": " + compProperties.explosiveRadius.ToStringByStyle(ToStringStyle.FloatOne));
		}
		if (projectilePropertiesCE.pelletCount > 1)
		{
			stringBuilder.AppendLine("   " + "CE_DescPelletCount".Translate() + ": " + GenText.ToStringByStyle(projectilePropertiesCE.pelletCount, ToStringStyle.Integer));
		}
		if (projectilePropertiesCE.spreadMult != 1f)
		{
			stringBuilder.AppendLine("   " + "CE_DescSpreadMult".Translate() + ": " + projectilePropertiesCE.spreadMult.ToStringByStyle(ToStringStyle.PercentZero));
		}
		CompProperties_Fragments compProperties2 = projectileDef.GetCompProperties<CompProperties_Fragments>();
		if (compProperties2 != null)
		{
			stringBuilder.AppendLine("   " + "CE_DescFragments".Translate() + ":");
			foreach (ThingDefCountClass fragment in compProperties2.fragments)
			{
				ProjectilePropertiesCE projectilePropertiesCE2 = fragment?.thingDef?.projectile as ProjectilePropertiesCE;
				stringBuilder.AppendLine("      " + fragment.LabelCap);
				stringBuilder.AppendLine("         " + "CE_DescDamage".Translate() + ": " + ((ProjectileProperties)projectilePropertiesCE2)?.damageAmountBase.ToString() + " (" + projectilePropertiesCE2?.damageDef.LabelCap.ToString() + ")");
				stringBuilder.AppendLine("         " + "CE_DescSharpPenetration".Translate() + ": " + projectilePropertiesCE2?.armorPenetrationSharp.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_mmRHA".Translate());
				stringBuilder.AppendLine("         " + "CE_DescBluntPenetration".Translate() + ": " + projectilePropertiesCE2?.armorPenetrationBlunt.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_MPa".Translate());
			}
		}
		return stringBuilder.ToString();
	}

	public static float GetExplosionArmorPenetration(this ProjectileProperties props)
	{
		return (float)props.damageAmountBase * 0.4f;
	}

	public static float GetExplosionArmorPenetration(this CompProperties_ExplosiveCE props)
	{
		return props.damageAmountBase * 0.4f;
	}

	public static bool IsAmmoSystemActive(AmmoDef def)
	{
		if (Controller.settings.EnableAmmoSystem)
		{
			return true;
		}
		return def?.isMortarAmmo ?? false;
	}

	public static bool IsAmmoSystemActive(AmmoSetDef ammoSet)
	{
		if (Controller.settings.EnableAmmoSystem)
		{
			return true;
		}
		return ammoSet?.isMortarAmmoSet ?? false;
	}
}
