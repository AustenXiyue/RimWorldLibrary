using CombatExtended.RocketGUI;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Command), "DrawIcon")]
public static class Harmony_Command
{
	public static bool Prefix(Command __instance, Rect rect, GizmoRenderParms parms, Material buttonMat)
	{
		Command_VerbTarget command = __instance as Command_VerbTarget;
		if (command != null)
		{
			ThingWithComps thingWithComps = command.verb?.EquipmentSource;
			WeaponPlatform platform = thingWithComps as WeaponPlatform;
			if (platform != null)
			{
				global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
				{
					rect.position += new Vector2(command.iconOffset.x * rect.size.x, command.iconOffset.y * rect.size.y);
					Color color = GUI.color;
					color = ((((Gizmo)command).disabled && !parms.lowLight) ? command.IconDrawColor.SaturationChanged(0f) : command.IconDrawColor);
					if (parms.lowLight)
					{
						color = GUI.color.ToTransparent(0.6f);
					}
					float num = rect.width * 0.15f / 2f;
					rect.xMin += num;
					rect.xMax -= num;
					float num2 = rect.height * 0.15f / 2f;
					rect.yMin += num2;
					rect.yMax -= num2;
					global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(rect, platform, null, color, buttonMat);
				});
				return false;
			}
		}
		return true;
	}
}
