using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ShieldEXBelt;

[StaticConstructorOnStartup]
public class CompShieldEx4 : CompShield
{
	private Vector3 impactAngleVect;

	private int lastAbsorbDamageTick = -9999;

	public bool shouldactive = true;

	public float remember = 0f;

	private static readonly Material BubbleMat = MaterialPool.MatFrom("Shield/ShieldBubbleBlood", ShaderDatabase.Transparent);

	public float HemogenOffset => parent.GetStatValue(SEB.HemogenMaxOffset) / 100f;

	public Gene_Hemogen Hemogen
	{
		get
		{
			if (base.PawnOwner == null)
			{
				return null;
			}
			Gene_Hemogen gene_Hemogen = base.PawnOwner.genes?.GetFirstGeneOfType<Gene_Hemogen>();
			if (gene_Hemogen != null)
			{
				return gene_Hemogen;
			}
			return null;
		}
	}

	public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
	{
		if (!base.PawnOwner.Drafted || Hemogen == null)
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
					if (base.ShieldState != 0 && Hemogen != null && Hemogen.Value > 0f)
					{
						Reset();
					}
				}
			},
			icon = ContentFinder<Texture2D>.Get("Shield/ShieldBubbleBlood")
		};
	}

	public override void CompDrawWornExtras()
	{
		if (base.IsApparel && Hemogen != null)
		{
			Draw();
		}
	}

	private void Draw()
	{
		if (base.ShieldState == ShieldState.Active && base.ShouldDisplay)
		{
			float num = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, Hemogen.Value * 2f);
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

	public override void Notify_Equipped(Pawn pawn)
	{
		base.Notify_Equipped(pawn);
		if (Hemogen != null)
		{
			Hemogen.SetMax(Hemogen.Max + HemogenOffset);
			remember = HemogenOffset;
		}
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		base.Notify_Unequipped(pawn);
		if (pawn != null)
		{
			Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
			gene_Hemogen?.SetMax(gene_Hemogen.Max - remember);
		}
		remember = 0f;
	}

	private void Break()
	{
		float scale = Mathf.Lerp(base.Props.minDrawSize, base.Props.maxDrawSize, Hemogen.Value * 2f);
		EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
		FleckMaker.Static(base.PawnOwner.TrueCenter(), base.PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
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
	}

	public override void CompTick()
	{
		if (Hemogen == null)
		{
			return;
		}
		if (base.ShieldState == ShieldState.Active)
		{
			if (Hemogen.Value == 0f)
			{
				Break();
			}
		}
		else if (shouldactive && base.ShieldState == ShieldState.Resetting && (double)Hemogen.ValuePercent >= 0.5)
		{
			Reset();
		}
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
		if (base.ShieldState != 0 || Hemogen == null || dinfo.Def == DamageDefOf.SurgicalCut)
		{
			return;
		}
		if (dinfo.Def == DamageDefOf.Extinguish)
		{
			AbsorbedDamage(dinfo);
			absorbed = true;
			return;
		}
		float num = dinfo.Amount * base.PawnOwner.GetStatValue(StatDefOf.IncomingDamageFactor) / 100f;
		Hemogen.Value -= num;
		if (Hemogen.Value <= 0f)
		{
			Break();
		}
		else
		{
			AbsorbedDamage(dinfo);
		}
		absorbed = true;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref shouldactive, "shouldactive", defaultValue: false);
		Scribe_Values.Look(ref remember, "remember", 0f);
	}
}
