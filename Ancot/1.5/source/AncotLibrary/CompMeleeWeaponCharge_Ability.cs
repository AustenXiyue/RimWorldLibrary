using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompMeleeWeaponCharge_Ability : ThingComp
{
	public int charge;

	public int maxCharge
	{
		get
		{
			if (!Props.maxChargeCanMultiply)
			{
				return (int)parent.GetStatValue(AncotDefOf.Ancot_WeaponMaxCharge);
			}
			return (int)(maxChargeFactor * parent.GetStatValue(AncotDefOf.Ancot_WeaponMaxCharge));
		}
	}

	public int ticksPerCharge
	{
		get
		{
			if (!Props.maxChargeSpeedCanMultiply)
			{
				return (int)parent.GetStatValue(AncotDefOf.Ancot_WeaponChargeTick);
			}
			return (int)(ticksPerChargeFactor * parent.GetStatValue(AncotDefOf.Ancot_WeaponChargeTick));
		}
	}

	public float maxChargeFactor
	{
		get
		{
			if (CasterPawn == null)
			{
				return 1f;
			}
			return CasterPawn.GetStatValue(AncotDefOf.Ancot_WeaponMaxChargeFactor);
		}
	}

	public float ticksPerChargeFactor
	{
		get
		{
			if (CasterPawn == null)
			{
				return 1f;
			}
			return CasterPawn.GetStatValue(AncotDefOf.Ancot_WeaponChargeTickFactor);
		}
	}

	public bool CanBeUsed => charge >= Props.chargePerUse;

	public CompEquippable compEquippable => parent.TryGetComp<CompEquippable>();

	public Pawn CasterPawn => compEquippable.PrimaryVerb.caster as Pawn;

	public Color barColor => Props.barColor;

	private CompProperties_MeleeWeaponCharge_Ability Props => (CompProperties_MeleeWeaponCharge_Ability)props;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref charge, "charge", 0);
	}

	public override void Initialize(CompProperties props)
	{
		base.props = props;
		charge = maxCharge;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (parent == null)
		{
			charge = 0;
		}
		if (charge < maxCharge)
		{
			if (Props.autoRecharge && parent.IsHashIntervalTick(ticksPerCharge))
			{
				charge++;
			}
			if (charge > maxCharge)
			{
				charge = maxCharge;
			}
		}
	}

	public void UsedOnce()
	{
		if (charge > 0)
		{
			charge -= Props.chargePerUse;
		}
		if (Props.destroyOnEmpty && charge == 0 && !parent.Destroyed)
		{
			parent.Destroy();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (Find.Selector.SelectedObjects.Count == 1)
		{
			yield return new Gizmo_ChargeBar_MeleeWeapon
			{
				compWeaponCharge = parent.TryGetComp<CompMeleeWeaponCharge_Ability>()
			};
		}
	}
}
