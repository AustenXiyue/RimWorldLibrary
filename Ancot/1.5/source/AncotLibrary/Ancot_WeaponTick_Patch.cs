using HarmonyLib;
using Verse;

namespace AncotLibrary;

[HarmonyPatch(typeof(Pawn_EquipmentTracker))]
[HarmonyPatch("EquipmentTrackerTick")]
public static class Ancot_WeaponTick_Patch
{
	[HarmonyPostfix]
	public static void Postfix(Pawn ___pawn)
	{
		ThingWithComps primary = ___pawn.equipment.Primary;
		if (primary == null || primary.AllComps.NullOrEmpty())
		{
			return;
		}
		foreach (ThingComp allComp in primary.AllComps)
		{
			if (allComp is CompWeaponCharge compWeaponCharge)
			{
				compWeaponCharge.CompTick();
			}
			if (allComp is CompOverChargeShot compOverChargeShot)
			{
				compOverChargeShot.CompTick();
			}
			if (allComp is CompMeleeWeaponCharge_Ability compMeleeWeaponCharge_Ability)
			{
				compMeleeWeaponCharge_Ability.CompTick();
			}
		}
	}
}
