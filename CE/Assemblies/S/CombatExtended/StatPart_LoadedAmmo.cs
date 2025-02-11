using RimWorld;
using Verse;

namespace CombatExtended;

public class StatPart_LoadedAmmo : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetValue(req, out var num))
		{
			val += num;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		float num;
		return TryGetValue(req, out num) ? ("CE_StatsReport_LoadedAmmo".Translate() + ": " + parentStat.ValueToString(num)) : ((TaggedString)null);
	}

	public bool TryGetValue(StatRequest req, out float num)
	{
		num = 0f;
		if (req.HasThing)
		{
			CompAmmoUser compAmmoUser = req.Thing.TryGetComp<CompAmmoUser>();
			CompUnderBarrel compUnderBarrel = req.Thing.TryGetComp<CompUnderBarrel>();
			if (compUnderBarrel != null)
			{
				if (compAmmoUser != null && compAmmoUser.CurrentAmmo != null)
				{
					num = compAmmoUser.CurrentAmmo.GetStatValueAbstract(parentStat) * (float)compAmmoUser.CurMagCount;
					if (parentStat == CE_StatDefOf.Bulk)
					{
						num *= compAmmoUser.Props.loadedAmmoBulkFactor;
					}
				}
				if (compUnderBarrel.usingUnderBarrel)
				{
					num += (compUnderBarrel.mainGunLoadedAmmo?.GetStatValueAbstract(parentStat) ?? 0f) * (float)compUnderBarrel.mainGunMagCount;
				}
				else
				{
					num += (compUnderBarrel.UnderBarrelLoadedAmmo?.GetStatValueAbstract(parentStat) ?? 0f) * (float)compUnderBarrel.UnderBarrelMagCount;
				}
				return num != 0f;
			}
			if (compAmmoUser != null && compAmmoUser.CurrentAmmo != null)
			{
				num = compAmmoUser.CurrentAmmo.GetStatValueAbstract(parentStat) * (float)compAmmoUser.CurMagCount;
				if (parentStat == CE_StatDefOf.Bulk)
				{
					num *= compAmmoUser.Props.loadedAmmoBulkFactor;
				}
			}
		}
		return num != 0f;
	}
}
