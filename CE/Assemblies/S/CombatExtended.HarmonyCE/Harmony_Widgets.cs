using System.Collections.Generic;
using CombatExtended.RocketGUI;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(Widgets), "DefIcon")]
public static class Harmony_Widgets
{
	private static readonly List<AttachmentLink> empty = new List<AttachmentLink>();

	public static bool Prefix(Rect rect, Def def, float scale, Color? color)
	{
		if (def is WeaponPlatformDef weaponPlatformDef)
		{
			float num = rect.width * (1f - scale) / 2f;
			rect.xMin += num;
			rect.xMax -= num;
			float num2 = rect.width * (1f - scale) / 2f;
			rect.yMin += num2;
			rect.yMax -= num2;
			global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(rect, weaponPlatformDef, empty, weaponPlatformDef.defaultGraphicParts, null, color);
			return false;
		}
		return true;
	}
}
