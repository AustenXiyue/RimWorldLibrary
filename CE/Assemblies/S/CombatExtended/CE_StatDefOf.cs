using RimWorld;

namespace CombatExtended;

[DefOf]
public static class CE_StatDefOf
{
	public static StatDef Bulk;

	public static StatDef WornBulk;

	public static StatDef StuffEffectMultiplierToughness;

	public static StatDef ToughnessRating;

	public static StatDef ShotSpread;

	public static StatDef SwayFactor;

	public static StatDef SightsEfficiency;

	public static StatDef AimingAccuracy;

	public static StatDef ReloadSpeed;

	public static StatDef MuzzleFlash;

	public static StatDef MagazineCapacity;

	public static StatDef AmmoGenPerMagOverride;

	public static StatDef NightVisionEfficiency_Weapon;

	public static StatDef TicksBetweenBurstShots;

	public static StatDef BurstShotCount;

	public static StatDef Recoil;

	public static StatDef ReloadTime;

	public static StatDef OneHandedness;

	public static StatDef BipodStats;

	public static StatDef MeleePenetrationFactor;

	public static StatDef MeleeCounterParryBonus;

	public static StatDef CarryBulk;

	public static StatDef CarryWeight;

	public static StatDef MeleeCritChance;

	public static StatDef MeleeDodgeChance;

	public static StatDef MeleeParryChance;

	public static StatDef UnarmedDamage;

	public static StatDef BodyPartSharpArmor;

	public static StatDef BodyPartBluntArmor;

	public static StatDef AverageSharpArmor;

	public static StatDef NightVisionEfficiency;

	public static StatDef SmokeSensitivity;

	public static StatDef Suppressability;

	public static StatDef ArmorRating_Electric;

	static CE_StatDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_StatDefOf));
	}
}
