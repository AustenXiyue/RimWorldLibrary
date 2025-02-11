using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AncotLibrary;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Milira;

[StaticConstructorOnStartup]
public class Building_TurretGunFortress : Building_Turret
{
	protected int burstCooldownTicksLeft;

	protected int burstWarmupTicksLeft;

	protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

	private bool holdFire;

	private bool burstActivated;

	public Thing gun;

	protected TurretTop top;

	protected CompPowerTrader powerComp;

	protected CompCanBeDormant dormantComp;

	protected CompInitiatable initiatableComp;

	protected CompMannable mannableComp;

	protected CompInteractable interactableComp;

	public CompRefuelable refuelableComp;

	protected Effecter progressBarEffecter;

	protected CompMechPowerCell powerCellComp;

	private const int TryStartShootSomethingIntervalTicks = 10;

	private static readonly Material Turret = MaterialPool.MatFrom("Milira/Building/Security/MilianHeavyTurretPlasma_TopII", ShaderDatabase.Cutout, Color.white);

	public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

	private static readonly SimpleCurve ArmorToDamageReductionCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(2f, 0.6f)
	};

	protected CompThingContainer_Milian compThingContainer => this.TryGetComp<CompThingContainer_Milian>();

	public float damageReductionFactor
	{
		get
		{
			if (compThingContainer != null && compThingContainer.innerPawn != null)
			{
				return ArmorToDamageReductionCurve.Evaluate(TryGetOverallArmor(compThingContainer.innerPawn, StatDefOf.ArmorRating_Sharp));
			}
			return 1f;
		}
	}

	public bool Active
	{
		get
		{
			if ((powerComp == null || powerComp.PowerOn) && (dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated) && (interactableComp == null || burstActivated))
			{
				if (powerCellComp != null)
				{
					return !powerCellComp.depleted;
				}
				return true;
			}
			return false;
		}
	}

	public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

	public override LocalTargetInfo CurrentTarget => currentTargetInt;

	private bool WarmingUp => burstWarmupTicksLeft > 0;

	public override Verb AttackVerb => GunCompEq.PrimaryVerb;

	public bool IsMannable => mannableComp != null;

	private bool PlayerControlled
	{
		get
		{
			if ((base.Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist)
			{
				return !IsActivable;
			}
			return false;
		}
	}

	protected virtual bool CanSetForcedTarget => PlayerControlled;

	private bool CanToggleHoldFire => PlayerControlled;

	private bool IsMortar => def.building.IsMortar;

	private bool IsMortarOrProjectileFliesOverhead
	{
		get
		{
			if (!AttackVerb.ProjectileFliesOverhead())
			{
				return IsMortar;
			}
			return true;
		}
	}

	private bool IsActivable => interactableComp != null;

	protected virtual bool HideForceTargetGizmo => false;

	public TurretTop Top => top;

	private bool CanExtractShell
	{
		get
		{
			if (!PlayerControlled)
			{
				return false;
			}
			return gun.TryGetComp<CompChangeableProjectile>()?.Loaded ?? false;
		}
	}

	private bool MannedByColonist
	{
		get
		{
			if (mannableComp != null && mannableComp.ManningPawn != null)
			{
				return mannableComp.ManningPawn.Faction == Faction.OfPlayer;
			}
			return false;
		}
	}

	private bool MannedByNonColonist
	{
		get
		{
			if (mannableComp != null && mannableComp.ManningPawn != null)
			{
				return mannableComp.ManningPawn.Faction != Faction.OfPlayer;
			}
			return false;
		}
	}

	public Building_TurretGunFortress()
	{
		top = new TurretTop(this);
	}

	public override void PostMake()
	{
		base.PostMake();
		burstCooldownTicksLeft = def.building.turretInitialCooldownTime.SecondsToTicks();
		MakeGun();
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		dormantComp = GetComp<CompCanBeDormant>();
		initiatableComp = GetComp<CompInitiatable>();
		powerComp = GetComp<CompPowerTrader>();
		mannableComp = GetComp<CompMannable>();
		interactableComp = GetComp<CompInteractable>();
		refuelableComp = GetComp<CompRefuelable>();
		powerCellComp = GetComp<CompMechPowerCell>();
		if (!respawningAfterLoad)
		{
			top.SetRotationFromOrientation();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		ResetCurrentTarget();
		progressBarEffecter?.Cleanup();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
		Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
		Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
		Scribe_Values.Look(ref holdFire, "holdFire", defaultValue: false);
		Scribe_Values.Look(ref burstActivated, "burstActivated", defaultValue: false);
		Scribe_Deep.Look(ref gun, "gun");
		BackCompatibility.PostExposeData(this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (gun == null)
			{
				Log.Error("Turret had null gun after loading. Recreating.");
				MakeGun();
			}
			else
			{
				UpdateGunVerbs();
			}
		}
	}

	public override bool ClaimableBy(Faction by, StringBuilder reason = null)
	{
		if (!base.ClaimableBy(by, reason))
		{
			return false;
		}
		if (mannableComp != null && mannableComp.ManningPawn != null)
		{
			return false;
		}
		if (Active && mannableComp == null)
		{
			return false;
		}
		if (((dormantComp != null && !dormantComp.Awake) || (initiatableComp != null && !initiatableComp.Initiated)) && (powerComp == null || powerComp.PowerOn))
		{
			return false;
		}
		return true;
	}

	public override void OrderAttack(LocalTargetInfo targ)
	{
		if (!targ.IsValid)
		{
			if (forcedTarget.IsValid)
			{
				ResetForcedTarget();
			}
			return;
		}
		if ((targ.Cell - base.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
		{
			Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if ((targ.Cell - base.Position).LengthHorizontal > AttackVerb.verbProps.range)
		{
			Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (forcedTarget != targ)
		{
			forcedTarget = targ;
			if (burstCooldownTicksLeft <= 0)
			{
				TryStartShootSomething(canBeginBurstImmediately: false);
			}
		}
		if (holdFire)
		{
			Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, historical: false);
		}
	}

	public override void Tick()
	{
		base.Tick();
		if (CanExtractShell && MannedByColonist)
		{
			CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
			if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
			{
				ExtractShell();
			}
		}
		if (forcedTarget.IsValid && !CanSetForcedTarget)
		{
			ResetForcedTarget();
		}
		if (!CanToggleHoldFire)
		{
			holdFire = false;
		}
		if (forcedTarget.ThingDestroyed)
		{
			ResetForcedTarget();
		}
		if (Active && (mannableComp == null || mannableComp.MannedNow) && !base.IsStunned && base.Spawned)
		{
			GunCompEq.verbTracker.VerbsTick();
			if (AttackVerb.state == VerbState.Bursting)
			{
				return;
			}
			burstActivated = false;
			if (WarmingUp)
			{
				burstWarmupTicksLeft--;
				if (burstWarmupTicksLeft == 0)
				{
					BeginBurst();
				}
			}
			else
			{
				if (burstCooldownTicksLeft > 0)
				{
					burstCooldownTicksLeft--;
					if (IsMortar)
					{
						if (progressBarEffecter == null)
						{
							progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
						}
						progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
						MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
						mote.progress = 1f - (float)Math.Max(burstCooldownTicksLeft, 0) / (float)BurstCooldownTime().SecondsToTicks();
						mote.offsetZ = -0.8f;
					}
				}
				if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
				{
					TryStartShootSomething(canBeginBurstImmediately: true);
				}
			}
			top.TurretTopTick();
		}
		else
		{
			ResetCurrentTarget();
		}
		if (this.IsHashIntervalTick(60))
		{
			Modification_AutoRepair();
		}
	}

	public void TryActivateBurst()
	{
		burstActivated = true;
		TryStartShootSomething(canBeginBurstImmediately: true);
	}

	public void TryStartShootSomething(bool canBeginBurstImmediately)
	{
		if (progressBarEffecter != null)
		{
			progressBarEffecter.Cleanup();
			progressBarEffecter = null;
		}
		if (!base.Spawned || (holdFire && CanToggleHoldFire) || (AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !AttackVerb.Available())
		{
			ResetCurrentTarget();
			return;
		}
		bool isValid = currentTargetInt.IsValid;
		if (forcedTarget.IsValid)
		{
			currentTargetInt = forcedTarget;
		}
		else
		{
			currentTargetInt = TryFindNewTarget();
		}
		if (!isValid && currentTargetInt.IsValid && def.building.playTargetAcquiredSound)
		{
			SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		if (currentTargetInt.IsValid)
		{
			float randomInRange = def.building.turretBurstWarmupTime.RandomInRange;
			if (randomInRange > 0f)
			{
				burstWarmupTicksLeft = randomInRange.SecondsToTicks();
			}
			else if (canBeginBurstImmediately)
			{
				BeginBurst();
			}
			else
			{
				burstWarmupTicksLeft = 1;
			}
		}
		else
		{
			ResetCurrentTarget();
		}
	}

	public virtual LocalTargetInfo TryFindNewTarget()
	{
		IAttackTargetSearcher attackTargetSearcher = TargSearcher();
		Faction faction = attackTargetSearcher.Thing.Faction;
		float range = AttackVerb.verbProps.range;
		if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate(Building x)
		{
			float num = AttackVerb.verbProps.EffectiveMinRange(x, this);
			float num2 = x.Position.DistanceToSquared(base.Position);
			return num2 > num * num && num2 < range * range;
		}).TryRandomElement(out var result))
		{
			return result;
		}
		TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
		if (!AttackVerb.ProjectileFliesOverhead())
		{
			targetScanFlags |= TargetScanFlags.NeedLOSToAll;
			targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
		}
		if (AttackVerb.IsIncendiary_Ranged())
		{
			targetScanFlags |= TargetScanFlags.NeedNonBurning;
		}
		if (IsMortar)
		{
			targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
		}
		return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, IsValidTarget);
	}

	private IAttackTargetSearcher TargSearcher()
	{
		if (mannableComp != null && mannableComp.MannedNow)
		{
			return mannableComp.ManningPawn;
		}
		return this;
	}

	private bool IsValidTarget(Thing t)
	{
		if (t is Pawn pawn)
		{
			if (base.Faction == Faction.OfPlayer && pawn.IsPrisoner)
			{
				return false;
			}
			if (AttackVerb.ProjectileFliesOverhead())
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
				if (roofDef != null && roofDef.isThickRoof)
				{
					return false;
				}
			}
			if (mannableComp == null)
			{
				return !GenAI.MachinesLike(base.Faction, pawn);
			}
			if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void BeginBurst()
	{
		AttackVerb.TryStartCastOn(CurrentTarget);
		OnAttackedTarget(CurrentTarget);
	}

	protected void BurstComplete()
	{
		burstCooldownTicksLeft = BurstCooldownTime().SecondsToTicks();
	}

	protected float BurstCooldownTime()
	{
		if (def.building.turretBurstCooldownTime >= 0f)
		{
			return def.building.turretBurstCooldownTime;
		}
		return AttackVerb.verbProps.defaultCooldownTime;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		if (AttackVerb.verbProps.minRange > 0f)
		{
			stringBuilder.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
		}
		if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
		{
			stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
		}
		else if (base.Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
		{
			stringBuilder.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
		}
		CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
		if (compChangeableProjectile != null)
		{
			if (compChangeableProjectile.Loaded)
			{
				stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
			}
			else
			{
				stringBuilder.AppendLine("ShellNotLoaded".Translate());
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		Vector3 drawOffset = new Vector3(0f, 0f, 0f);
		float angleOffset = 0f;
		AncotLibrary.EquipmentUtility.Recoil(def.building.turretGunDef, (Verb_LaunchProjectile)AttackVerb, out drawOffset, out angleOffset, top.CurRotation);
		top.DrawTurret(drawLoc, drawOffset, angleOffset);
		DrawTurretTop();
	}

	public void DrawTurretTop()
	{
		Vector3 vector = new Vector3(def.building.turretTopOffset.x, 0f, def.building.turretTopOffset.y).RotatedBy(top.CurRotation);
		float turretTopDrawSize = def.building.turretTopDrawSize;
		float num = base.CurrentEffectiveVerb?.AimAngleOverride ?? top.CurRotation;
		Matrix4x4 matrix = default(Matrix4x4);
		Vector3 drawPos = DrawPos;
		drawPos.y += 0.2f;
		matrix.SetTRS(drawPos + Altitudes.AltIncVect + vector, (-90f + num).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
		Graphics.DrawMesh(MeshPool.plane10, matrix, Turret, 0);
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		float range = AttackVerb.verbProps.range;
		if (range < 90f)
		{
			GenDraw.DrawRadiusRing(base.Position, range);
		}
		float num = AttackVerb.verbProps.EffectiveMinRange(allowAdjacentShot: true);
		if (num < 90f && num > 0.1f)
		{
			GenDraw.DrawRadiusRing(base.Position, num);
		}
		if (WarmingUp)
		{
			int degreesWide = (int)((float)burstWarmupTicksLeft * 0.5f);
			GenDraw.DrawAimPie(this, CurrentTarget, degreesWide, (float)def.size.x * 0.5f);
		}
		if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
		{
			Vector3 b = ((!forcedTarget.HasThing) ? forcedTarget.Cell.ToVector3Shifted() : forcedTarget.Thing.TrueCenter());
			Vector3 a = this.TrueCenter();
			b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
			a.y = b.y;
			GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (CanExtractShell)
		{
			CompChangeableProjectile compChangeableProjectile = gun.TryGetComp<CompChangeableProjectile>();
			yield return new Command_Action
			{
				defaultLabel = "CommandExtractShell".Translate(),
				defaultDesc = "CommandExtractShellDesc".Translate(),
				icon = compChangeableProjectile.LoadedShell.uiIcon,
				iconAngle = compChangeableProjectile.LoadedShell.uiIconAngle,
				iconOffset = compChangeableProjectile.LoadedShell.uiIconOffset,
				iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.LoadedShell),
				action = delegate
				{
					ExtractShell();
				}
			};
		}
		CompChangeableProjectile compChangeableProjectile2 = gun.TryGetComp<CompChangeableProjectile>();
		if (compChangeableProjectile2 != null)
		{
			StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
			foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
			{
				yield return item;
			}
		}
		if (!HideForceTargetGizmo)
		{
			if (CanSetForcedTarget)
			{
				Command_VerbTarget command_VerbTarget = new Command_VerbTarget
				{
					defaultLabel = "CommandSetForceAttackTarget".Translate(),
					defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
					verb = AttackVerb,
					hotKey = KeyBindingDefOf.Misc4,
					drawRadius = false,
					requiresAvailableVerb = false
				};
				if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
				{
					command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
				}
				yield return command_VerbTarget;
			}
			if (forcedTarget.IsValid)
			{
				Command_Action command_Action2 = new Command_Action
				{
					defaultLabel = "CommandStopForceAttack".Translate(),
					defaultDesc = "CommandStopForceAttackDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
					action = delegate
					{
						ResetForcedTarget();
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				};
				if (!forcedTarget.IsValid)
				{
					command_Action2.Disable("CommandStopAttackFailNotForceAttacking".Translate());
				}
				command_Action2.hotKey = KeyBindingDefOf.Misc5;
				yield return command_Action2;
			}
		}
		if (!CanToggleHoldFire)
		{
			yield break;
		}
		yield return new Command_Toggle
		{
			defaultLabel = "CommandHoldFire".Translate(),
			defaultDesc = "CommandHoldFireDesc".Translate(),
			icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
			hotKey = KeyBindingDefOf.Misc6,
			toggleAction = delegate
			{
				holdFire = !holdFire;
				if (holdFire)
				{
					ResetForcedTarget();
				}
			},
			isActive = () => holdFire
		};
	}

	private void ExtractShell()
	{
		GenPlace.TryPlaceThing(gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near);
	}

	private void ResetForcedTarget()
	{
		forcedTarget = LocalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
		if (burstCooldownTicksLeft <= 0)
		{
			TryStartShootSomething(canBeginBurstImmediately: false);
		}
	}

	private void ResetCurrentTarget()
	{
		currentTargetInt = LocalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
	}

	public void MakeGun()
	{
		gun = ThingMaker.MakeThing(def.building.turretGunDef);
		UpdateGunVerbs();
	}

	private void UpdateGunVerbs()
	{
		List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			Verb verb = allVerbs[i];
			verb.caster = this;
			verb.castCompleteCallback = BurstComplete;
		}
	}

	public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		if (!dinfo.Def.isExplosive)
		{
			dinfo.SetAmount(dinfo.Amount * (1f - damageReductionFactor));
		}
		base.PreApplyDamage(ref dinfo, out absorbed);
	}

	private float TryGetOverallArmor(Pawn pawn, StatDef stat)
	{
		float num = 0f;
		float num2 = Mathf.Clamp01(pawn.GetStatValue(stat) / 2f);
		List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
		List<Apparel> list = ((pawn.apparel != null) ? pawn.apparel.WornApparel : null);
		for (int i = 0; i < allParts.Count; i++)
		{
			float num3 = 1f - num2;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].def.apparel.CoversBodyPart(allParts[i]))
					{
						float num4 = Mathf.Clamp01(list[j].GetStatValue(stat) / 2f);
						num3 *= 1f - num4;
					}
				}
			}
			num += allParts[i].coverageAbs * (1f - num3);
		}
		return Mathf.Clamp(num * 2f, 0f, 2f);
	}

	public void Modification_AutoRepair()
	{
		if (ModsConfig.IsActive("Ancot.MilianModification") && compThingContainer.innerPawn.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_FortressDamageControl) != null)
		{
			HitPoints += 6;
			HitPoints = Mathf.Min(HitPoints, base.MaxHitPoints);
		}
	}
}
