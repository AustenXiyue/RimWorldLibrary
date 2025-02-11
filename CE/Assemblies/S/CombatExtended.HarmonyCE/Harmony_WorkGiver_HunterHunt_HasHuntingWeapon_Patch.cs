using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon")]
public class Harmony_WorkGiver_HunterHunt_HasHuntingWeapon_Patch
{
	public static void Postfix(ref bool __result, Pawn p)
	{
		if (__result)
		{
			CompAmmoUser compAmmoUser = p.equipment.Primary.TryGetComp<CompAmmoUser>();
			__result = compAmmoUser == null || compAmmoUser.CanBeFiredNow || compAmmoUser.HasAmmo;
		}
		else
		{
			ThingWithComps primary = p.equipment.Primary;
			__result = primary != null && Controller.settings.AllowMeleeHunting && primary.def.IsMeleeWeapon;
		}
	}
}
