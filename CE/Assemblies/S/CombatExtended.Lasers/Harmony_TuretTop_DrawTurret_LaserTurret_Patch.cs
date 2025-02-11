using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.Lasers;

[HarmonyPatch(typeof(TurretTop), "DrawTurret")]
[StaticConstructorOnStartup]
internal class Harmony_TuretTop_DrawTurret_LaserTurret_Patch
{
	private static FieldInfo parentTurretField;

	private static FieldInfo curRotationIntField;

	static Harmony_TuretTop_DrawTurret_LaserTurret_Patch()
	{
		parentTurretField = typeof(TurretTop).GetField("parentTurret", BindingFlags.Instance | BindingFlags.NonPublic);
		curRotationIntField = typeof(TurretTop).GetField("curRotationInt", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	private static bool Prefix(TurretTop __instance, float ___curRotationInt, Building_Turret ___parentTurret, Vector3 recoilDrawOffset, float recoilAngleOffset)
	{
		if (!(___parentTurret is Building_LaserGunCE building_LaserGunCE))
		{
			return true;
		}
		float num = ___curRotationInt;
		if (building_LaserGunCE.TargetCurrentlyAimingAt.HasThing)
		{
			num = (building_LaserGunCE.TargetCurrentlyAimingAt.CenterVector3 - building_LaserGunCE.TrueCenter()).AngleFlat();
		}
		if (building_LaserGunCE.Gun is IDrawnWeaponWithRotation drawnWeaponWithRotation)
		{
			num += drawnWeaponWithRotation.RotationOffset;
		}
		Material material = ___parentTurret.def.building.turretTopMat;
		if (building_LaserGunCE.Gun is SpinningLaserGunTurret spinningLaserGunTurret)
		{
			spinningLaserGunTurret.turret = building_LaserGunCE;
			material = spinningLaserGunTurret.Graphic.MatSingle;
		}
		Vector3 vector = new Vector3(___parentTurret.def.building.turretTopOffset.x, 0f, ___parentTurret.def.building.turretTopOffset.y);
		float turretTopDrawSize = ___parentTurret.def.building.turretTopDrawSize;
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(building_LaserGunCE.DrawPos + Altitudes.AltIncVect + vector, (num + (float)TurretTop.ArtworkRotation).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
		Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
		return false;
	}
}
