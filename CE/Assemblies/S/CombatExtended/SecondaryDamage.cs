using RimWorld;
using Verse;

namespace CombatExtended;

public class SecondaryDamage
{
	private const float SecExplosionPenPerDmg = 0.8f;

	public DamageDef def;

	public int amount;

	public float chance = 1f;

	public DamageInfo GetDinfo()
	{
		return new DamageInfo(def, amount);
	}

	public DamageInfo GetDinfo(DamageInfo primaryDinfo)
	{
		float armorPenetration = 0f;
		if (def.isExplosive)
		{
			armorPenetration = (float)amount * 0.8f;
		}
		else if (def.armorCategory == DamageArmorCategoryDefOf.Sharp)
		{
			armorPenetration = primaryDinfo.ArmorPenetrationInt;
		}
		DamageInfo result = new DamageInfo(def, amount, armorPenetration, primaryDinfo.Angle, primaryDinfo.Instigator, primaryDinfo.HitPart, primaryDinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown, null, primaryDinfo.InstigatorGuilty);
		result.SetBodyRegion(primaryDinfo.Height, primaryDinfo.Depth);
		return result;
	}
}
