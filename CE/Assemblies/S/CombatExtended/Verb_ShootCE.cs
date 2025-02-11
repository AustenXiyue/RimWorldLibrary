using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class Verb_ShootCE : Verb_LaunchProjectileCE
{
	private const int AimTicksMin = 30;

	private const int AimTicksMax = 240;

	private const float PawnXp = 20f;

	private const float HostileXp = 170f;

	private const float SuppressionSwayFactor = 1.5f;

	private bool _isAiming;

	public Vector3 drawPos;

	public bool isBipodGun => (base.EquipmentSource?.TryGetComp<BipodComp>() ?? null) != null;

	protected override int ShotsPerBurst
	{
		public get
		{
			return (CompFireModes != null) ? ShotsPerBurstFor(CompFireModes.CurrentFireMode) : base.VerbPropsCE.burstShotCount;
		}
	}

	private bool ShouldAim
	{
		get
		{
			if (CompFireModes != null)
			{
				if (base.ShooterPawn != null)
				{
					if (base.ShooterPawn.CurJob != null && base.ShooterPawn.CurJob.def == JobDefOf.Hunt)
					{
						return true;
					}
					if (IsSuppressed)
					{
						return false;
					}
					Pawn_PathFollower pather = base.ShooterPawn.pather;
					if (pather != null && pather.Moving)
					{
						return false;
					}
				}
				return CompFireModes.CurrentAimMode == AimMode.AimedShot;
			}
			return false;
		}
	}

	public override float SwayAmplitude
	{
		get
		{
			float swayAmplitude = base.SwayAmplitude;
			float b = base.SightsEfficiency;
			if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				b = 0f;
			}
			if (ShouldAim)
			{
				return swayAmplitude * Mathf.Max(0f, 1f - base.AimingAccuracy) / Mathf.Max(1f, b);
			}
			if (IsSuppressed)
			{
				return swayAmplitude * 1.5f;
			}
			return swayAmplitude;
		}
	}

	public float AimAngle
	{
		get
		{
			if (base.CurrentTarget == null)
			{
				return 143f;
			}
			Vector3 vector = ((base.CurrentTarget.Thing == null) ? base.CurrentTarget.Cell.ToVector3Shifted() : base.CurrentTarget.Thing.DrawPos);
			float result = 143f;
			if ((vector - caster.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
			{
				result = (vector - caster.DrawPos).AngleFlat();
			}
			return result;
		}
	}

	private bool IsSuppressed => base.ShooterPawn?.TryGetComp<CompSuppressable>()?.isSuppressed == true;

	public override CompAmmoUser CompAmmo => base.CompAmmo;

	public override ThingDef Projectile => (CompAmmo?.CurrentAmmo != null) ? CompAmmo.CurAmmoProjectile : base.Projectile;

	public float SwayAmplitudeFor(AimMode mode)
	{
		float swayAmplitude = base.SwayAmplitude;
		float b = base.SightsEfficiency;
		if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
		{
			b = 0f;
		}
		if (ShouldAimFor(mode))
		{
			return swayAmplitude * Mathf.Max(0f, 1f - base.AimingAccuracy) / Mathf.Max(1f, b);
		}
		if (IsSuppressed)
		{
			return swayAmplitude * 1.5f;
		}
		return swayAmplitude;
	}

	public bool ShouldAimFor(AimMode mode)
	{
		if (base.ShooterPawn != null)
		{
			if (base.ShooterPawn.CurJob != null && base.ShooterPawn.CurJob.def == JobDefOf.Hunt)
			{
				return true;
			}
			if (IsSuppressed)
			{
				return false;
			}
			Pawn_PathFollower pather = base.ShooterPawn.pather;
			if (pather != null && pather.Moving)
			{
				return false;
			}
		}
		return mode == AimMode.AimedShot;
	}

	public virtual int ShotsPerBurstFor(FireMode mode)
	{
		if (CompFireModes != null)
		{
			switch (mode)
			{
			case FireMode.SingleFire:
				return 1;
			case FireMode.BurstFire:
				if (CompFireModes.Props.aimedBurstShotCount > 0)
				{
					return CompFireModes.Props.aimedBurstShotCount;
				}
				break;
			}
		}
		float num = base.VerbPropsCE.burstShotCount;
		if (base.EquipmentSource != null)
		{
			CompUnderBarrel compUnderBarrel = base.EquipmentSource.TryGetComp<CompUnderBarrel>();
			if (compUnderBarrel != null && !compUnderBarrel.usingUnderBarrel)
			{
				float statValue = base.EquipmentSource.GetStatValue(CE_StatDefOf.BurstShotCount);
				if (statValue > 0f)
				{
					num = statValue;
				}
			}
		}
		return (int)num;
	}

	public override void WarmupComplete()
	{
		float lengthHorizontal = (currentTarget.Cell - caster.Position).LengthHorizontal;
		int num = (int)Mathf.Lerp(30f, 240f, lengthHorizontal / 100f);
		if (ShouldAim && !_isAiming)
		{
			if (caster is Building_TurretGunCE building_TurretGunCE)
			{
				building_TurretGunCE.burstWarmupTicksLeft += num;
				_isAiming = true;
				return;
			}
			if (base.ShooterPawn != null)
			{
				base.ShooterPawn.stances.SetStance(new Stance_Warmup(num, currentTarget, this));
				_isAiming = true;
				RecalculateWarmupTicks();
				return;
			}
		}
		base.WarmupComplete();
		_isAiming = false;
		if (base.ShooterPawn?.skills != null && currentTarget.Thing is Pawn)
		{
			float num2 = verbProps.AdjustedFullCycleTime(this, base.ShooterPawn);
			num2 += num.TicksToSeconds();
			float num3 = (currentTarget.Thing.HostileTo(base.ShooterPawn) ? 170f : 20f);
			num3 *= num2;
			base.ShooterPawn.skills.Learn(SkillDefOf.Shooting, num3);
		}
	}

	public override bool Available()
	{
		if (!base.Available())
		{
			return false;
		}
		if ((base.ShooterPawn?.CurJobDef == JobDefOf.AttackStatic || base.WarmingUp) && !(CompAmmo?.CanBeFiredNow ?? true))
		{
			CompAmmo?.TryStartReload();
			resetRetarget();
			return false;
		}
		return true;
	}

	public override void VerbTickCE()
	{
		if (_isAiming)
		{
			if (!ShouldAim)
			{
				WarmupComplete();
			}
			if (!(caster is Building_TurretGunCE) && base.ShooterPawn?.stances?.curStance?.GetType() != typeof(Stance_Warmup))
			{
				_isAiming = false;
			}
		}
		if (isBipodGun && Controller.settings.BipodMechanics)
		{
			base.EquipmentSource.TryGetComp<BipodComp>().SetUpStart(CasterPawn);
		}
	}

	public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
	{
		if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
		{
			if (!base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
			{
				return false;
			}
			float num = targ.Cell.DistanceTo(root);
			if (num < 5f)
			{
				return base.CanHitTargetFrom(root, targ);
			}
			Map map = base.ShooterPawn.Map;
			LightingTracker lightingTracker = map.GetLightingTracker();
			float glowForCell = lightingTracker.GetGlowForCell(targ.Cell);
			if (glowForCell / num < 0.1f)
			{
				return false;
			}
		}
		return base.CanHitTargetFrom(root, targ);
	}

	public override void RecalculateWarmupTicks()
	{
		if (!Controller.settings.FasterRepeatShots)
		{
			return;
		}
		Vector3 vector = caster.TrueCenter();
		Vector3 vector2 = currentTarget.Thing?.TrueCenter() ?? currentTarget.Cell.ToVector3Shifted();
		Pawn pawn = currentTarget.Pawn;
		if (pawn != null)
		{
			vector2 += pawn.Drawer.leaner.LeanOffset * 0.5f;
		}
		Vector3 vector3 = vector2 - vector;
		float num = (-90f + 57.29578f * Mathf.Atan2(vector3.z, vector3.x)) % 360f;
		float num2 = Mathf.Abs(num - lastShotRotation) + lastRecoilDeg;
		lastRecoilDeg = 0f;
		float? num3 = storedShotReduction;
		float num4;
		if (!num3.HasValue)
		{
			CompFireModes obj = CompFireModes;
			num4 = ((obj != null && obj.CurrentAimMode == AimMode.SuppressFire) ? 0.1f : (_isAiming ? 0.5f : 0.25f));
		}
		else
		{
			num4 = num3.GetValueOrDefault();
		}
		float a = num4;
		float num5 = Mathf.Max(a, num2 / 45f);
		storedShotReduction = num5;
		if (!(num5 < 1f))
		{
			return;
		}
		if (caster is Building_TurretGunCE building_TurretGunCE)
		{
			if (building_TurretGunCE.burstWarmupTicksLeft > 0)
			{
				building_TurretGunCE.burstWarmupTicksLeft = (int)((float)building_TurretGunCE.burstWarmupTicksLeft * num5);
			}
		}
		else if (base.WarmupStance != null)
		{
			base.WarmupStance.ticksLeft = (int)((float)base.WarmupStance.ticksLeft * num5);
		}
	}

	public void ExternalCallDropCasing(int randomSeedOffset = -1)
	{
		bool flag = false;
		GunDrawExtension extension = base.EquipmentSource?.def.GetModExtension<GunDrawExtension>();
		if (base.ShooterPawn != null)
		{
			flag = drawPos != Vector3.zero;
		}
		CE_Utility.GenerateAmmoCasings(base.projectilePropsCE, flag ? drawPos : caster.DrawPos, caster.Map, 0f, base.VerbPropsCE.recoilAmount, flag, extension, randomSeedOffset);
	}

	public override bool TryCastShot()
	{
		CompAmmoUser compAmmoUser = CompAmmo;
		if (compAmmoUser != null && !compAmmoUser.TryPrepareShot())
		{
			return false;
		}
		if (base.TryCastShot())
		{
			return OnCastSuccessful();
		}
		return false;
	}

	protected virtual bool OnCastSuccessful()
	{
		bool flag = false;
		GunDrawExtension gunDrawExtension = base.EquipmentSource?.def.GetModExtension<GunDrawExtension>();
		if (base.ShooterPawn != null)
		{
			base.ShooterPawn.records.Increment(RecordDefOf.ShotsFired);
			flag = drawPos != Vector3.zero;
		}
		if (base.VerbPropsCE.ejectsCasings && (gunDrawExtension == null || !gunDrawExtension.DropCasingWhenReload))
		{
			CE_Utility.GenerateAmmoCasings(base.projectilePropsCE, flag ? drawPos : caster.DrawPos, caster.Map, AimAngle, base.VerbPropsCE.recoilAmount, flag, gunDrawExtension);
		}
		if (CompAmmo == null)
		{
			return true;
		}
		int ammoConsumedPerShot = (CompAmmo.Props.ammoSet?.ammoConsumedPerShot ?? 1) * base.VerbPropsCE.ammoConsumedPerShotCount;
		CompAmmo.Notify_ShotFired(ammoConsumedPerShot);
		if (base.ShooterPawn != null && !CompAmmo.CanBeFiredNow)
		{
			CompAmmo.TryStartReload();
			resetRetarget();
		}
		if (!CompAmmo.HasMagazine && CompAmmo.UseAmmo)
		{
			if (!CompAmmo.HasAmmoOrMagazine)
			{
				if (base.VerbPropsCE.muzzleFlashScale > 0.01f)
				{
					FleckMakerCE.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, base.VerbPropsCE.muzzleFlashScale);
				}
				if (base.VerbPropsCE.soundCast != null)
				{
					base.VerbPropsCE.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
				}
				if (base.VerbPropsCE.soundCastTail != null)
				{
					base.VerbPropsCE.soundCastTail.PlayOneShotOnCamera();
				}
				if (base.ShooterPawn != null && base.ShooterPawn.thinker != null)
				{
					base.ShooterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
				}
			}
			return CompAmmo.Notify_PostShotFired();
		}
		return true;
	}
}
