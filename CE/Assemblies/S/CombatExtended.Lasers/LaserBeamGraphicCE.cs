using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace CombatExtended.Lasers;

[StaticConstructorOnStartup]
public class LaserBeamGraphicCE : Thing
{
	public LaserBeamDefCE projDef;

	private float beamWidth;

	private float beamLength;

	private int ticks;

	private int colorIndex = 2;

	private Vector3 a;

	private Vector3 b;

	public Matrix4x4 drawingMatrix = default(Matrix4x4);

	private Material materialBeam;

	private Mesh mesh;

	private Thing launcher;

	private Thing equipment;

	private ThingDef equipmentDef;

	public List<Mesh> meshes = new List<Mesh>();

	public int ticksToDetonation;

	private static Material BeamMat;

	private static Material BeamEndMat;

	private static MaterialPropertyBlock MatPropertyBlock;

	public float Opacity => (float)Math.Sin(Math.Pow(1.0 - 1.0 * (double)ticks / (double)projDef.lifetime, projDef.impulse) * Math.PI);

	public bool Lightning => projDef.LightningBeam;

	public bool Static => projDef.StaticLightning;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref beamWidth, "beamWidth", 0f);
		Scribe_Values.Look(ref beamLength, "beamLength", 0f);
		Scribe_Values.Look(ref ticks, "ticks", 0);
		Scribe_Values.Look(ref colorIndex, "colorIndex", 0);
		Scribe_Values.Look(ref a, "a");
		Scribe_Values.Look(ref b, "b");
		Scribe_Defs.Look(ref projDef, "projectileDef");
		Scribe_References.Look(ref equipment, "equipment");
	}

	public override void Tick()
	{
		if (def == null || ticks++ > projDef.lifetime)
		{
			Destroy();
		}
		if (ticksToDetonation > 0)
		{
			ticksToDetonation--;
			if (ticksToDetonation <= 0)
			{
				Explode();
			}
		}
	}

	private void SetColor(Thing launcher)
	{
		IBeamColorThing beamColorThing = null;
		Pawn pawn = launcher as Pawn;
		if (pawn != null && pawn.equipment != null)
		{
			beamColorThing = pawn.equipment.Primary as IBeamColorThing;
		}
		if (beamColorThing == null)
		{
			beamColorThing = launcher as IBeamColorThing;
		}
		if (beamColorThing != null && beamColorThing.BeamColor != -1)
		{
			colorIndex = beamColorThing.BeamColor;
		}
		if (beamColorThing != null)
		{
			equipmentDef = pawn.equipment.Primary.def;
		}
	}

	public void Setup(Thing launcher, Thing equipment, Vector3 origin, Vector3 destination)
	{
		this.launcher = launcher;
		a = origin;
		b = destination;
	}

	public void SetupDrawing()
	{
		if (mesh != null)
		{
			return;
		}
		materialBeam = projDef.GetBeamMaterial(colorIndex) ?? BeamMat;
		if (def.graphicData != null && def.graphicData.graphicClass != null && def.graphicData.graphicClass == typeof(Graphic_Random))
		{
			materialBeam = projDef.GetBeamMaterial(0) ?? def.graphicData.Graphic.MatSingle;
		}
		beamWidth = projDef.beamWidth;
		Quaternion q = Quaternion.LookRotation(b - a);
		Vector3 normalized = (b - a).normalized;
		beamLength = (b - a).magnitude;
		Vector3 s = new Vector3(beamWidth, 1f, beamLength);
		Vector3 pos = (a + b) / 2f;
		pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
		drawingMatrix.SetTRS(pos, q, s);
		float num = 1f * (float)materialBeam.mainTexture.width / (float)materialBeam.mainTexture.height;
		float num2 = ((projDef.seam < 0f) ? num : projDef.seam);
		float num3 = beamWidth / num / 2f * num2;
		float sv = ((beamLength <= num3 * 2f) ? 0.5f : (num3 * 2f / beamLength));
		if (Lightning)
		{
			float num4 = Vector3.Distance(a, b);
			for (int i = 0; i < projDef.ArcCount; i++)
			{
				Mesh item = LightningLaserBoltMeshMaker.NewBoltMesh(0f - (num4 + 0.25f), projDef.LightningVariance, beamWidth);
				meshes.Add(item);
			}
		}
		else
		{
			mesh = MeshMakerLaser.Mesh(num2, sv);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (def == null || projDef.decorations == null || respawningAfterLoad)
		{
			return;
		}
		foreach (LaserBeamDecoration decoration in projDef.decorations)
		{
			float num = decoration.spacing * projDef.beamWidth;
			float num2 = decoration.initialOffset * projDef.beamWidth;
			Vector3 normalized = (b - a).normalized;
			float num3 = (b - a).AngleFlat();
			Vector3 vector = normalized * num;
			Vector3 exactPosition = a + vector * 0.5f + normalized * num2;
			float num4 = (b - a).magnitude - num;
			int num5 = 0;
			while (num4 > 0f && ThingMaker.MakeThing(decoration.mote) is MoteLaserDectorationCE moteLaserDectorationCE)
			{
				moteLaserDectorationCE.beam = this;
				moteLaserDectorationCE.airTimeLeft = projDef.lifetime;
				moteLaserDectorationCE.Scale = projDef.beamWidth;
				moteLaserDectorationCE.exactRotation = num3;
				moteLaserDectorationCE.exactPosition = exactPosition;
				moteLaserDectorationCE.SetVelocity(num3, decoration.speed);
				moteLaserDectorationCE.baseSpeed = decoration.speed;
				moteLaserDectorationCE.speedJitter = decoration.speedJitter;
				moteLaserDectorationCE.speedJitterOffset = decoration.speedJitterOffset * (float)num5;
				GenSpawn.Spawn(moteLaserDectorationCE, a.ToIntVec3(), map);
				exactPosition += vector;
				num4 -= num;
				num5++;
			}
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		SetupDrawing();
		float opacity = Opacity;
		Color color = projDef.graphicData.color;
		color.a *= opacity;
		MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
		if (Lightning)
		{
			Vector3 vector = default(Vector3);
			vector.x = b.x;
			vector.y = b.y;
			vector.z = b.z;
			float num = Vector3.Distance(a, b);
			for (int i = 0; i < projDef.ArcCount; i++)
			{
				if (def.graphicData != null && def.graphicData.graphicClass != null && def.graphicData.graphicClass == typeof(Graphic_Flicker) && !Find.TickManager.Paused && Find.TickManager.TicksGame % projDef.flickerFrameTime == 0)
				{
					materialBeam = projDef.GetBeamMaterial((projDef.materials.IndexOf(materialBeam) + 1 < projDef.materials.Count) ? (projDef.materials.IndexOf(materialBeam) + 1) : 0) ?? def.graphicData.Graphic.MatSingle;
				}
				meshes[i] = ((Find.TickManager.Paused || Find.TickManager.TicksGame % projDef.flickerFrameTime != 0 || (meshes[i] != null && Static)) ? meshes[i] : LightningLaserBoltMeshMaker.NewBoltMesh(0f - (num + 0.25f), projDef.LightningVariance, beamWidth));
				Graphics.DrawMesh(meshes[i], b, Quaternion.LookRotation((vector - a).normalized), materialBeam, 0, null, 0, MatPropertyBlock, ShadowCastingMode.Off);
			}
		}
		else
		{
			if (def.graphicData != null && def.graphicData.graphicClass != null && def.graphicData.graphicClass == typeof(Graphic_Flicker) && !Find.TickManager.Paused && Find.TickManager.TicksGame % projDef.flickerFrameTime == 0)
			{
				materialBeam = projDef.GetBeamMaterial((projDef.materials.IndexOf(materialBeam) + 1 < projDef.materials.Count) ? (projDef.materials.IndexOf(materialBeam) + 1) : 0) ?? def.graphicData.Graphic.MatSingle;
			}
			Graphics.DrawMesh(mesh, drawingMatrix, materialBeam, 0, null, 0, MatPropertyBlock);
		}
	}

	public virtual void Explode()
	{
		Map map = base.Map;
		IntVec3 intVec = b.ToIntVec3();
		if (def.projectile.explosionEffect != null)
		{
			Effecter effecter = def.projectile.explosionEffect.Spawn();
			effecter.Trigger(new TargetInfo(intVec, map), new TargetInfo(intVec, map));
			effecter.Cleanup();
		}
		IntVec3 center = intVec;
		Map map2 = map;
		float explosionRadius = def.projectile.explosionRadius;
		DamageDef damageDef = def.projectile.damageDef;
		Thing instigator = launcher;
		float weaponDamageMultiplier = equipment?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
		int damageAmount = def.projectile.GetDamageAmount(weaponDamageMultiplier);
		SoundDef soundExplode = def.projectile.soundExplode;
		ThingDef weapon = equipmentDef;
		ThingDef projectile = def;
		ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
		float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
		int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
		ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
		GenExplosion.DoExplosion(center, map2, explosionRadius, damageDef, instigator, damageAmount, 0f, soundExplode, weapon, projectile, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, null, def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, def.projectile.preExplosionSpawnChance, def.projectile.preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire, def.projectile.explosionDamageFalloff, null, null, null);
	}

	static LaserBeamGraphicCE()
	{
		BeamMat = MaterialPool.MatFrom("Other/OrbitalBeam", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
		BeamEndMat = MaterialPool.MatFrom("Other/OrbitalBeamEnd", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.OrbitalBeam);
		MatPropertyBlock = new MaterialPropertyBlock();
	}
}
