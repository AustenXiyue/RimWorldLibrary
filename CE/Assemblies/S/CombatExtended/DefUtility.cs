using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace CombatExtended;

public static class DefUtility
{
	internal static FlagArray isVisibleLayerArray = new FlagArray(65535);

	internal static FlagArray isAOEArray = new FlagArray(65535);

	internal static FlagArray isFlamableArray = new FlagArray(65535);

	internal static FlagArray isMenuHiddenArray = new FlagArray(65535);

	internal static FlagArray isRadioArray = new FlagArray(65535);

	public static void Initialize()
	{
		foreach (ApparelLayerDef allDef in DefDatabase<ApparelLayerDef>.AllDefs)
		{
			ProcessApparelLayer(allDef);
		}
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.IsApparel))
		{
			ProcessApparel(item);
		}
		foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.HasModExtension<ThingDefExtensionCE>()))
		{
			ProcessThingDefExtensionCE(item2);
		}
		foreach (AmmoSetDef allDef2 in DefDatabase<AmmoSetDef>.AllDefs)
		{
			ProcessAmmo(allDef2);
		}
		foreach (ThingDef item3 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.verbs != null && d.verbs.Any((VerbProperties x) => typeof(Verb_LaunchProjectileCE).IsAssignableFrom(x.verbClass))))
		{
			ProcessWeapons(item3);
		}
		foreach (AttachmentDef allDef3 in DefDatabase<AttachmentDef>.AllDefs)
		{
			allDef3.ValidateStats();
		}
		foreach (WeaponPlatformDef allDef4 in DefDatabase<WeaponPlatformDef>.AllDefs)
		{
			allDef4.PrepareStats();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsVisibleLayer(this ThingDef def)
	{
		if (!def.IsApparel)
		{
			throw new ArgumentException("Argument need to be apparel!");
		}
		return isVisibleLayerArray[def.index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsVisibleLayer(this ApparelLayerDef def)
	{
		return isVisibleLayerArray[def.index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMenuHidden(this ThingDef def)
	{
		return isMenuHiddenArray[def.index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsFlamable(this ThingDef def)
	{
		return isFlamableArray[def.index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetMenuHidden(this ThingDef def, bool value)
	{
		isMenuHiddenArray[def.index] = value;
		if (def.HasModExtension<ThingDefExtensionCE>())
		{
			def.GetModExtension<ThingDefExtensionCE>().MenuHidden = value;
		}
	}

	public static bool IsIlluminationDevice(this ThingDef def)
	{
		return def.verbs?.Any((VerbProperties v) => v.verbClass == typeof(Verb_ShootFlareCE)) ?? false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAOEWeapon(this ThingDef def)
	{
		return isAOEArray[def.index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ProduceSmokeScreen(this ThingDef def)
	{
		return def.weaponTags?.Contains("GrenadeSmoke") ?? false;
	}

	private static void ProcessApparel(ThingDef def)
	{
		ApparelLayerDef lastLayer = def.apparel.LastLayer;
		if (lastLayer != null)
		{
			isVisibleLayerArray[def.index] = isVisibleLayerArray[lastLayer.index];
		}
		if (def.HasModExtension<ApparelDefExtension>())
		{
			ApparelDefExtension modExtension = def.GetModExtension<ApparelDefExtension>();
			isRadioArray[def.index] = modExtension.isRadioPack;
			if (Prefs.DevMode && modExtension.isRadioPack)
			{
				Log.Message($"{def}");
			}
		}
	}

	private static void ProcessThing(ThingDef def)
	{
		if (def.useHitPoints)
		{
			isFlamableArray[def.index] = def.IsFlamable();
		}
	}

	private static void ProcessApparelLayer(ApparelLayerDef layer)
	{
		isVisibleLayerArray[layer.index] = IsVisibleLayer_Internal(layer);
	}

	private static bool IsVisibleLayer_Internal(ApparelLayerDef layer)
	{
		return layer.drawOrder >= ApparelLayerDefOf.Shell.drawOrder && layer != ApparelLayerDefOf.Belt && !(layer.GetModExtension<ApparelLayerExtension>()?.IsHeadwear ?? false);
	}

	private static void ProcessThingDefExtensionCE(ThingDef def)
	{
		ThingDefExtensionCE modExtension = def.GetModExtension<ThingDefExtensionCE>();
		if (modExtension != null)
		{
			isMenuHiddenArray[def.index] = modExtension.MenuHidden;
		}
	}

	private static void ProcessWeapons(ThingDef def)
	{
		CompProperties_AmmoUser compProperties_AmmoUser = (CompProperties_AmmoUser)(def.comps?.FirstOrDefault((CompProperties c) => c.compClass == typeof(CompAmmoUser)));
		if (compProperties_AmmoUser?.ammoSet != null)
		{
			isAOEArray[def.index] = isAOEArray[compProperties_AmmoUser.ammoSet.index];
		}
		FlagArray flagArray = isAOEArray;
		ushort index = def.index;
		int value;
		if (!isAOEArray[def.index])
		{
			List<string> weaponTags = def.weaponTags;
			if (weaponTags == null || !weaponTags.Contains("CE_AI_AOE"))
			{
				List<VerbProperties> verbs = def.verbs;
				if (verbs == null || !verbs.Any((VerbProperties v) => v.defaultProjectile?.thingClass == typeof(ProjectileCE_Explosive)))
				{
					List<VerbProperties> verbs2 = def.verbs;
					if (verbs2 == null || !verbs2.Any((VerbProperties v) => v.verbClass == typeof(Verb_ShootCEOneUse)))
					{
						value = ((def.comps?.Any((CompProperties c) => c.compClass == typeof(CompExplosive) || c.compClass == typeof(CompExplosiveCE)) ?? false) ? 1 : 0);
						goto IL_014b;
					}
				}
			}
		}
		value = 1;
		goto IL_014b;
		IL_014b:
		flagArray[index] = (byte)value != 0;
		float value2 = def.verbs.Max((VerbProperties v) => v.ticksBetweenBurstShots);
		if (!def.statBases.Any((StatModifier s) => s.stat == CE_StatDefOf.TicksBetweenBurstShots))
		{
			def.statBases.Add(new StatModifier
			{
				stat = CE_StatDefOf.TicksBetweenBurstShots,
				value = value2
			});
		}
		float value3 = def.verbs.Max((VerbProperties v) => v.burstShotCount);
		if (!def.statBases.Any((StatModifier s) => s.stat == CE_StatDefOf.BurstShotCount))
		{
			def.statBases.Add(new StatModifier
			{
				stat = CE_StatDefOf.BurstShotCount,
				value = value3
			});
		}
		float value4 = def.verbs.Max((VerbProperties v) => (v is VerbPropertiesCE verbPropertiesCE) ? verbPropertiesCE.recoilAmount : 0f);
		if (!def.statBases.Any((StatModifier s) => s.stat == CE_StatDefOf.Recoil))
		{
			def.statBases.Add(new StatModifier
			{
				stat = CE_StatDefOf.Recoil,
				value = value4
			});
		}
		if (compProperties_AmmoUser != null)
		{
			float reloadTime = def.GetCompProperties<CompProperties_AmmoUser>().reloadTime;
			if (!def.statBases.Any((StatModifier s) => s.stat == CE_StatDefOf.ReloadTime))
			{
				def.statBases.Add(new StatModifier
				{
					stat = CE_StatDefOf.ReloadTime,
					value = reloadTime
				});
			}
			float value5 = def.GetCompProperties<CompProperties_AmmoUser>().AmmoGenPerMagOverride;
			if (!def.statBases.Any((StatModifier s) => s.stat == CE_StatDefOf.AmmoGenPerMagOverride))
			{
				def.statBases.Add(new StatModifier
				{
					stat = CE_StatDefOf.AmmoGenPerMagOverride,
					value = value5
				});
			}
		}
	}

	private static void ProcessAmmo(AmmoSetDef def)
	{
		isAOEArray[def.index] = def.isMortarAmmoSet || (def.ammoTypes?.Any((AmmoLink a) => IsAOEAmmoLink(a)) ?? false);
	}

	private static bool IsAOEAmmoLink(AmmoLink link)
	{
		return link.ammo?.detonateProjectile != null || link.projectile?.thingClass == typeof(ProjectileCE_Explosive) || link.projectile?.thingClass == typeof(Projectile_Explosive) || link.projectile?.comps?.Any((CompProperties c) => c.compClass == typeof(CompFragments) || c.compClass == typeof(CompExplosive) || c.compClass == typeof(CompExplosiveCE)) == true;
	}

	public static bool IsRadioPack(this ThingDef def)
	{
		return isRadioArray[def.index];
	}
}
