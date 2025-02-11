using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class HediffComp_Stabilize : HediffComp
{
	private const float bleedIncreasePerSec = 0.01f;

	private const float internalBleedOffset = 0.2f;

	private static readonly Texture2D StabilizedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Stabilized_Icon");

	private bool stabilized = false;

	private float bleedModifier = 1f;

	public HediffCompProperties_Stabilize Props => props as HediffCompProperties_Stabilize;

	public bool Stabilized => stabilized;

	public float BleedModifier
	{
		get
		{
			float num = bleedModifier;
			if (parent is Hediff_MissingPart)
			{
				num *= 0.5f;
			}
			if (parent.Part.depth == BodyPartDepth.Inside)
			{
				num += 0.2f;
			}
			return Mathf.Clamp01(num);
		}
	}

	public float StabilizedBleed
	{
		get
		{
			float num = parent.Severity * parent.def.injuryProps.bleedRate;
			if (parent.Part != null)
			{
				num *= parent.Part.def.bleedRate;
			}
			return num * (1f - BleedModifier);
		}
	}

	public override TextureAndColor CompStateIcon
	{
		get
		{
			if (bleedModifier < 1f && !parent.IsPermanent() && !parent.IsTended())
			{
				Color color = Color.Lerp(HediffComp_TendDuration.UntendedColor, Color.white, Mathf.Clamp01(1f - bleedModifier));
				return new TextureAndColor(StabilizedIcon, color);
			}
			return TextureAndColor.None;
		}
	}

	public override string CompDescriptionExtra
	{
		get
		{
			if (bleedModifier < 1f && !parent.IsPermanent() && !parent.IsTended())
			{
				return "\n\n" + "CE_StabilizeHediffDescription".Translate(Mathf.Max(bleedModifier, 0f).ToStringPercent("0.#"), StabilizationHoursLeft().ToString("0.00"));
			}
			return null;
		}
	}

	public void Stabilize(Pawn medic, Medicine medicine)
	{
		if (stabilized)
		{
			Log.Error("CE tried to stabilize an injury that is already stabilized before");
			return;
		}
		if (medicine == null)
		{
			Log.Error("CE tried to stabilize without medicine");
			return;
		}
		float num = 2f * medic.GetStatValue(StatDefOf.MedicalTendQuality) * medicine.GetStatValue(StatDefOf.MedicalPotency);
		bleedModifier = 1f - num;
		stabilized = true;
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref stabilized, "stabilized", defaultValue: false);
		Scribe_Values.Look(ref bleedModifier, "bleedModifier", 1f);
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (stabilized && bleedModifier < 1f && parent.ageTicks % 60 == 0)
		{
			bleedModifier += 0.01f;
			if (bleedModifier >= 1f)
			{
				bleedModifier = 1f;
			}
		}
		else if (!stabilized && parent.pawn.Downed)
		{
			LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Stabilizing, parent.pawn, OpportunityType.Important);
		}
	}

	public double StabilizationHoursLeft()
	{
		return (double)(1f - bleedModifier) / 0.01 * 60.0 / 2500.0;
	}

	public override string CompDebugString()
	{
		if (parent.BleedRate < 0f)
		{
			return "Not bleeding";
		}
		if (!stabilized)
		{
			return "Not stabilized";
		}
		return "Stabilized" + ((parent.Part.depth == BodyPartDepth.Inside) ? " internal bleeding" : "") + "\nbleed rate modifier: " + bleedModifier;
	}
}
