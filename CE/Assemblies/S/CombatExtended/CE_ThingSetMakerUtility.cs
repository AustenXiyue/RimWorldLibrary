using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended;

public static class CE_ThingSetMakerUtility
{
	public static bool CanGenerate(ThingDef d, bool allowBasic, bool allowAdvanced)
	{
		int result;
		if (d is AmmoDef ammoDef)
		{
			List<string> tradeTags = ammoDef.tradeTags;
			if (tradeTags != null && tradeTags.Contains("CE_AmmoInjector"))
			{
				result = ((AmmoUtility.IsAmmoSystemActive(ammoDef) && AdvancedOrBasic(ammoDef, allowBasic, allowAdvanced)) ? 1 : 0);
				goto IL_003a;
			}
		}
		result = 1;
		goto IL_003a;
		IL_003a:
		return (byte)result != 0;
	}

	public static bool AdvancedOrBasic(AmmoDef d, bool allowBasic, bool allowAdvanced)
	{
		return d.ammoClass == null || (d.ammoClass.advanced ? allowBasic : allowAdvanced);
	}

	public static ThingDef GetAmmoDef(CompProperties_AmmoUser comp, bool random, bool canGenerateAdvanced)
	{
		if (random)
		{
			IEnumerable<AmmoDef> enumerable = from v in comp.ammoSet.ammoTypes
				select v.ammo into d
				where canGenerateAdvanced || !d.ammoClass.advanced
				select d;
			if (enumerable.EnumerableNullOrEmpty())
			{
				enumerable = comp.ammoSet.ammoTypes.Select((AmmoLink v) => v.ammo);
			}
			return enumerable.RandomElement();
		}
		return comp.ammoSet.ammoTypes.First().ammo;
	}

	public static void GenerateAmmoForWeapon(List<Thing> outThings, bool random, bool canGenerateAdvanced, IntRange magCount)
	{
		List<Thing> list = new List<Thing>();
		foreach (Thing outThing in outThings)
		{
			CompAmmoUser compAmmoUser = outThing.TryGetComp<CompAmmoUser>();
			if (compAmmoUser != null && compAmmoUser.UseAmmo)
			{
				Thing thing = ThingMaker.MakeThing(GetAmmoDef(compAmmoUser.Props, random, canGenerateAdvanced));
				thing.stackCount = Math.Max(Math.Max(compAmmoUser.Props.AmmoGenPerMagOverride, compAmmoUser.Props.magazineSize), 1) * magCount.RandomInRange;
				list.Add(thing);
			}
		}
		foreach (Thing item in list)
		{
			outThings.Add(item);
		}
	}
}
