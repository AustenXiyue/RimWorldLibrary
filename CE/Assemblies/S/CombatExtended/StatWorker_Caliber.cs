using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class StatWorker_Caliber : StatWorker
{
	private ThingDef GunDef(StatRequest req)
	{
		ThingDef thingDef = req.Def as ThingDef;
		if (thingDef?.building?.IsTurret == true)
		{
			thingDef = thingDef.building.turretGunDef;
		}
		return thingDef;
	}

	private Thing Gun(StatRequest req)
	{
		return (req.Thing as Building_Turret)?.GetGun() ?? req.Thing;
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		if (!base.ShouldShowFor(req))
		{
			return false;
		}
		AmmoSetDef ammoSet = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet;
		if (ShouldDisplayAmmoSet(ammoSet))
		{
			return true;
		}
		return GunDef(req)?.Verbs?.Any((VerbProperties x) => x.defaultProjectile != null) == true;
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
	{
		AmmoSetDef ammoSet = GunDef(statRequest)?.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet;
		if (!ShouldDisplayAmmoSet(ammoSet))
		{
			yield break;
		}
		foreach (AmmoLink ammoType in ammoSet.ammoTypes)
		{
			yield return new Dialog_InfoCard.Hyperlink(ammoType.ammo);
		}
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AmmoSetDef ammoSetDef = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet;
		if (ShouldDisplayAmmoSet(ammoSetDef))
		{
			stringBuilder.AppendLine(ammoSetDef.LabelCap);
			float num = Gun(req)?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
			if (Mathf.Abs(1f - num) > 0.0001f)
			{
				stringBuilder.AppendLine("CE_RangedQualityMultiplier".Translate() + ": " + num.ToStringByStyle(ToStringStyle.PercentOne));
			}
			stringBuilder.AppendLine();
			foreach (AmmoLink ammoType in ammoSetDef.ammoTypes)
			{
				string text = (string.IsNullOrEmpty(ammoType.ammo.ammoClass.LabelCapShort) ? ((string)ammoType.ammo.ammoClass.LabelCap) : ammoType.ammo.ammoClass.LabelCapShort);
				stringBuilder.AppendLine(text + ":\n" + ammoType.projectile.GetProjectileReadout(Gun(req)));
			}
		}
		else
		{
			IEnumerable<ThingDef> enumerable = (from x in GunDef(req)?.Verbs?.Where((VerbProperties x) => x.defaultProjectile != null)
				select x.defaultProjectile);
			foreach (ThingDef item in enumerable)
			{
				stringBuilder.AppendLine(item.LabelCap + ":\n" + item.GetProjectileReadout(Gun(req)));
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		AmmoSetDef ammoSetDef = GunDef(optionalReq)?.GetCompProperties<CompProperties_AmmoUser>()?.ammoSet;
		if (ShouldDisplayAmmoSet(ammoSetDef))
		{
			TaggedString? taggedString = ammoSetDef?.LabelCap;
			return taggedString.HasValue ? ((string)taggedString.GetValueOrDefault()) : null;
		}
		IEnumerable<ThingDef> source = (from x in GunDef(optionalReq)?.Verbs?.Where((VerbProperties x) => x.defaultProjectile != null)
			select x.defaultProjectile);
		return source.First().LabelCap + ((source.Count() > 1) ? ("(+" + (source.Count() - 1) + ")") : "");
	}

	private bool ShouldDisplayAmmoSet(AmmoSetDef ammoSet)
	{
		return ammoSet != null && AmmoUtility.IsAmmoSystemActive(ammoSet);
	}
}
