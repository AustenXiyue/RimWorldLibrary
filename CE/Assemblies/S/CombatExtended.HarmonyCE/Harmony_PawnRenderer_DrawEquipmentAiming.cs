using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
internal static class Harmony_PawnRenderer_DrawEquipmentAiming
{
	public static Rot4 south = Rot4.South;

	private static Thing equipment;

	private static Vector3 recoilOffset = default(Vector3);

	private static float muzzleJump = 0f;

	private static Vector3 casingDrawPos;

	private static readonly Matrix4x4 TBot5 = Matrix4x4.Translate(new Vector3(0f, -0.006f, 0f));

	private static readonly Matrix4x4 TBot3 = Matrix4x4.Translate(new Vector3(0f, -0.004f, 0f));

	public static void Prefix(Thing eq, Vector3 drawLoc)
	{
		equipment = eq;
		casingDrawPos = drawLoc;
	}

	private static void RecoilCE(Thing eq, Vector3 position, float aimAngle, float num, CompEquippable compEquippable)
	{
		if (Controller.settings.RecoilAnim && compEquippable.PrimaryVerb.verbProps is VerbPropertiesCE)
		{
			CE_Utility.Recoil(eq.def, compEquippable.PrimaryVerb, out var drawOffset, out var angleOffset, aimAngle, handheld: true);
			recoilOffset = drawOffset;
			muzzleJump = angleOffset;
		}
	}

	private static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Vector3 position, float aimAngle)
	{
		GunDrawExtension gunDrawExtension = eq.def.GetModExtension<GunDrawExtension>() ?? new GunDrawExtension
		{
			DrawSize = eq.def.graphicData.drawSize
		};
		if (gunDrawExtension.DrawSize == Vector2.one)
		{
			gunDrawExtension.DrawSize = eq.def.graphicData.drawSize;
		}
		Vector3 s = new Vector3(gunDrawExtension.DrawSize.x, 1f, gunDrawExtension.DrawSize.y);
		Vector3 vector = new Vector3(gunDrawExtension.DrawOffset.x, 0f, gunDrawExtension.DrawOffset.y);
		Vector3 vector2 = new Vector3(gunDrawExtension.CasingOffset.x, 0f, gunDrawExtension.CasingOffset.y);
		if (aimAngle > 200f && aimAngle < 340f)
		{
			vector.x *= -1f;
			muzzleJump = 0f - muzzleJump;
			vector2.x *= -1f;
		}
		float y = matrix.rotation.eulerAngles.y;
		matrix.SetTRS(position + vector.RotatedBy(y) + recoilOffset, Quaternion.AngleAxis(y + muzzleJump, Vector3.up), s);
		CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
		if (compEquippable != null && compEquippable.PrimaryVerb is Verb_ShootCE verb_ShootCE)
		{
			verb_ShootCE.drawPos = casingDrawPos + (vector2 + vector).RotatedBy(y);
		}
		if (eq is WeaponPlatform weaponPlatform)
		{
			weaponPlatform.DrawPlatform(matrix, mesh == MeshPool.plane10Flip, layer);
		}
		else
		{
			Graphics.DrawMesh(mesh, matrix, mat, layer);
		}
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		CodeInstruction[] collection = (CodeInstruction[])(object)new CodeInstruction[6]
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Ldarg_1, (object)null),
			new CodeInstruction(OpCodes.Ldarg_2, (object)null),
			new CodeInstruction(OpCodes.Ldloc_1, (object)null),
			new CodeInstruction(OpCodes.Ldloc_2, (object)null),
			new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming), "RecoilCE", (Type[])null, (Type[])null))
		};
		bool flag = false;
		int index = 0;
		for (int i = 0; i < list.Count; i++)
		{
			CodeInstruction val = list[i];
			if (flag && val.opcode == OpCodes.Stloc_1)
			{
				index = i + 1;
				break;
			}
			if (val.opcode == OpCodes.Call && val.operand == typeof(EquipmentUtility).GetMethod("Recoil"))
			{
				flag = true;
			}
		}
		list.InsertRange(index, collection);
		list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming), "DrawMesh", (Type[])null, (Type[])null);
		list.InsertRange(list.Count - 2, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[3]
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Ldarg_1, (object)null),
			new CodeInstruction(OpCodes.Ldarg_2, (object)null)
		});
		return list;
	}
}
