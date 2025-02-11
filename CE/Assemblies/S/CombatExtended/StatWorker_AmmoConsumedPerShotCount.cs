using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended.Compatibility;
using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_AmmoConsumedPerShotCount : StatWorker
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
		int result;
		if (base.ShouldShowFor(req) && !GunDef(req).IsMeleeWeapon)
		{
			CompProperties_AmmoUser obj = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>();
			result = ((obj == null || obj.ammoSet?.ammoConsumedPerShot != 1 || GunDef(req)?.Verbs?.Any((VerbProperties x) => ((x as VerbPropertiesCE)?.ammoConsumedPerShotCount ?? 1) > 1) == true) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return (GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>())?.ammoSet?.ammoConsumedPerShot ?? (GunDef(req)?.Verbs?.OfType<VerbPropertiesCE>().FirstOrDefault((VerbPropertiesCE x) => x.ammoConsumedPerShotCount > 1)?.ammoConsumedPerShotCount).GetValueOrDefault();
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<VerbProperties> list = GunDef(req)?.Verbs;
		if (!list.NullOrEmpty() && !list.OfType<VerbPropertiesCE>().Any())
		{
			stringBuilder.AppendLine("Not patched for CE");
		}
		stringBuilder.AppendLine("");
		return stringBuilder.ToString().TrimEndNewlines();
	}
}
