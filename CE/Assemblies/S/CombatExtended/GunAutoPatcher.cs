using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class GunAutoPatcher
{
	private static bool shouldPatch(ThingDef thingDef)
	{
		List<string> weaponTags = thingDef.weaponTags;
		if (weaponTags != null && weaponTags.Contains("Patched"))
		{
			return false;
		}
		if (thingDef.thingClass != typeof(ThingWithComps) && thingDef.thingClass != typeof(Thing))
		{
			return false;
		}
		if (!thingDef.IsRangedWeapon)
		{
			return false;
		}
		List<VerbProperties> verbs = thingDef.verbs;
		if (verbs != null)
		{
			foreach (VerbProperties item in verbs)
			{
				if (item is VerbPropertiesCE)
				{
					return false;
				}
				Type type = item.defaultProjectile?.thingClass;
				if ((object)type != null)
				{
					if (type != typeof(Bullet) && type != typeof(Projectile_Explosive))
					{
						return false;
					}
					Type verbClass = item.verbClass;
					if (verbClass != typeof(Verb_ShootOneUse) && verbClass != typeof(Verb_Shoot) && verbClass != typeof(Verb_LaunchProjectile) && verbClass != typeof(Verb_LaunchProjectileStatic))
					{
						return false;
					}
					continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	static GunAutoPatcher()
	{
		if (!Controller.settings.EnableWeaponAutopatcher)
		{
			return;
		}
		IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where(shouldPatch);
		IEnumerable<GunPatcherPresetDef> allDefs = DefDatabase<GunPatcherPresetDef>.AllDefs;
		IEnumerable<ThingDef> enumerable2 = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.tools != null && x.tools.All((Tool y) => !(y is ToolCE)));
		foreach (GunPatcherPresetDef item2 in allDefs)
		{
			try
			{
				enumerable.PatchGunsFromPreset(item2);
			}
			catch (Exception ex)
			{
				Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, ex?.ToString() ?? "", StackTraceUtility.ExtractStringFromException(ex)));
				Log.Error($"Unhandled exception handling {item2}");
			}
		}
		if (enumerable.Count() > 0)
		{
			foreach (ThingDef item3 in enumerable)
			{
				try
				{
					item3.PatchGunFromPreset(allDefs.MaxBy((GunPatcherPresetDef x) => x.DamageRange.Average + x.RangeRange.Average + x.ProjSpeedRange.Average + x.WarmupRange.Average));
				}
				catch (Exception ex2)
				{
					Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, ex2?.ToString() ?? "", StackTraceUtility.ExtractStringFromException(ex2)));
					Log.Error($"Unhandled exception patching gun {item3} from preset");
				}
			}
		}
		foreach (ThingDef item4 in enumerable2)
		{
			List<Tool> list = new List<Tool>();
			foreach (Tool tool in item4.tools)
			{
				Tool item;
				try
				{
					item = tool.ConvertTool();
				}
				catch
				{
					Log.Error($"Failed to autoconvert tool {tool} in {item4}.  Using original");
					item = tool;
				}
				list.Add(item);
			}
			item4.tools = list;
		}
	}
}
