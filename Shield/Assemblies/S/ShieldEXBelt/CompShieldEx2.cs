using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ShieldEXBelt;

[StaticConstructorOnStartup]
public class CompShieldEx2 : CompShield
{
	private Vector3 impactAngleVect;

	private int lastAbsorbDamageTick = -9999;

	private static readonly Material BubbleMat = MaterialPool.MatFrom("Shield/ShieldBubbleVoid", ShaderDatabase.Transparent);

	public override void CompTick()
	{
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		base.Notify_Equipped(pawn);
		if (base.Energy == 0f)
		{
			AccessTools.Field(typeof(CompShield), "energy").SetValue(this, parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
		}
	}

	public override void CompDrawWornExtras()
	{
		if (base.IsApparel)
		{
			Draw();
		}
	}

	private void Draw()
	{
		if (base.ShieldState == ShieldState.Active && base.ShouldDisplay)
		{
			float num = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, energy);
			Vector3 drawPos = base.PawnOwner.Drawer.DrawPos;
			drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
			if (num2 < 8)
			{
				float num3 = (float)(8 - num2) / 8f * 0.05f;
				drawPos += impactAngleVect * num3;
				num -= num3;
			}
			float angle = Rand.Range(0, 360);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
		}
	}

	private void AbsorbedDamage(DamageInfo dinfo)
	{
		SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.PawnOwner.Position, base.PawnOwner.Map));
		impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
		Vector3 loc = base.PawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
		float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
		FleckMaker.Static(loc, base.PawnOwner.Map, FleckDefOf.ExplosionFlash, num);
		int num2 = (int)num;
		for (int i = 0; i < num2; i++)
		{
			FleckMaker.ThrowDustPuff(loc, base.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
		}
		lastAbsorbDamageTick = Find.TickManager.TicksGame;
		KeepDisplaying();
	}

	private void Break()
	{
		float scale = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, energy);
		EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
		FleckMaker.Static(base.PawnOwner.TrueCenter(), base.PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
		}
		energy = 0f;
		ticksToReset = base.Props.startingTicksToReset;
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
		if (base.ShieldState != 0 || base.PawnOwner == null || dinfo.Def == DamageDefOf.SurgicalCut)
		{
			return;
		}
		if (dinfo.Def == DamageDefOf.Extinguish)
		{
			AbsorbedDamage(dinfo);
			absorbed = true;
			return;
		}
		energy -= dinfo.Amount * base.Props.energyLossPerDamage;
		if (energy <= 0f)
		{
			Break();
			parent.Destroy();
		}
		else
		{
			parent.HitPoints = (int)((float)parent.HitPoints * (energy / parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax)));
			AbsorbedDamage(dinfo);
		}
		absorbed = true;
	}
}
