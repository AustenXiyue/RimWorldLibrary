using RimWorld;
using Verse;

namespace CombatExtended;

public class StatWorker_MuzzleFlash : StatWorker
{
	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		ThingDef thingDef = req.Thing?.def;
		if (thingDef != null && thingDef.verbs != null && thingDef.IsRangedWeapon && thingDef.verbs.Count > 0)
		{
			int num = 0;
			float num2 = 0f;
			foreach (VerbProperties verb in thingDef.verbs)
			{
				if (!verb.IsMeleeAttack && verb.muzzleFlashScale > 0.01f)
				{
					num2 += verb.muzzleFlashScale;
					num++;
				}
			}
			return num2 / (float)num;
		}
		return 0f;
	}
}
