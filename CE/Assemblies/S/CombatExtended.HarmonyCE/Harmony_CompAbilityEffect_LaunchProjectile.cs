using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(CompAbilityEffect_LaunchProjectile), "LaunchProjectile")]
internal static class Harmony_CompAbilityEffect_LaunchProjectile
{
	internal static bool Prefix(CompAbilityEffect_LaunchProjectile __instance, LocalTargetInfo target)
	{
		Type thingClass = __instance.Props.projectileDef.thingClass;
		if (thingClass.IsSubclassOf(typeof(ProjectileCE)) || thingClass == typeof(ProjectileCE))
		{
			ThingDef projectile = __instance.Props.projectileDef.GetProjectile();
			if (projectile.projectile is ProjectilePropertiesCE projectilePropertiesCE)
			{
				Pawn pawn = __instance.parent.pawn;
				Vector3 vector = pawn.TrueCenter();
				Vector2 vector2 = default(Vector2);
				vector2.Set(vector.x, vector.z);
				Vector2 vector3 = default(Vector2);
				if (target.HasThing)
				{
					vector3.Set(target.Thing.TrueCenter().x, target.Thing.TrueCenter().z);
				}
				else
				{
					vector3.Set(target.Cell.ToIntVec2.x, target.Cell.ToIntVec2.z);
				}
				Vector2 vector4 = vector3 - vector2;
				float shotRotation = (-90f + 57.29578f * Mathf.Atan2(vector4.y, vector4.x)) % 360f;
				CollisionVertical collisionVertical = new CollisionVertical(target.Thing);
				float shotAngle = ProjectileCE.GetShotAngle(projectilePropertiesCE.speed, (target.Cell - pawn.Position).LengthHorizontal, collisionVertical.HeightRange.Average - 1f, projectilePropertiesCE.flyOverhead, projectilePropertiesCE.Gravity);
				CE_Utility.LaunchProjectileCE(projectile, vector2, target, pawn, shotAngle, shotRotation, 1f, projectilePropertiesCE.speed);
				return false;
			}
		}
		return true;
	}
}
