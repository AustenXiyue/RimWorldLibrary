using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.Lasers;

public class LaserBeamCE : BulletCE
{
	public float DamageModifier = 1f;

	public override float DamageAmount => base.DamageAmount * DamageModifier;

	public LaserBeamDefCE laserBeamDef => def as LaserBeamDefCE;

	public Vector3 destination => new Vector3(Destination.x, 0f, Destination.y);

	public Vector3 Origin => new Vector3(origin.x, 0f, origin.y);

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
	}

	private void TriggerEffect(EffecterDef effect, Vector3 position, Thing hitThing = null)
	{
		TriggerEffect(effect, IntVec3.FromVector3(position));
	}

	private void TriggerEffect(EffecterDef effect, IntVec3 dest)
	{
		if (effect != null)
		{
			TargetInfo targetInfo = new TargetInfo(dest, base.Map);
			Effecter effecter = effect.Spawn();
			effecter.Trigger(targetInfo, targetInfo);
			effecter.Cleanup();
		}
	}

	public void SpawnBeam(Vector3 a, Vector3 b)
	{
		if (ThingMaker.MakeThing(laserBeamDef.beamGraphic) is LaserBeamGraphicCE laserBeamGraphicCE)
		{
			laserBeamGraphicCE.ticksToDetonation = def.projectile.explosionDelay;
			laserBeamGraphicCE.projDef = laserBeamDef;
			laserBeamGraphicCE.Setup(launcher, equipment, a, b);
			GenSpawn.Spawn(laserBeamGraphicCE, Origin.ToIntVec3(), base.Map);
		}
	}

	private void SpawnBeamReflections(Vector3 a, Vector3 b, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 normalized = (b - a).normalized;
			Rand.PushState();
			Vector3 b2 = b - normalized.RotatedBy(Rand.Range(-22.5f, 22.5f)) * Rand.Range(1f, 4f);
			Rand.PopState();
			SpawnBeam(b, b2);
		}
	}

	public override void Impact(Thing hitThing)
	{
		LaserGunDef laserGunDef = equipmentDef as LaserGunDef;
		Vector3 normalized = (destination - Origin).normalized;
		Vector3 muzzle = Origin + normalized * (laserGunDef?.barrelLength ?? 0.9f);
		Impact(hitThing, muzzle);
	}

	public void Impact(Thing hitThing, Vector3 muzzle)
	{
		bool flag = hitThing.IsShielded() && laserBeamDef.IsWeakToShields;
		LaserGunDef laserGunDef = equipmentDef as LaserGunDef;
		Vector3 normalized = (destination - muzzle).normalized;
		Vector3 b;
		if (hitThing == null)
		{
			b = destination;
		}
		else if (flag)
		{
			b = hitThing.TrueCenter() - normalized.RotatedBy(Rand.Range(-22.5f, 22.5f)) * 0.8f;
		}
		else if ((destination - hitThing.TrueCenter()).magnitude < 1f)
		{
			b = destination;
		}
		else
		{
			b = hitThing.TrueCenter();
			b.x += Rand.Range(-0.5f, 0.5f);
			b.z += Rand.Range(-0.5f, 0.5f);
		}
		Pawn pawn = launcher as Pawn;
		IDrawnWeaponWithRotation drawnWeaponWithRotation = null;
		if (pawn != null && pawn.equipment != null)
		{
			drawnWeaponWithRotation = pawn.equipment.Primary as IDrawnWeaponWithRotation;
		}
		if (drawnWeaponWithRotation == null && launcher is Building_LaserGunCE building_LaserGunCE)
		{
			drawnWeaponWithRotation = building_LaserGunCE.Gun as IDrawnWeaponWithRotation;
		}
		if (hitThing is Pawn && flag)
		{
			DamageModifier *= laserBeamDef.shieldDamageMultiplier;
			SpawnBeamReflections(muzzle, b, 5);
		}
		base.Impact(hitThing);
	}
}
