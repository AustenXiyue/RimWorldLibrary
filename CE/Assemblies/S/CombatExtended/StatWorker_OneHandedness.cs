using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_OneHandedness : StatWorker
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return IsOneHanded(req) ? 1 : 0;
	}

	public bool IsOneHanded(StatRequest req)
	{
		if (req.Thing != null)
		{
			return req.Thing.def.weaponTags?.Contains("CE_OneHandedWeapon") ?? false;
		}
		if (req.Def is ThingDef thingDef)
		{
			return thingDef.weaponTags?.Contains("CE_OneHandedWeapon") ?? false;
		}
		return false;
	}

	public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
	{
		return ((val > 0f) ? "CE_Yes" : "CE_No").Translate();
	}
}
