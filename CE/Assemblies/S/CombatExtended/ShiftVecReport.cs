using System;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ShiftVecReport
{
	public LocalTargetInfo target = null;

	public GlobalTargetInfo globalTarget = GlobalTargetInfo.Invalid;

	public float aimingAccuracy = 1f;

	public float sightsEfficiency = 1f;

	private float accuracyFactorInt = -1f;

	public float circularMissRadius = 0f;

	public float indirectFireShift = 0f;

	public float lightingShift = 0f;

	public float weatherShift = 0f;

	private float enviromentShiftInt = -1f;

	private float visibilityShiftInt = -1f;

	public float shotSpeed = 0f;

	private float leadDistInt = -1f;

	public float shotDist = 0f;

	public float maxRange;

	public bool isAiming = false;

	public float swayDegrees = 0f;

	public float spreadDegrees = 0f;

	public Thing cover = null;

	public float smokeDensity = 0f;

	public bool blindFiring = false;

	public bool roofed = false;

	public Pawn targetPawn => target.Thing as Pawn;

	public float accuracyFactor
	{
		get
		{
			if (accuracyFactorInt < 0f)
			{
				accuracyFactorInt = (1.5f - aimingAccuracy) / sightsEfficiency;
			}
			return accuracyFactorInt;
		}
	}

	public float enviromentShift
	{
		get
		{
			if (enviromentShiftInt < 0f)
			{
				enviromentShiftInt = ((blindFiring ? 1f : lightingShift) * 7f + weatherShift * 1.5f) * CE_Utility.LightingRangeMultiplier(shotDist) + smokeDensity;
			}
			return enviromentShiftInt;
		}
	}

	public float visibilityShift
	{
		get
		{
			if (visibilityShiftInt < 0f)
			{
				float num = sightsEfficiency;
				if (num < 0.02f)
				{
					num = 0.02f;
				}
				visibilityShiftInt = enviromentShift * (shotDist / 50f / num) * (2f - aimingAccuracy);
			}
			return visibilityShiftInt;
		}
	}

	private bool targetIsMoving => targetPawn != null && targetPawn.pather != null && targetPawn.pather.Moving && (targetPawn.stances.stunner == null || !targetPawn.stances.stunner.Stunned);

	public float leadDist
	{
		get
		{
			if (leadDistInt < 0f)
			{
				if (targetIsMoving)
				{
					float moveSpeed = CE_Utility.GetMoveSpeed(targetPawn);
					float num = shotDist / shotSpeed;
					leadDistInt = moveSpeed * num;
				}
				else
				{
					leadDistInt = 0f;
				}
			}
			return leadDistInt;
		}
	}

	public float leadShift => leadDist * Mathf.Min(accuracyFactor * 0.25f, 2.5f) + Mathf.Min((blindFiring ? 1f : lightingShift) * CE_Utility.LightingRangeMultiplier(shotDist) * leadDist * 0.25f, blindFiring ? 100f : 2f) + Mathf.Min((blindFiring ? 0f : smokeDensity) * 0.5f, 2f);

	public float distShift => shotDist * (shotDist / Math.Max(maxRange, 20f)) * Mathf.Min(accuracyFactor * 0.5f, 0.8f);

	public ShiftVecReport(ShiftVecReport report)
	{
		target = report.target;
		sightsEfficiency = report.sightsEfficiency;
		aimingAccuracy = report.aimingAccuracy;
		circularMissRadius = report.circularMissRadius;
		indirectFireShift = report.indirectFireShift;
		lightingShift = report.lightingShift;
		shotSpeed = report.shotSpeed;
		shotDist = report.shotDist;
		maxRange = report.maxRange;
		isAiming = report.isAiming;
		swayDegrees = report.swayDegrees;
		spreadDegrees = report.spreadDegrees;
		cover = report.cover;
		smokeDensity = report.smokeDensity;
		blindFiring = report.blindFiring;
		roofed = report.roofed;
	}

	public ShiftVecReport()
	{
	}

	public Vector2 GetRandCircularVec()
	{
		return CE_Utility.GenRandInCircle(visibilityShift + circularMissRadius + indirectFireShift);
	}

	public float GetRandDist()
	{
		return shotDist + Rand.Range(0f - distShift, distShift);
	}

	public Vector2 GetRandLeadVec()
	{
		if (blindFiring)
		{
			return new Vector2(0f, 0f);
		}
		Vector3 vector = default(Vector3);
		if (targetIsMoving)
		{
			vector = (targetPawn.pather.nextCell - targetPawn.Position).ToVector3() * (leadDist + Rand.Range(0f - leadShift, leadShift));
		}
		return new Vector2(vector.x, vector.z);
	}

	public Vector2 GetRandSpreadVec()
	{
		return Rand.InsideUnitCircle * spreadDegrees;
	}

	public static string AsPercent(float pct)
	{
		return Mathf.RoundToInt(100f * pct) + "%";
	}

	public string GetTextReadout()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("   " + "CE_VisibilityError".Translate() + "\t" + visibilityShift.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_cells".Translate());
		if (Controller.settings.DebuggingMode)
		{
			stringBuilder.AppendLine("   " + $"DEBUG: visibilityShift\t\t{visibilityShift} ");
			stringBuilder.AppendLine("   " + $"DEBUG: leadDist\t\t{leadDist} ");
			stringBuilder.AppendLine("   " + $"DEBUG: enviromentShift\t{enviromentShift}");
			stringBuilder.AppendLine("   " + $"DEBUG: accuracyFactor\t{accuracyFactor}");
			stringBuilder.AppendLine("   " + $"DEBUG: circularMissRadius\t{circularMissRadius}");
			stringBuilder.AppendLine("   " + $"DEBUG: sightsEfficiency\t{sightsEfficiency}");
			stringBuilder.AppendLine("   " + $"DEBUG: weathershift\t\t{weatherShift}");
			stringBuilder.AppendLine("   " + $"DEBUG: accuracyFactor\t\t{accuracyFactor}");
			stringBuilder.AppendLine("   " + $"DEBUG: lightingShift\t\t{lightingShift}");
		}
		if (lightingShift > 0f)
		{
			stringBuilder.AppendLine("      " + "Darkness".Translate() + "\t" + AsPercent(lightingShift));
		}
		if (weatherShift > 0f)
		{
			stringBuilder.AppendLine("      " + "Weather".Translate() + "\t" + AsPercent(weatherShift));
		}
		if (smokeDensity > 0f)
		{
			stringBuilder.AppendLine("      " + "CE_SmokeDensity".Translate() + "\t" + AsPercent(smokeDensity));
		}
		if (leadShift > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_LeadError".Translate() + "\t" + leadShift.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_cells".Translate());
		}
		if (distShift > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_RangeError".Translate() + "\t" + distShift.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_cells".Translate());
		}
		if (swayDegrees > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_Sway".Translate() + "\t\t" + swayDegrees.ToStringByStyle(ToStringStyle.FloatTwo) + "CE_degrees".Translate());
		}
		if (spreadDegrees > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_Spread".Translate() + "\t\t" + spreadDegrees.ToStringByStyle(ToStringStyle.FloatTwo) + "CE_degrees".Translate());
		}
		if (circularMissRadius > 0f)
		{
			stringBuilder.AppendLine("   " + "CE_MissRadius".Translate() + "\t" + circularMissRadius.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_cells".Translate());
			if (indirectFireShift > 0f)
			{
				stringBuilder.AppendLine("   " + "CE_IndirectFire".Translate() + "\t" + indirectFireShift.ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_cells".Translate());
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_MortarDirectFire, KnowledgeAmount.FrameDisplayed);
			}
		}
		else
		{
			if (cover != null)
			{
				stringBuilder.AppendLine(string.Concat("   " + "CE_CoverHeight".Translate() + "\t", (new CollisionVertical(cover).Max * 1.75f).ToString(), " ") + "CE_meters".Translate());
			}
			if (target.Thing != null)
			{
				stringBuilder.AppendLine("   " + "CE_TargetHeight".Translate() + "\t" + (new CollisionVertical(target.Thing).HeightRange.Span * 1.75f).ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_meters".Translate());
				stringBuilder.AppendLine("   " + "CE_TargetWidth".Translate() + "\t" + (CE_Utility.GetCollisionWidth(target.Thing) * 1.75f).ToStringByStyle(ToStringStyle.FloatTwo) + " " + "CE_meters".Translate());
				if (target.Thing is Pawn pawn && pawn.IsCrouching())
				{
					LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Crouching, OpportunityType.GoodToKnow);
				}
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_AimingSystem, KnowledgeAmount.FrameDisplayed);
		}
		return stringBuilder.ToString();
	}
}
