using HarmonyLib;
using UnityEngine;
using Verse;

namespace CombatExtended.Lasers;

[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
public static class Harmony_PawnRenderer_Draw_EquipmentAiming_GunDrawing_Patch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(ref Thing eq, ref Vector3 drawLoc, ref float aimAngle)
	{
		if (eq is IDrawnWeaponWithRotation drawnWeaponWithRotation)
		{
			drawLoc -= new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
			aimAngle = (aimAngle + drawnWeaponWithRotation.RotationOffset) % 360f;
			drawLoc += new Vector3(0f, 0f, 0.4f).RotatedBy(aimAngle);
		}
	}
}
