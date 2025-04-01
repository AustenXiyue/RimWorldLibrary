using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ShieldEXBelt;

[StaticConstructorOnStartup]
public class CompShieldEx3 : CompShield
{
	private Vector3 impactAngleVect;

	private int lastAbsorbDamageTick = -9999;

	public bool shouldactive = true;

	private static readonly Material BubbleMat = MaterialPool.MatFrom("Shield/ShieldBubbleArcho", ShaderDatabase.Transparent);

	public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
	{
		if (!base.PawnOwner.Drafted || base.PawnOwner.GetPsylinkLevel() <= 0)
		{
			yield break;
		}
		yield return new Command_Toggle
		{
			defaultLabel = "SEB.Comps.ShieldSwitchLabel".Translate(),
			defaultDesc = "SEB.Comps.ShieldSwitchDesc".Translate(),
			alsoClickIfOtherInGroupClicked = true,
			activateIfAmbiguous = true,
			isActive = () => shouldactive,
			toggleAction = delegate
			{
				if (shouldactive)
				{
					shouldactive = false;
					if (base.ShieldState == ShieldState.Active)
					{
						Break();
					}
				}
				else
				{
					shouldactive = true;
					if (base.ShieldState != 0)
					{
						Reset();
					}
				}
			},
			icon = ContentFinder<Texture2D>.Get("Shield/ShieldBubbleArcho")
		};
	}

	public override void CompDrawWornExtras()
	{
		if (base.IsApparel && base.PawnOwner.GetPsylinkLevel() > 0)
		{
			Draw();
		}
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		base.Notify_Equipped(pawn);
		if (base.ShieldState == ShieldState.Active && base.PawnOwner.GetPsylinkLevel() >= 5)
		{
			pawn.health.AddHediff(SEB.ShieldBeltEnvironmentProtect, null, null);
		}
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		base.Notify_Unequipped(pawn);
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(SEB.ShieldBeltEnvironmentProtect);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
	}

	private void Draw()
	{
		if (base.ShieldState == ShieldState.Active && base.ShouldDisplay)
		{
			float num = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, base.PawnOwner.psychicEntropy.CurrentPsyfocus * 4f);
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
		float scale = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, base.PawnOwner.psychicEntropy.CurrentPsyfocus * 4f);
		EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
		FleckMaker.Static(base.PawnOwner.TrueCenter(), base.PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
		}
		Hediff firstHediffOfDef = base.PawnOwner.health.hediffSet.GetFirstHediffOfDef(SEB.ShieldBeltEnvironmentProtect);
		if (firstHediffOfDef != null)
		{
			base.PawnOwner.health.RemoveHediff(firstHediffOfDef);
		}
		ticksToReset = 1;
	}

	private void Reset()
	{
		if (base.PawnOwner.Spawned)
		{
			SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.PawnOwner.Position, base.PawnOwner.Map));
			FleckMaker.ThrowLightningGlow(base.PawnOwner.TrueCenter(), base.PawnOwner.Map, 3f);
		}
		ticksToReset = -1;
		if (base.PawnOwner.GetPsylinkLevel() >= 5)
		{
			base.PawnOwner.health.AddHediff(SEB.ShieldBeltEnvironmentProtect, null, null);
		}
	}

	public override void CompTick()
	{
		if (base.PawnOwner != null && shouldactive && base.ShieldState == ShieldState.Resetting && (double)(base.PawnOwner.psychicEntropy.EntropyValue / base.PawnOwner.psychicEntropy.MaxEntropy) <= 0.5)
		{
			Reset();
		}
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
		float num = base.PawnOwner.psychicEntropy.EntropyValue / base.PawnOwner.psychicEntropy.MaxEntropy - 1f;
		if (!Rand.Chance(num))
		{
			float num2 = dinfo.Amount / (1f + base.PawnOwner.psychicEntropy.CurrentPsyfocus);
			base.PawnOwner.psychicEntropy.TryAddEntropy(num2, null, scale: true, overLimit: true);
			float offset = (num2 - dinfo.Amount) / base.PawnOwner.psychicEntropy.MaxEntropy;
			base.PawnOwner.psychicEntropy.OffsetPsyfocusDirectly(offset);
			if (num >= 0f && base.PawnOwner.psychicEntropy.limitEntropyAmount)
			{
				Break();
			}
			else
			{
				AbsorbedDamage(dinfo);
			}
			absorbed = true;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref shouldactive, "shouldactive", defaultValue: false);
	}
}
