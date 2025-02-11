using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Thing), "SmeltProducts")]
public class Harmony_Thing_SmeltProducts
{
	public static void Postfix(Thing __instance, ref IEnumerable<Thing> __result)
	{
		CompAmmoUser compAmmoUser = (__instance as ThingWithComps)?.TryGetComp<CompAmmoUser>();
		if (compAmmoUser != null && compAmmoUser.HasMagazine && compAmmoUser.CurMagCount > 0 && compAmmoUser.CurrentAmmo != null)
		{
			Thing thing = ThingMaker.MakeThing(compAmmoUser.CurrentAmmo);
			thing.stackCount = compAmmoUser.CurMagCount;
			__result = CollectionExtensions.AddItem<Thing>(__result, thing);
		}
	}
}
