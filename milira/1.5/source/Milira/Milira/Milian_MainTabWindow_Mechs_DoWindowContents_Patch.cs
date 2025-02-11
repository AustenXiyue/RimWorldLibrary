using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Milira;

[HarmonyPatch(typeof(MainTabWindow_Mechs))]
[HarmonyPatch("DoWindowContents")]
public static class Milian_MainTabWindow_Mechs_DoWindowContents_Patch
{
	[HarmonyPostfix]
	public static void Postfix(Rect rect)
	{
		foreach (Pawn item in MechanitorUtility.MechsInPlayerFaction())
		{
			if (MilianUtility.IsMilian(item))
			{
				item?.Drawer?.renderer?.renderTree?.SetDirty();
			}
		}
	}
}
