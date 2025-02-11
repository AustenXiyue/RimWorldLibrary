using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public static class GunPatcherUtil
{
	public static bool DiscardDesignationsMatch(this ThingDef thingDef, GunPatcherPresetDef presetDef)
	{
		bool result = false;
		if (presetDef.DiscardDesignations)
		{
			string[] source = thingDef.label.Replace("A", " ").ToLower().Replace("-", "")
				.Split(' ');
			return source.Any((string x) => presetDef.names.Contains(x));
		}
		return result;
	}

	public static bool MatchesVerbProps(this ThingDef gun, GunPatcherPresetDef preset)
	{
		_ = preset.WarmupRange;
		_ = preset.DamageRange;
		_ = preset.RangeRange;
		_ = preset.ProjSpeedRange;
		return preset.WarmupRange.Includes(gun.Verbs.FirstOrFallback()?.warmupTime ?? 0f) && preset.RangeRange.Includes(gun.Verbs.FirstOrFallback()?.range ?? 0f) && preset.DamageRange.Includes(((float?)gun.Verbs.FirstOrFallback()?.defaultProjectile.projectile.GetDamageAmount(1f)) ?? 0f) && preset.ProjSpeedRange.Includes(gun.Verbs.FirstOrFallback()?.defaultProjectile.projectile.speed ?? 0f);
	}

	public static void AddOrChangeStat(this ThingDef gun, StatModifier mod)
	{
		if (gun.statBases.Any((StatModifier x) => x.stat == mod.stat))
		{
			gun.statBases.Find((StatModifier x) => x.stat == mod.stat).value = mod.value;
		}
		else
		{
			gun.statBases.Add(mod);
		}
	}

	public static void MergeStatLists(this ThingDef gun, List<StatModifier> mods)
	{
		foreach (StatModifier mod in mods)
		{
			gun.AddOrChangeStat(mod);
		}
	}

	public static void PatchGunsFromPreset(this IEnumerable<ThingDef> unpatchedGuns, GunPatcherPresetDef preset)
	{
		foreach (ThingDef gun in unpatchedGuns)
		{
			try
			{
				bool flag = false;
				if (gun.label.ToLower().Replace("-", "").Split(' ')
					.Any((string y) => preset.names?.Contains(y) ?? false))
				{
					flag = true;
				}
				else
				{
					List<string> names = preset.names;
					if (names != null && names.Contains(gun.label.ToLower()))
					{
						flag = true;
					}
					else if (gun.DiscardDesignationsMatch(preset))
					{
						flag = true;
					}
					else if (gun.MatchesVerbProps(preset))
					{
						flag = true;
					}
					else if (preset.tags != null && gun.weaponTags != null && preset.tags.Intersect(gun.weaponTags).Any())
					{
						flag = true;
					}
					else if (preset.specialGuns.Any((LabelGun y) => y.names.Contains(gun.label) || y.names.Intersect(gun.label.ToLower().Replace("-", "").Split(' ')).Any()))
					{
						flag = true;
					}
				}
				if (flag)
				{
					gun.PatchGunFromPreset(preset);
				}
			}
			catch (Exception ex)
			{
				Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, ex?.ToString() ?? "", StackTraceUtility.ExtractStringFromException(ex)));
				Log.Error($"Unhandled exception patching gun {gun} from preset {preset}");
			}
		}
	}

	public static void PatchGunFromPreset(this ThingDef gun, GunPatcherPresetDef preset)
	{
		if (Controller.settings.DebugAutopatcherLogger)
		{
			Log.Message($"Auto-patching {gun} ({gun.label})");
		}
		ThingDef OldProj = gun.Verbs[0].defaultProjectile;
		VerbProperties verbProperties = gun.Verbs[0];
		List<StatModifier> list = gun.statBases.ListFullCopy();
		List<VerbProperties> verbs = gun.verbs;
		List<CompProperties> comps = gun.comps;
		List<Tool> tools = gun.tools;
		List<string> weaponTags = gun.weaponTags;
		gun.weaponTags = new List<string>(weaponTags);
		gun.verbs = new List<VerbProperties>();
		gun.verbs.Add(preset.gunStats.MemberwiseClone());
		if (preset.rangeCurve != null)
		{
			gun.verbs[0].range = preset.rangeCurve.Evaluate(verbProperties.range);
		}
		if (preset.warmupCurve != null)
		{
			gun.verbs[0].warmupTime = preset.warmupCurve.Evaluate(verbProperties.warmupTime);
		}
		if (gun.comps == null)
		{
			gun.comps = new List<CompProperties>();
		}
		else
		{
			gun.comps = new List<CompProperties>(gun.comps);
		}
		float value = preset.Mass;
		float bulk = preset.Bulk;
		float value2 = preset.CooldownTime;
		LabelGun labelGun = null;
		try
		{
			if (preset.MassCurve != null)
			{
				value = preset.MassCurve.Evaluate(gun.GetStatValueAbstract(StatDefOf.Mass));
			}
			if (preset.specialGuns.Any((LabelGun x) => x.names.Intersect(gun.label.ToLower().Replace("-", "").Split(' ')
				.ToList()).Any()))
			{
				labelGun = preset.specialGuns.Find((LabelGun x) => x.names.Intersect(gun.label.ToLower().Replace("-", "").Split(' ')
					.ToList()).Any());
				gun.comps.Add(new CompProperties_AmmoUser
				{
					ammoSet = labelGun.caliber,
					reloadTime = labelGun.reloadTime,
					magazineSize = labelGun.magCap,
					reloadOneAtATime = preset.reloadOneAtATime
				});
				value = labelGun.mass;
				bulk = labelGun.bulk;
				Log.Message(labelGun.names.First().Colorize(Color.yellow));
			}
			else
			{
				gun.comps.Add(new CompProperties_AmmoUser
				{
					ammoSet = preset.setCaliber,
					reloadTime = preset.ReloadTime,
					magazineSize = preset.AmmoCapacity,
					reloadOneAtATime = preset.reloadOneAtATime
				});
			}
			if (preset.DetermineCaliber)
			{
				CompProperties_AmmoUser compProperties_AmmoUser = gun.comps.Find((CompProperties x) => x is CompProperties_AmmoUser) as CompProperties_AmmoUser;
				compProperties_AmmoUser.ammoSet = preset.CaliberRanges.Find((CaliberFloatRange x) => x.DamageRange.Includes(OldProj.projectile.GetDamageAmount(1f)) && x.SpeedRange.Includes(OldProj.projectile.speed))?.AmmoSet ?? compProperties_AmmoUser.ammoSet;
			}
			if (preset.cooldownCurve != null)
			{
				value2 = preset.cooldownCurve.Evaluate(gun.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown));
			}
			if (preset.addTags != null)
			{
				foreach (string addTag in preset.addTags)
				{
					gun.weaponTags.Add(addTag);
				}
			}
			if (gun.tools != null)
			{
				List<Tool> list2 = new List<Tool>();
				foreach (Tool tool in gun.tools)
				{
					if (tool is ToolCE)
					{
						list2.Add(tool);
						continue;
					}
					Tool item;
					try
					{
						item = tool.ConvertTool();
					}
					catch
					{
						item = tool;
						Log.Warning($"Failed to convert {tool} to ToolCE");
					}
					list2.Add(item);
				}
				gun.tools = list2;
			}
		}
		catch (Exception ex)
		{
			try
			{
				Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, ex?.ToString() ?? "", StackTraceUtility.ExtractStringFromException(ex)));
			}
			catch (Exception arg)
			{
				Log.Error($"Cannot extract stack trace from {ex}: Failed with exception {arg}");
			}
			Log.Error($"Failed auto patching {gun}.  Rolling back");
			gun.comps = comps;
			gun.tools = tools;
			gun.verbs = verbs;
			gun.weaponTags = weaponTags;
		}
		gun.comps.Add(preset.fireModes);
		gun.AddOrChangeStat(new StatModifier
		{
			stat = StatDefOf.Mass,
			value = value
		});
		gun.AddOrChangeStat(new StatModifier
		{
			stat = CE_StatDefOf.Bulk,
			value = bulk
		});
		gun.AddOrChangeStat(new StatModifier
		{
			stat = StatDefOf.RangedWeapon_Cooldown,
			value = value2
		});
		gun.statBases.RemoveAll((StatModifier x) => x.stat.label.ToLower().Contains("accuracy"));
		gun.statBases.Add(new StatModifier
		{
			stat = CE_StatDefOf.ShotSpread,
			value = preset.Spread
		});
		gun.statBases.Add(new StatModifier
		{
			stat = CE_StatDefOf.SwayFactor,
			value = preset.Sway
		});
		if (preset.MiscOtherStats != null)
		{
			gun.statBases.AddRange(preset.MiscOtherStats);
		}
		if (labelGun != null && labelGun.stats != null)
		{
			gun.MergeStatLists(labelGun.stats);
		}
		if (preset.addBipods)
		{
			BipodCategoryDef bipodCategoryDef = DefDatabase<BipodCategoryDef>.AllDefsListForReading.Find((BipodCategoryDef x) => x.bipod_id == preset.bipodTag);
			if (bipodCategoryDef != null)
			{
				gun.comps.Add(new CompProperties_BipodComp
				{
					catDef = bipodCategoryDef,
					warmupPenalty = bipodCategoryDef.warmup_mult_NOT_setup,
					warmupMult = bipodCategoryDef.warmup_mult_setup,
					ticksToSetUp = bipodCategoryDef.setuptime,
					recoilMultoff = bipodCategoryDef.recoil_mult_NOT_setup,
					recoilMulton = bipodCategoryDef.recoil_mult_setup,
					additionalrange = bipodCategoryDef.ad_Range,
					swayMult = bipodCategoryDef.swayMult,
					swayPenalty = bipodCategoryDef.swayPenalty
				});
				gun.statBases.Add(new StatModifier
				{
					value = 0f,
					stat = CE_StatDefOf.BipodStats
				});
			}
		}
	}
}
