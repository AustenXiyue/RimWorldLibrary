using System.Text;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_Magazine : StatWorker
{
	public CompAmmoUser compAmmo;

	private ThingDef GunDef(StatRequest req)
	{
		ThingDef thingDef = req.Def as ThingDef;
		if (thingDef?.building?.IsTurret == true)
		{
			thingDef = thingDef.building.turretGunDef;
		}
		return thingDef;
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		return base.ShouldShowFor(req) && (GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize).GetValueOrDefault() > 0;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		compAmmo = req.Thing?.TryGetComp<CompAmmoUser>();
		if ((compAmmo?.Props?.ammoSet ?? null) != (((CompProperties_AmmoUser)(req.Thing?.def?.comps?.Find((CompProperties x) => x is CompProperties_AmmoUser)))?.ammoSet ?? null))
		{
			return compAmmo.Props.magazineSize;
		}
		return (GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize).GetValueOrDefault();
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		CompProperties_AmmoUser compProperties_AmmoUser = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>();
		stringBuilder.AppendLine("CE_MagazineSize".Translate() + ": " + GenText.ToStringByStyle(GetMagSize(req), ToStringStyle.Integer));
		stringBuilder.AppendLine("CE_ReloadTime".Translate() + ": " + compProperties_AmmoUser.reloadTime.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate());
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
	{
		if (!optionalReq.HasThing)
		{
			CompProperties_AmmoUser compProperties_AmmoUser = GunDef(optionalReq)?.GetCompProperties<CompProperties_AmmoUser>();
			return compProperties_AmmoUser.magazineSize + " / " + compProperties_AmmoUser.reloadTime.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate();
		}
		CompProperties_AmmoUser compProperties_AmmoUser2 = GunDef(optionalReq)?.GetCompProperties<CompProperties_AmmoUser>();
		return GetMagSize(optionalReq) + " / " + compProperties_AmmoUser2.reloadTime.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate();
	}

	private int GetMagSize(StatRequest req)
	{
		compAmmo = req.Thing?.TryGetComp<CompAmmoUser>();
		if ((compAmmo?.Props?.ammoSet ?? null) != (((CompProperties_AmmoUser)(req.Thing?.def?.comps?.Find((CompProperties x) => x is CompProperties_AmmoUser)))?.ammoSet ?? null))
		{
			return compAmmo.Props.magazineSize;
		}
		if (req.HasThing)
		{
			return (int)req.Thing.GetStatValue(CE_StatDefOf.MagazineCapacity);
		}
		return (GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize).GetValueOrDefault();
	}
}
