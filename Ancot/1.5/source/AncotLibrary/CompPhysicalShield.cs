using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
public class CompPhysicalShield : ThingComp
{
	public bool holdShield = false;

	public float stamina;

	private static readonly SimpleCurve ArmorRatingToStaminaConsumeFactor = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(0.5f, 1.4f),
		new CurvePoint(2f, 0.5f),
		new CurvePoint(999f, 0.5f)
	};

	protected int ticksToReset = -1;

	public Vector3 impactAngleVect = new Vector3(0f, 0f, 0f);

	public An_ShieldState ShieldStateNote;

	private QualityCategory Quality
	{
		get
		{
			if (parent is Apparel thing)
			{
				return thing.TryGetComp<CompQuality>().Quality;
			}
			return QualityCategory.Normal;
		}
	}

	private float maxStaminaFactor
	{
		get
		{
			if (parent is Apparel apparel && apparel.def.MadeFromStuff)
			{
				return Mathf.Min(5f, apparel.Stuff.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints));
			}
			return 1f;
		}
	}

	private float staminaConsumeFactor
	{
		get
		{
			if (parent is Apparel thing)
			{
				return ArmorRatingToStaminaConsumeFactor.Evaluate(thing.GetStatValue(StatDefOf.ArmorRating_Sharp));
			}
			return 1f;
		}
	}

	public float maxStamina => Props.maxStamina * AncotUtility.QualityFactor(Quality) * maxStaminaFactor;

	public string barGizmoLabel => Props.barGizmoLabel;

	public Color shieldBarColor => Props.shieldBarColor;

	public string graphicPath_Holding => Props.graphicPath_Holding;

	public string graphicPath_Ready => Props.graphicPath_Ready;

	public string graphicPath_Disabled => Props.graphicPath_Disabled;

	public EffecterDef blockEffecter
	{
		get
		{
			if (Props.blockEffecter != null)
			{
				return Props.blockEffecter;
			}
			return AncotDefOf.Ancot_ShieldBlock;
		}
	}

	public EffecterDef breakEffecter
	{
		get
		{
			if (Props.breakEffecter != null)
			{
				return Props.breakEffecter;
			}
			return AncotDefOf.Ancot_ShieldBreak;
		}
	}

	public CompProperties_PhysicalShield Props => (CompProperties_PhysicalShield)props;

	protected Pawn PawnOwner
	{
		get
		{
			if (!(parent is Apparel { Wearer: var wearer }))
			{
				if (parent is Pawn result)
				{
					return result;
				}
				return null;
			}
			return wearer;
		}
	}

	private float staminaRecoveryRateFactor
	{
		get
		{
			if (PawnOwner != null)
			{
				return PawnOwner.GetStatValue(AncotDefOf.Ancot_StaminaRecoveryRateFactor);
			}
			return 1f;
		}
	}

	public float StaminaRecoverPerTickEffectvie => Props.staminaGainPerTick * staminaRecoveryRateFactor;

	private float ThresholdStaminaCostPct => Props.thresholdStaminaCostPct * maxStamina;

	public bool IsApparel => parent is Apparel;

	private bool IsBuiltIn => !IsApparel;

	private CompMechAutoFight compMechAutoFight => PawnOwner.TryGetComp<CompMechAutoFight>();

	public bool autoFightForPlayer
	{
		get
		{
			if (compMechAutoFight != null && PawnOwner != null && PawnOwner.Faction.IsPlayer)
			{
				return compMechAutoFight.autoFight;
			}
			return false;
		}
	}

	public An_ShieldState ShieldState
	{
		get
		{
			if (parent is Pawn p && (p.IsCharging() || p.IsSelfShutdown()) && !PawnOwner.Spawned)
			{
				return An_ShieldState.Disabled;
			}
			CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
			if ((comp != null && !comp.Awake) || (PawnOwner != null && PawnOwner.Faction.IsPlayer && !PawnOwner.Drafted && !autoFightForPlayer))
			{
				return An_ShieldState.Disabled;
			}
			if (ticksToReset <= 0)
			{
				if (holdShield && stamina > 0f)
				{
					return An_ShieldState.Active;
				}
				return An_ShieldState.Ready;
			}
			return An_ShieldState.Resetting;
		}
	}

	public bool CanUseShield
	{
		get
		{
			Pawn pawnOwner = PawnOwner;
			if (!pawnOwner.Spawned || pawnOwner.Dead || pawnOwner.Downed)
			{
				return false;
			}
			if (pawnOwner.InAggroMentalState)
			{
				return true;
			}
			if (pawnOwner.Drafted)
			{
				return true;
			}
			if (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner)
			{
				return true;
			}
			if (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && (pawnOwner.Drafted || autoFightForPlayer) && Find.Selector.SingleSelectedThing == pawnOwner)
			{
				return true;
			}
			return false;
		}
	}

	public override void Initialize(CompProperties props)
	{
		base.props = props;
		stamina = maxStamina;
		if (PawnOwner != null && PawnOwner.Faction != Faction.OfPlayer && Props.alwaysHoldShield)
		{
			holdShield = true;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref stamina, "stamina", 0f);
		Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
		Scribe_Values.Look(ref holdShield, "holdShield", defaultValue: false);
	}

	public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetWornGizmosExtra())
		{
			yield return item;
		}
		if (!IsApparel)
		{
			yield break;
		}
		foreach (Gizmo gizmo in GetGizmos())
		{
			yield return gizmo;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (!IsBuiltIn)
		{
			yield break;
		}
		foreach (Gizmo gizmo in GetGizmos())
		{
			yield return gizmo;
		}
	}

	private IEnumerable<Gizmo> GetGizmos()
	{
		if (PawnOwner.Faction == Faction.OfPlayer && PawnOwner.Drafted)
		{
			yield return new Command_Toggle
			{
				Order = Props.gizmoOrder,
				defaultLabel = Props.gizmoLabel.Translate(),
				defaultDesc = Props.gizmoDesc.Translate(),
				icon = ContentFinder<Texture2D>.Get(Props.gizmoIconPath),
				toggleAction = delegate
				{
					holdShield = !holdShield;
				},
				isActive = () => holdShield
			};
		}
		if (Find.Selector.SingleSelectedThing == PawnOwner && (PawnOwner.Drafted || autoFightForPlayer))
		{
			yield return new Gizmo_PhysicalShieldBar
			{
				compPhysicalShield = parent.TryGetComp<CompPhysicalShield>()
			};
		}
	}

	public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
		if (ShieldState == An_ShieldState.Active && PawnOwner != null)
		{
			float asAngle = PawnOwner.Rotation.AsAngle;
			float num = dinfo.Angle + 180f;
			if (num > 360f)
			{
				num -= 360f;
			}
			if (num >= asAngle - Props.defenseAngle / 2f && num <= asAngle + Props.defenseAngle / 2f)
			{
				BlockByShield(ref dinfo, out var blocked);
				absorbed = blocked;
			}
			if (asAngle - Props.defenseAngle / 2f < 0f && num > 360f + asAngle - Props.defenseAngle / 2f && num < 360f)
			{
				BlockByShield(ref dinfo, out var blocked2);
				absorbed = blocked2;
			}
			if (asAngle + Props.defenseAngle / 2f > 360f && num < asAngle + Props.defenseAngle / 2f - 360f && num > 0f)
			{
				BlockByShield(ref dinfo, out var blocked3);
				absorbed = blocked3;
			}
		}
	}

	private void BlockByShield(ref DamageInfo dinfo, out bool blocked)
	{
		if (Props.recordLastHarmTickWhenBlocked)
		{
			PawnOwner.mindState.lastHarmTick = Find.TickManager.TicksGame;
			if (dinfo.Def.isRanged)
			{
				PawnOwner.mindState.lastRangedHarmTick = Find.TickManager.TicksGame;
			}
		}
		float amount = dinfo.Amount;
		if (!dinfo.Def.harmsHealth || dinfo.IgnoreArmor)
		{
			blocked = false;
			return;
		}
		if (dinfo.Def.isRanged)
		{
			float num = amount * dinfo.ArmorPenetrationInt * Props.staminaConsumeRateRange * staminaConsumeFactor;
			blocked = true;
			if (num > ThresholdStaminaCostPct)
			{
				num = ThresholdStaminaCostPct;
			}
			stamina -= num;
		}
		else
		{
			float num2 = amount * dinfo.ArmorPenetrationInt * Props.staminaConsumeRateMelee * staminaConsumeFactor;
			blocked = true;
			if (num2 > ThresholdStaminaCostPct)
			{
				num2 = ThresholdStaminaCostPct;
			}
			stamina -= num2;
		}
		if (stamina < 0f)
		{
			Break();
			breakEffecter.Spawn().Trigger(PawnOwner, dinfo.Instigator ?? PawnOwner);
			MoteMaker.ThrowText(PawnOwner.DrawPos, PawnOwner.Map, "Ancot.TextMote_Break".Translate(), Color.red, 1.9f);
		}
		else
		{
			AbsorbedDamage(dinfo);
		}
		if (IsApparel)
		{
			parent.HitPoints -= (int)Mathf.Min(amount / 12f, 10f);
			if (parent.HitPoints <= 0)
			{
				parent.Destroy();
			}
		}
	}

	private void AbsorbedDamage(DamageInfo dinfo)
	{
		impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
		blockEffecter.Spawn().Trigger(PawnOwner, dinfo.Instigator ?? PawnOwner);
		MoteMaker.ThrowText(PawnOwner.DrawPos, PawnOwner.Map, "Ancot.TextMote_Block".Translate(), 1.9f);
	}

	private void ShieldPiercing(DamageInfo dinfo)
	{
		breakEffecter.Spawn().Trigger(PawnOwner, dinfo.Instigator ?? PawnOwner);
		MoteMaker.ThrowText(PawnOwner.DrawPos, PawnOwner.Map, "Ancot.TextMote_Penetrate".Translate(), Color.red, 1.9f);
		DamageInfo dinfo2 = dinfo;
		dinfo2.SetAmount(dinfo2.Amount / 4f);
		dinfo2.SetIgnoreArmor(ignoreArmor: true);
		PawnOwner.TakeDamage(dinfo2);
		Log.Message("dinfo" + dinfo2.Amount);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (impactAngleVect != new Vector3(0f, 0f, 0f))
		{
			impactAngleVect *= 0.8f;
		}
		if (PawnOwner == null)
		{
			stamina = 0f;
		}
		else if (ShieldState == An_ShieldState.Resetting || (ShieldState == An_ShieldState.Disabled && stamina == 0f))
		{
			ticksToReset--;
			if (ticksToReset <= 0)
			{
				Reset();
			}
			RemoveShieldRaiseHediff();
		}
		else if (ShieldState == An_ShieldState.Active || ShieldState == An_ShieldState.Ready || ShieldState == An_ShieldState.Disabled)
		{
			if (holdShield && ShieldState == An_ShieldState.Active)
			{
				stamina += StaminaRecoverPerTickEffectvie * 0.4f;
				AddShieldRaiseHediff();
			}
			else
			{
				stamina += StaminaRecoverPerTickEffectvie;
				RemoveShieldRaiseHediff();
			}
			if (stamina > maxStamina)
			{
				stamina = maxStamina;
			}
		}
		else
		{
			RemoveShieldRaiseHediff();
		}
		if (ShieldStateNote != ShieldState)
		{
			ShieldStateNote = ShieldState;
			Notify_StateChange();
			PawnOwner?.Drawer?.renderer?.renderTree?.SetDirty();
		}
	}

	private void Break()
	{
		stamina = 0f;
		ticksToReset = Props.startingTicksToReset;
		PawnOwner.stances.SetStance(new Stance_Cooldown(Props.shieldBreakStanceTick, null, null));
	}

	public void Notify_StateChange()
	{
		if (PawnOwner == null)
		{
			return;
		}
		foreach (Hediff hediff in PawnOwner.health.hediffSet.hediffs)
		{
			hediff.TryGetComp<HediffComp_PhysicalShieldState>()?.Notify_ShieldStateChange(PawnOwner, ShieldState);
		}
	}

	public void AddShieldRaiseHediff()
	{
		if (PawnOwner != null && Props.holdShieldHediff != null)
		{
			HealthUtility.AdjustSeverity(PawnOwner, Props.holdShieldHediff, 1f);
		}
	}

	public void RemoveShieldRaiseHediff()
	{
		if (PawnOwner != null && Props.holdShieldHediff != null)
		{
			Hediff firstHediffOfDef = PawnOwner.health.hediffSet.GetFirstHediffOfDef(Props.holdShieldHediff);
			if (firstHediffOfDef != null && PawnOwner != null)
			{
				PawnOwner.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}

	private void Reset()
	{
		if (PawnOwner.Spawned)
		{
			FleckMaker.ThrowLightningGlow(PawnOwner.TrueCenter(), PawnOwner.Map, 3f);
		}
		ticksToReset = -1;
		stamina = maxStamina;
	}

	public override bool CompAllowVerbCast(Verb verb)
	{
		if (Props.blocksRangedWeapons)
		{
			return !(verb is Verb_LaunchProjectile);
		}
		return true;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.MaxStamina".Translate(), maxStamina.ToString(), "Ancot.MaxStaminaDesc".Translate(), 1000);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.StaminaGainPerTick".Translate(), (StaminaRecoverPerTickEffectvie * 60f).ToString() + "Ancot.PerSecond".Translate(), "Ancot.StaminaGainPerTickDesc".Translate(), 990);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.DefenseAngle".Translate(), Props.defenseAngle + "°", "Ancot.DefenseAngleDesc".Translate(), 980);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.BlocksRangedWeapons".Translate(), Props.blocksRangedWeapons ? "Ancot.True".Translate() : "Ancot.False".Translate(), "Ancot.BlocksRangedWeaponsDesc".Translate(), 960);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.ShieldBreakStanceTick".Translate(), (Props.shieldBreakStanceTick / 60).ToString() + "Ancot.Second".Translate(), "Ancot.ShieldBreakStanceTickDesc".Translate(), 950);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.StaminaConsumeRateRange".Translate(), (staminaConsumeFactor * Props.staminaConsumeRateRange).ToString("F2"), "Ancot.StaminaConsumeRateRangeDesc".Translate(), 940);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.StaminaConsumeRateMelee".Translate(), (staminaConsumeFactor * Props.staminaConsumeRateMelee).ToString("F2"), "Ancot.StaminaConsumeRateMeleeDesc".Translate(), 930);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.ThresholdStaminaCost".Translate(), ThresholdStaminaCostPct.ToString(), "Ancot.ThresholdStaminaCostDesc".Translate(), 920);
		yield return new StatDrawEntry(AncotDefOf.Ancot_Shield, "Ancot.StartingTicksToReset".Translate(), (Props.startingTicksToReset / 60).ToString() + "Ancot.Second".Translate(), "Ancot.StartingTicksToResetDesc".Translate(), 910);
	}
}
