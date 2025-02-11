using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProjectileImpactFX.HarmonyInstance;

[HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo")]
public static class Verb_TryCastNextBurstShot_MuzzlePosition_Transpiler
{
	public static bool Oversized = false;

	public static float meleeXOffset = 0.4f;

	public static float rangedXOffset = 0.1f;

	public static float meleeZOffset = 0f;

	public static float rangedZOffset = 0f;

	public static float meleeAngle = 270f;

	public static bool meleeMirrored = true;

	public static float rangedAngle = 135f;

	public static bool rangedMirrored = true;

	public static void ThrowMuzzleFlash(IntVec3 cell, Map map, ThingDef moteDef, float scale, Verb verb)
	{
		if (verb.EquipmentSource != null && verb.verbProps.range > 1.48f)
		{
			Vector3 vector = (verb.CasterIsPawn ? verb.CasterPawn.Drawer.DrawPos : verb.Caster.DrawPos);
			CompEquippable compEquippable = verb.EquipmentSource.TryGetComp<CompEquippable>();
			float num = (verb.CurrentTarget.CenterVector3 - vector).AngleFlat();
			if (verb.EquipmentSource.def.HasModExtension<BarrelOffsetExtension>())
			{
				BarrelOffsetExtension modExtension = verb.EquipmentSource.def.GetModExtension<BarrelOffsetExtension>();
				EffectProjectileExtension effectProjectileExtension = (verb.GetProjectile().HasModExtension<EffectProjectileExtension>() ? verb.GetProjectile().GetModExtension<EffectProjectileExtension>() : null);
				float barrellength = modExtension.barrellength;
				vector += (verb.CurrentTarget.CenterVector3 - vector).normalized * (verb.EquipmentSource.def.graphic.drawSize.magnitude * barrellength);
				if (effectProjectileExtension != null && effectProjectileExtension.muzzleFlare)
				{
					ThingDef named = DefDatabase<ThingDef>.GetNamed((!effectProjectileExtension.muzzleSmokeDef.NullOrEmpty()) ? effectProjectileExtension.muzzleFlareDef : "Mote_SparkFlash");
					MoteMaker.MakeStaticMote(vector, map, named, effectProjectileExtension.muzzleFlareSize);
				}
				else if (modExtension.muzzleFlare)
				{
					ThingDef named2 = DefDatabase<ThingDef>.GetNamed((!modExtension.muzzleSmokeDef.NullOrEmpty()) ? modExtension.muzzleFlareDef : "Mote_SparkFlash");
					MoteMaker.MakeStaticMote(vector, map, named2, modExtension.muzzleFlareSize);
				}
				if (effectProjectileExtension != null && effectProjectileExtension.muzzleSmoke)
				{
					string defName = ((!effectProjectileExtension.muzzleSmokeDef.NullOrEmpty()) ? effectProjectileExtension.muzzleSmokeDef : "OG_Mote_SmokeTrail");
					TrailThrower.ThrowSmoke(vector, effectProjectileExtension.muzzleSmokeSize, map, defName);
				}
				else if (modExtension.muzzleSmoke)
				{
					string defName2 = ((!modExtension.muzzleSmokeDef.NullOrEmpty()) ? modExtension.muzzleSmokeDef : "OG_Mote_SmokeTrail");
					TrailThrower.ThrowSmoke(vector, modExtension.muzzleSmokeSize, map, defName2);
				}
			}
			MoteMaker.MakeStaticMote(vector, map, moteDef, scale);
		}
		else
		{
			MoteMaker.MakeStaticMote(cell.ToVector3Shifted(), map, moteDef, scale);
		}
	}

	public static void SetAnglesAndOffsets(Thing eq, ThingWithComps offHandEquip, float aimAngle, Thing thing, ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle, bool mainHandAiming, bool offHandAiming)
	{
		Pawn pawn = thing as Pawn;
		bool flag = pawn != null;
		if (flag)
		{
			flag = IsMeleeWeapon(pawn.equipment.Primary);
		}
		bool flag2 = false;
		float num = (meleeMirrored ? (360f - meleeAngle) : meleeAngle);
		float num2 = (rangedMirrored ? (360f - rangedAngle) : rangedAngle);
		Vector3 vector = AdjustRenderOffsetFromDir(thing.Rotation, null, offHandAiming);
		if (thing.Rotation == Rot4.East)
		{
			offsetMainHand.z += vector.z;
			offsetMainHand.x += vector.x;
			offsetOffHand.y = -1f;
			offsetOffHand.z = 0.1f;
			offsetOffHand.z += vector.z;
			offsetOffHand.x += vector.x;
			offHandAngle = mainHandAngle;
		}
		else if (thing.Rotation == Rot4.West)
		{
			if (flag2)
			{
				offsetMainHand.y = -1f;
			}
			offsetMainHand.z += vector.z;
			offsetMainHand.x += vector.x;
			offsetOffHand.z = -0.1f;
			offsetOffHand.z += vector.z;
			offsetOffHand.x += vector.x;
			offHandAngle = mainHandAngle;
		}
		else if (thing.Rotation == Rot4.North)
		{
			if (!mainHandAiming)
			{
				offsetMainHand.x = vector.x + ((!flag2) ? 0f : (flag ? meleeXOffset : rangedXOffset));
				offsetOffHand.x = 0f - vector.x + (flag ? (0f - meleeXOffset) : (0f - rangedXOffset));
				offsetMainHand.z = vector.z + ((!flag2) ? 0f : (flag ? meleeZOffset : rangedZOffset));
				offsetOffHand.z = vector.z + (flag ? meleeZOffset : rangedZOffset);
			}
			else
			{
				offsetOffHand.x = -0.1f;
			}
		}
		else if (!mainHandAiming)
		{
			offsetMainHand.y = 1f;
			offsetMainHand.x = 0f - vector.x + ((!flag2) ? 0f : (flag ? (0f - meleeXOffset) : (0f - rangedXOffset)));
			offsetOffHand.x = vector.x + (flag ? meleeXOffset : rangedXOffset);
			offsetMainHand.z = vector.z + ((!flag2) ? 0f : (flag ? meleeZOffset : rangedZOffset));
			offsetOffHand.z = vector.z + (flag ? meleeZOffset : rangedZOffset);
		}
		else
		{
			offsetOffHand.y = 1f;
			offHandAngle = ((!flag) ? num : num2);
			offsetOffHand.x = 0.1f;
		}
	}

	private static Vector3 AdjustRenderOffsetFromDir(Rot4 curDir, object compOversizedWeapon, bool Offhand = false)
	{
		return Vector3.zero;
	}

	private static bool IsMeleeWeapon(ThingWithComps eq)
	{
		bool result;
		if (eq == null)
		{
			result = false;
		}
		else
		{
			CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
			if (compEquippable != null && compEquippable.PrimaryVerb.IsMeleeAttack)
			{
				return true;
			}
			result = false;
		}
		return result;
	}
}
