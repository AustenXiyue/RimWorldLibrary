using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class ArmorUtilityCE
{
	private const float PenetrationRandVariation = 0.05f;

	private const float SoftArmorMinDamageFactor = 0.2f;

	private const float HardArmorDamageFactor = 0.5f;

	private const float SpikeTrapAPModifierBlunt = 0.65f;

	private const float BulletImpactForceMultiplier = 0.2f;

	private static readonly StuffCategoryDef[] softStuffs = new StuffCategoryDef[2]
	{
		StuffCategoryDefOf.Fabric,
		StuffCategoryDefOf.Leathery
	};

	public static DamageInfo GetAfterArmorDamage(DamageInfo originalDinfo, Pawn pawn, BodyPartRecord hitPart, out bool armorDeflected, out bool armorReduced, out bool shieldAbsorbed)
	{
		shieldAbsorbed = false;
		armorDeflected = false;
		armorReduced = false;
		Comp_BurnDamageCalc comp_BurnDamageCalc = pawn.TryGetComp<Comp_BurnDamageCalc>();
		if (originalDinfo.Def.armorCategory == null || (!(originalDinfo.Weapon?.projectile is ProjectilePropertiesCE) && Verb_MeleeAttackCE.LastAttackVerb == null && originalDinfo.Weapon == null && originalDinfo.Instigator == null))
		{
			return originalDinfo;
		}
		DamageInfo damageInfo = new DamageInfo(originalDinfo);
		float dmgAmount = damageInfo.Amount;
		float penAmount = damageInfo.ArmorPenetrationInt;
		bool flag = damageInfo.Def.harmAllLayersUntilOutside || hitPart.depth == BodyPartDepth.Outside;
		if (damageInfo.IsAmbientDamage())
		{
			penAmount = damageInfo.defInt.defaultArmorPenetration;
			damageInfo.SetAmount(Mathf.CeilToInt(GetAmbientPostArmorDamage(dmgAmount, penAmount, originalDinfo.Def.armorCategory.armorRatingStat, pawn, hitPart)));
			armorDeflected = damageInfo.Amount <= 0f;
			return damageInfo;
		}
		if (comp_BurnDamageCalc != null)
		{
			comp_BurnDamageCalc.deflectedSharp = false;
		}
		if (flag && pawn.apparel != null && !pawn.apparel.WornApparel.NullOrEmpty())
		{
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			Apparel apparel = wornApparel.FirstOrDefault((Apparel x) => x is Apparel_Shield);
			if (apparel != null)
			{
				bool flag2 = false;
				ThingDef weapon = damageInfo.Weapon;
				if (weapon == null || !weapon.IsMeleeWeapon)
				{
					ShieldDefExtension modExtension = apparel.def.GetModExtension<ShieldDefExtension>();
					if (modExtension == null)
					{
						Log.ErrorOnce("Combat Extended :: shield " + apparel.def.ToString() + " is Apparel_Shield but has no ShieldDefExtension", apparel.def.GetHashCode() + 12748102);
					}
					else if (modExtension.PartIsCoveredByShield(hitPart, pawn))
					{
						flag2 = (pawn.stances?.curStance as Stance_Busy)?.verb == null || !hitPart.IsInGroup(CE_BodyPartGroupDefOf.RightArm);
					}
				}
				if (flag2 && !TryPenetrateArmor(damageInfo.Def, apparel.GetStatValue(damageInfo.Def.armorCategory.armorRatingStat), ref penAmount, ref dmgAmount, apparel))
				{
					if (damageInfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp && comp_BurnDamageCalc != null)
					{
						comp_BurnDamageCalc.deflectedSharp = true;
					}
					damageInfo = GetDeflectDamageInfo(damageInfo, hitPart, ref dmgAmount, ref penAmount);
					float num = penAmount;
					ThingDef weapon2 = damageInfo.Weapon;
					penAmount = num * ((weapon2 != null && weapon2.IsMeleeWeapon) ? 1f : 0.2f);
					if (!TryPenetrateArmor(damageInfo.Def, apparel.GetStatValue(damageInfo.Def.armorCategory.armorRatingStat), ref penAmount, ref dmgAmount))
					{
						shieldAbsorbed = true;
						armorDeflected = true;
						damageInfo.SetAmount(0f);
						if (damageInfo.Weapon?.projectile is ProjectilePropertiesCE projectilePropertiesCE && !projectilePropertiesCE.secondaryDamage.NullOrEmpty())
						{
							foreach (SecondaryDamage item in projectilePropertiesCE.secondaryDamage)
							{
								if (apparel.Destroyed)
								{
									break;
								}
								if (Rand.Chance(item.chance))
								{
									DamageInfo dinfo = item.GetDinfo();
									float penAmount2 = dinfo.ArmorPenetrationInt;
									float dmgAmount2 = dinfo.Amount;
									TryPenetrateArmor(dinfo.Def, apparel.GetStatValue(dinfo.Def.armorCategory.armorRatingStat), ref penAmount2, ref dmgAmount2, apparel);
								}
							}
						}
						return damageInfo;
					}
					shieldAbsorbed = true;
					BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, BodyPartTagDefOf.ManipulationLimbCore).FirstOrFallback((BodyPartRecord x) => x.IsInGroup(CE_BodyPartGroupDefOf.LeftArm) || x.IsInGroup(CE_BodyPartGroupDefOf.RightArm));
					if (bodyPartRecord == null)
					{
						bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, BodyPartTagDefOf.ManipulationLimbSegment).First((BodyPartRecord x) => x.IsInGroup(CE_BodyPartGroupDefOf.LeftShoulder));
					}
					damageInfo.SetHitPart(bodyPartRecord);
					damageInfo.SetAmount(dmgAmount);
					if (damageInfo.Weapon?.projectile is ProjectilePropertiesCE projectilePropertiesCE2 && !projectilePropertiesCE2.secondaryDamage.NullOrEmpty())
					{
						foreach (SecondaryDamage item2 in projectilePropertiesCE2.secondaryDamage)
						{
							if (apparel.Destroyed)
							{
								break;
							}
							if (Rand.Chance(item2.chance))
							{
								DamageInfo dinfo2 = item2.GetDinfo();
								float penAmount3 = dinfo2.ArmorPenetrationInt;
								float dmgAmount3 = dinfo2.Amount;
								TryPenetrateArmor(dinfo2.Def, apparel.GetStatValue(dinfo2.Def.armorCategory.armorRatingStat), ref penAmount3, ref dmgAmount3, apparel);
							}
						}
					}
					return damageInfo;
				}
			}
			for (int num2 = wornApparel.Count - 1; num2 >= 0; num2--)
			{
				Apparel apparel2 = wornApparel[num2];
				if (apparel2 != null && apparel2.def.apparel.CoversBodyPart(hitPart) && !TryPenetrateArmor(damageInfo.Def, apparel2.PartialStat(damageInfo.Def.armorCategory.armorRatingStat, hitPart), ref penAmount, ref dmgAmount, apparel2))
				{
					if (damageInfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp && comp_BurnDamageCalc != null)
					{
						comp_BurnDamageCalc.deflectedSharp = true;
					}
					damageInfo = GetDeflectDamageInfo(damageInfo, hitPart, ref dmgAmount, ref penAmount);
					if (apparel2 == wornApparel.ElementAtOrDefault(num2))
					{
						num2++;
					}
				}
				if (dmgAmount <= 0f)
				{
					damageInfo.SetAmount(0f);
					armorDeflected = true;
					return damageInfo;
				}
			}
		}
		List<BodyPartRecord> list = new List<BodyPartRecord> { hitPart };
		if (damageInfo.Def.harmAllLayersUntilOutside)
		{
			BodyPartRecord bodyPartRecord2 = hitPart;
			while (bodyPartRecord2.parent != null && bodyPartRecord2.depth == BodyPartDepth.Inside)
			{
				bodyPartRecord2 = bodyPartRecord2.parent;
				list.Add(bodyPartRecord2);
			}
		}
		bool flag3 = damageInfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp;
		StatDef stat = (flag3 ? CE_StatDefOf.BodyPartSharpArmor : CE_StatDefOf.BodyPartBluntArmor);
		float statValue = pawn.GetStatValue(stat);
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			BodyPartRecord bodyPartRecord3 = list[num3];
			bool flag4 = bodyPartRecord3.IsInGroup(CE_BodyPartGroupDefOf.CoveredByNaturalArmor);
			float armorAmount = (flag4 ? pawn.PartialStat(damageInfo.Def.armorCategory.armorRatingStat, bodyPartRecord3, dmgAmount, penAmount) : 0f);
			if (!TryPenetrateArmor(damageInfo.Def, armorAmount, ref penAmount, ref dmgAmount, null, statValue))
			{
				damageInfo.SetHitPart(bodyPartRecord3);
				if (flag3 && flag4)
				{
					if (damageInfo.Def.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp && comp_BurnDamageCalc != null)
					{
						comp_BurnDamageCalc.deflectedSharp = true;
						comp_BurnDamageCalc.weapon = originalDinfo.Weapon;
					}
					damageInfo = GetDeflectDamageInfo(damageInfo, bodyPartRecord3, ref dmgAmount, ref penAmount);
					TryPenetrateArmor(damageInfo.Def, pawn.GetStatValue(damageInfo.Def.armorCategory.armorRatingStat), ref penAmount, ref dmgAmount, null, statValue);
				}
				break;
			}
			if (dmgAmount <= 0f)
			{
				damageInfo.SetAmount(0f);
				armorDeflected = true;
				return damageInfo;
			}
		}
		if (flag3 && damageInfo.Amount > dmgAmount)
		{
			pawn.TakeDamage(GetDeflectDamageInfo(damageInfo, hitPart, ref dmgAmount, ref penAmount, partialPen: true));
		}
		damageInfo.SetAmount(dmgAmount);
		return damageInfo;
	}

	private static bool TryPenetrateArmor(DamageDef def, float armorAmount, ref float penAmount, ref float dmgAmount, Thing armor = null, float partDensity = 0f)
	{
		bool flag = def.armorCategory == DamageArmorCategoryDefOf.Sharp;
		bool flag2 = def.armorCategory == CE_DamageArmorCategoryDefOf.Heat;
		bool flag3 = flag && armorAmount > penAmount;
		DamageDefExtensionCE damageDefExtensionCE = def.GetModExtension<DamageDefExtensionCE>() ?? new DamageDefExtensionCE();
		bool flag4 = flag3 && damageDefExtensionCE.noDamageOnDeflect;
		float num = penAmount - armorAmount;
		float num2 = (flag4 ? 0f : ((penAmount == 0f) ? 1f : Mathf.Clamp01(num / penAmount)));
		flag3 = flag3 || num2 == 0f;
		float num3 = dmgAmount * num2;
		num -= partDensity;
		if (armor != null)
		{
			bool flag5 = armor.Stuff != null && armor.Stuff.stuffProps.categories.Any((StuffCategoryDef s) => softStuffs.Contains(s));
			float num4 = 0f;
			if (flag5)
			{
				if (flag2)
				{
					num4 = armor.GetStatValue(StatDefOf.Flammability) * dmgAmount;
				}
				if (flag)
				{
					num4 = Mathf.Max(dmgAmount * 0.2f, dmgAmount - num3);
					TryDamageArmor(def, penAmount, armorAmount, ref num4, armor);
				}
			}
			else
			{
				if (!flag && penAmount / armorAmount < 0.5f)
				{
					num4 = 0f;
				}
				else
				{
					if (flag2)
					{
						num4 = armor.GetStatValue(StatDefOf.Flammability) * dmgAmount;
					}
					else if (penAmount == 0f || armorAmount == 0f)
					{
						if (armor.GetStatValue(StatDefOf.ArmorRating_Sharp) == 0f && armor.GetStatValue(StatDefOf.ArmorRating_Blunt) == 0f && armor.GetStatValue(StatDefOf.ArmorRating_Heat) == 0f)
						{
							Log.ErrorOnce($"penAmount or armorAmount are zero for {def.armorCategory} on {armor}", armor.def.GetHashCode() + 846532021);
						}
					}
					else
					{
						num4 = (dmgAmount - num3) * Mathf.Min(1f, penAmount * penAmount / (armorAmount * armorAmount)) + num3 * Mathf.Clamp01(armorAmount / penAmount);
					}
					num4 *= 0.5f;
				}
				TryDamageArmor(def, penAmount, armorAmount, ref num4, armor);
			}
		}
		if (!flag3 || !flag)
		{
			dmgAmount = Mathf.Max(0f, num3);
			penAmount = Mathf.Max(0f, num);
		}
		return !flag3;
	}

	private static bool TryDamageArmor(DamageDef def, float penAmount, float armorAmount, ref float armorDamage, Thing armor)
	{
		if (armorDamage == 0f)
		{
			return false;
		}
		if (Rand.Value < armorDamage - Mathf.Floor(armorDamage))
		{
			armorDamage = Mathf.Ceil(armorDamage);
		}
		armorDamage = Mathf.Floor(armorDamage);
		if (armorDamage != 0f)
		{
			armor.TakeDamage(new DamageInfo(def, armorDamage));
			return true;
		}
		return false;
	}

	private static float GetAmbientPostArmorDamage(float dmgAmount, float penAmount, StatDef armorRatingStat, Pawn pawn, BodyPartRecord part)
	{
		if (penAmount < 0f)
		{
			penAmount = 0f;
		}
		float num = 1f + penAmount;
		if (part.IsInGroup(CE_BodyPartGroupDefOf.CoveredByNaturalArmor))
		{
			num -= pawn.GetStatValue(armorRatingStat);
		}
		if (num <= 0f)
		{
			return 0f;
		}
		if (pawn.apparel != null && !pawn.apparel.WornApparel.NullOrEmpty())
		{
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			foreach (Apparel item in wornApparel)
			{
				if (item.def.apparel.CoversBodyPart(part))
				{
					num -= item.GetStatValue(armorRatingStat);
				}
				if (num <= 0f)
				{
					num = 0f;
					break;
				}
			}
		}
		if (num > 1f)
		{
			num = 1f;
		}
		Comp_BurnDamageCalc comp_BurnDamageCalc = pawn.TryGetComp<Comp_BurnDamageCalc>();
		if (comp_BurnDamageCalc != null)
		{
			if (armorRatingStat == StatDefOf.ArmorRating_Heat && comp_BurnDamageCalc.deflectedSharp)
			{
				num /= 2f;
			}
			comp_BurnDamageCalc.deflectedSharp = false;
		}
		return (float)Math.Floor(dmgAmount * num);
	}

	private static DamageInfo GetDeflectDamageInfo(DamageInfo dinfo, BodyPartRecord hitPart, ref float dmgAmount, ref float penAmount, bool partialPen = false)
	{
		if (dinfo.Def.armorCategory != DamageArmorCategoryDefOf.Sharp)
		{
			if (!partialPen)
			{
				dmgAmount = 0f;
				penAmount = 0f;
			}
			dinfo.SetAmount(0f);
			return dinfo;
		}
		float num = dmgAmount;
		float num2 = penAmount;
		float num3 = (partialPen ? ((dinfo.ArmorPenetrationInt - num2) * (dinfo.Amount - num) / dinfo.Amount) : num2) / dinfo.ArmorPenetrationInt;
		if (dinfo.Weapon?.projectile is ProjectilePropertiesCE projectilePropertiesCE)
		{
			num2 = projectilePropertiesCE.armorPenetrationBlunt * num3;
		}
		else if (dinfo.Instigator?.def.thingClass == typeof(Building_TrapDamager))
		{
			float num4 = dinfo.Instigator.GetStatValue(StatDefOf.TrapMeleeDamage) * 0.65f;
			num2 = num4 * num3;
		}
		else if (Verb_MeleeAttackCE.LastAttackVerb != null)
		{
			num2 = Verb_MeleeAttackCE.LastAttackVerb.ArmorPenetrationBlunt;
		}
		else
		{
			Log.Warning($"[CE] Deflection for Instigator:{dinfo.Instigator} Target:{dinfo.IntendedTarget} DamageDef:{dinfo.Def} Weapon:{dinfo.Weapon} has null verb, overriding AP.");
			num2 = 50f;
		}
		num = Mathf.Pow(num2 * 10000f, 1f / 3f) / 10f;
		if (dinfo.Weapon?.projectile is ProjectilePropertiesCE)
		{
			num *= dinfo.Amount / (float)dinfo.Weapon.projectile.damageAmountBase;
		}
		DamageInfo result = new DamageInfo(DamageDefOf.Blunt, num, num2, dinfo.Angle, dinfo.Instigator, GetOuterMostParent(hitPart), partialPen ? null : dinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown, null, dinfo.InstigatorGuilty);
		result.SetBodyRegion(dinfo.Height, dinfo.Depth);
		result.SetWeaponBodyPartGroup(dinfo.WeaponBodyPartGroup);
		result.SetWeaponHediff(dinfo.WeaponLinkedHediff);
		result.SetInstantPermanentInjury(dinfo.InstantPermanentInjury);
		result.SetAllowDamagePropagation(dinfo.AllowDamagePropagation);
		if (!partialPen)
		{
			dmgAmount = num;
			penAmount = num2;
		}
		return result;
	}

	private static BodyPartRecord GetOuterMostParent(BodyPartRecord part)
	{
		BodyPartRecord bodyPartRecord = part;
		if (bodyPartRecord != null)
		{
			while (bodyPartRecord.parent != null && bodyPartRecord.depth != BodyPartDepth.Outside)
			{
				bodyPartRecord = bodyPartRecord.parent;
			}
		}
		return bodyPartRecord;
	}

	private static bool IsAmbientDamage(this DamageInfo dinfo)
	{
		return (dinfo.Def.GetModExtension<DamageDefExtensionCE>() ?? new DamageDefExtensionCE()).isAmbientDamage;
	}

	public static void ApplyParryDamage(DamageInfo dinfo, Thing parryThing)
	{
		if (parryThing is Pawn pawn)
		{
			dinfo.SetAmount(dinfo.Amount * Mathf.Clamp01(Rand.Range(0.5f - pawn.GetStatValue(CE_StatDefOf.MeleeParryChance), 1f - pawn.GetStatValue(CE_StatDefOf.MeleeParryChance) * 1.25f)));
			pawn.TakeDamage(dinfo);
			return;
		}
		if (dinfo.IsAmbientDamage())
		{
			int num = Mathf.CeilToInt(dinfo.Amount * Mathf.Clamp01(parryThing.GetStatValue(dinfo.Def.armorCategory.armorRatingStat)));
			dinfo.SetAmount(num);
			parryThing.TakeDamage(dinfo);
			return;
		}
		float dmgAmount = dinfo.Amount * 0.5f;
		float num2;
		if (parryThing.def.IsApparel)
		{
			num2 = parryThing.GetStatValue(dinfo.Def.armorCategory.armorRatingStat);
		}
		else
		{
			num2 = parryThing.GetStatValue(CE_StatDefOf.ToughnessRating);
			if (dinfo.Def.armorCategory != DamageArmorCategoryDefOf.Sharp)
			{
				num2 *= 1.5f;
			}
		}
		float penAmount = dinfo.ArmorPenetrationInt;
		bool partialPen = TryPenetrateArmor(dinfo.Def, num2, ref penAmount, ref dmgAmount, parryThing);
		if (dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp && dmgAmount > 0f)
		{
			DamageInfo deflectDamageInfo = GetDeflectDamageInfo(dinfo, dinfo.HitPart, ref dmgAmount, ref penAmount, partialPen);
			if (dmgAmount > 0f)
			{
				ApplyParryDamage(deflectDamageInfo, parryThing);
			}
		}
	}
}
