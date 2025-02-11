using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AncotLibrary;

public class CompWeaponCharge : ThingComp
{
	public int charge;

	private bool emptyOnce = true;

	public int ticksToReset = -1;

	public int maxCharge
	{
		get
		{
			if (!Props.maxChargeCanMultiply)
			{
				return Mathf.RoundToInt(parent.GetStatValue(AncotDefOf.Ancot_WeaponMaxCharge));
			}
			return Mathf.RoundToInt(maxChargeFactor * parent.GetStatValue(AncotDefOf.Ancot_WeaponMaxCharge));
		}
	}

	public int ticksPerCharge
	{
		get
		{
			if (!Props.maxChargeSpeedCanMultiply)
			{
				return Mathf.RoundToInt(parent.GetStatValue(AncotDefOf.Ancot_WeaponChargeTick));
			}
			return Mathf.RoundToInt(ticksPerChargeFactor * parent.GetStatValue(AncotDefOf.Ancot_WeaponChargeTick));
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

	public EffecterDef chargeFireEffecter => Props.chargeFireEffecter;

	public int StartingTicksToReset => Props.resetTicks;

	public CompRangeWeaponVerbSwitch compVerbSwitch => parent.TryGetComp<CompRangeWeaponVerbSwitch>();

	public ThingDef projectileCharged
	{
		get
		{
			if (compVerbSwitch != null && compVerbSwitch.flag && Props.projectileCharged_Switched != null)
			{
				return Props.projectileCharged_Switched;
			}
			return Props.projectileCharged;
		}
	}

	public CompEquippable compEquippable => parent.TryGetComp<CompEquippable>();

	public Pawn CasterPawn => compEquippable.PrimaryVerb.caster as Pawn;

	public Color barColor => Props.barColor;

	private CompProperties_WeaponCharge Props => (CompProperties_WeaponCharge)props;

	public An_ChargeState ChargeState
	{
		get
		{
			if (ticksToReset > 0)
			{
				return An_ChargeState.Resetting;
			}
			if (charge == 0)
			{
				return An_ChargeState.Empty;
			}
			return An_ChargeState.Active;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref charge, "charge", 0);
		Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
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
		else if (ChargeState == An_ChargeState.Resetting)
		{
			ticksToReset--;
			if (ticksToReset <= 0)
			{
				Reset();
			}
		}
		else if (ChargeState == An_ChargeState.Active)
		{
			if (Props.autoRecharge && parent.IsHashIntervalTick(ticksPerCharge))
			{
				charge++;
			}
			if (charge > maxCharge)
			{
				charge = maxCharge;
			}
			if (!emptyOnce)
			{
				emptyOnce = true;
			}
		}
		if (ChargeState == An_ChargeState.Empty && emptyOnce)
		{
			emptyOnce = false;
			Empty();
		}
	}

	public void Empty()
	{
		charge = 0;
		ticksToReset = StartingTicksToReset;
	}

	public void Reset()
	{
		if (parent.Spawned)
		{
			SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
		}
		ticksToReset = -1;
		charge = (int)(Props.chargeOnResetRatio * (float)maxCharge);
	}

	public void UsedOnce()
	{
		if (charge > 0)
		{
			charge--;
		}
		if (Props.destroyOnEmpty && charge == 0 && !parent.Destroyed)
		{
			parent.Destroy();
		}
	}

	public void ChargeFireEffect(TargetInfo A, TargetInfo B)
	{
		if (Props.chargeFireEffecter != null)
		{
			Props.chargeFireEffecter.Spawn().Trigger(A, B);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (Find.Selector.SelectedObjects.Count == 1 && (CasterPawn == null || CasterPawn.Faction.IsPlayer))
		{
			yield return new Gizmo_ChargeBar
			{
				compWeaponCharge = this
			};
		}
	}

	public string ProjectileChargedInfo()
	{
		string text = string.Concat(string.Concat("Ancot.ProjectileChargedDesc".Translate() + "\n\n" + "Ancot.ProjectileCharged_Damage".Translate() + ": ", projectileCharged.projectile.GetDamageAmount(parent).ToString(), "\n") + "Ancot.ProjectileCharged_ArmorPenetration".Translate() + ": " + projectileCharged.projectile.GetArmorPenetration(parent).ToStringPercent() + "\n" + "Ancot.ProjectileCharged_StoppingPower".Translate() + ": ", projectileCharged.projectile.stoppingPower.ToString());
		if ((double)projectileCharged.projectile.explosionRadius > 0.1)
		{
			text = string.Concat(text, "\n" + "Ancot.ProjectileCharged_ExplosionRadius".Translate() + ": ", projectileCharged.projectile.explosionRadius.ToString());
		}
		return text;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (projectileCharged != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, "Ancot.ProjectileCharged".Translate(), projectileCharged.LabelCap, ProjectileChargedInfo(), 5600);
		}
	}
}
