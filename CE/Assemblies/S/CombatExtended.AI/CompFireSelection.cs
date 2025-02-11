using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.AI;

public class CompFireSelection : ICompTactics
{
	private FireMode lastFireMode = FireMode.AutoFire;

	private AimMode lastAimMode = AimMode.Snapshot;

	private LocalTargetInfo? _castTarg = null;

	private LocalTargetInfo? _destTarg = null;

	private int _NVEfficiencyAge = -1;

	private float _NVEfficiency = -1f;

	public override int Priority => 0;

	public float NightVisionEfficiency
	{
		get
		{
			if (_NVEfficiency == -1f || GenTicks.TicksGame - _NVEfficiencyAge > 250)
			{
				_NVEfficiency = SelPawn.GetStatValue(CE_StatDefOf.NightVisionEfficiency);
				_NVEfficiencyAge = GenTicks.TicksGame;
			}
			return _NVEfficiency;
		}
	}

	public virtual bool ShouldRun => !(SelPawn.Faction?.IsPlayer ?? false);

	public override bool StartCastChecks(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		_castTarg = castTarg;
		_destTarg = destTarg;
		return true;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastFireMode, "lastFireMode", FireMode.AutoFire);
		Scribe_Values.Look(ref lastAimMode, "lastAimMode", AimMode.Snapshot);
	}

	public override void OnStartCastSuccess(Verb verb)
	{
		base.OnStartCastSuccess(verb);
		if (ShouldRun)
		{
			CompFireModes compFireModes = verb.EquipmentSource?.TryGetComp<CompFireModes>() ?? null;
			if (verb.EquipmentSource != null && compFireModes != null && _castTarg.HasValue && _destTarg.HasValue)
			{
				OptimizeModes(compFireModes, verb, _castTarg.Value, _destTarg.Value);
			}
			_castTarg = null;
			_destTarg = null;
		}
	}

	public void OptimizeModes(CompFireModes fireModes, Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		ThingWithComps equipmentSource = verb.EquipmentSource;
		if ((equipmentSource != null && equipmentSource.def.IsIlluminationDevice()) || verb is Verb_ShootFlareCE)
		{
			fireModes.TrySetAimMode(AimMode.Snapshot);
			fireModes.TrySetFireMode(FireMode.AutoFire);
			return;
		}
		ThingWithComps equipmentSource2 = verb.EquipmentSource;
		if (equipmentSource2 != null && equipmentSource2.def.IsAOEWeapon())
		{
			fireModes.TrySetAimMode(AimMode.Snapshot);
			fireModes.TrySetFireMode(FireMode.AutoFire);
		}
		else
		{
			if (!(verb is Verb_ShootCE verb_ShootCE) || !(verb.verbProps is VerbPropertiesCE verbPropertiesCE) || !(verbPropertiesCE.defaultProjectile?.projectile is ProjectilePropertiesCE projectilePropertiesCE))
			{
				return;
			}
			if (verb_ShootCE.CompAmmo == null)
			{
				fireModes.TrySetAimMode(AimMode.AimedShot);
				fireModes.TrySetFireMode(FireMode.AutoFire);
				return;
			}
			float num = castTarg.Cell.DistanceTo(SelPawn.Position);
			if (projectilePropertiesCE.pelletCount > 1 && num < 20f)
			{
				fireModes.TrySetAimMode(AimMode.Snapshot);
				fireModes.TrySetFireMode(FireMode.AutoFire);
				return;
			}
			float num2 = verb_ShootCE.CompAmmo.CurMagCount + verb_ShootCE.CompAmmo.MagsLeft;
			if (castTarg.Thing is Pawn other)
			{
				if (SelPawn.EdgingCloser(other))
				{
					if (num <= 15f)
					{
						fireModes.TrySetAimMode(AimMode.SuppressFire);
						fireModes.TrySetFireMode(FireMode.AutoFire);
						return;
					}
					if (num <= 30f)
					{
						fireModes.TrySetAimMode(AimMode.Snapshot);
						fireModes.TrySetFireMode(FireMode.AutoFire);
						return;
					}
				}
				float num3 = Mathf.Max(verb.EffectiveRange, 1f);
				float num4 = verb_ShootCE.RecoilAmount * (0.6f + num / num3);
				if (num / num3 > 0.5f && !base.Map.VisibilityGoodAt(SelPawn, castTarg.Cell, NightVisionEfficiency))
				{
					fireModes.TrySetAimMode(AimMode.AimedShot);
					fireModes.TrySetFireMode(FireMode.BurstFire);
					return;
				}
				if (num4 <= 0.6f)
				{
					if (num / num3 < 0.4f)
					{
						fireModes.TrySetAimMode(AimMode.Snapshot);
						fireModes.TrySetFireMode(FireMode.AutoFire);
					}
					else
					{
						fireModes.TrySetAimMode(AimMode.AimedShot);
						fireModes.TrySetFireMode(FireMode.AutoFire);
					}
					return;
				}
				if (num4 > 3.5f && num / num3 >= 0.6f)
				{
					fireModes.TrySetAimMode(AimMode.AimedShot);
					fireModes.TrySetFireMode(FireMode.BurstFire);
					return;
				}
			}
			if (castTarg.Thing is Building_Turret)
			{
				if (num > 40f)
				{
					fireModes.TrySetAimMode(AimMode.AimedShot);
					fireModes.TrySetFireMode(FireMode.BurstFire);
				}
				else
				{
					fireModes.TrySetAimMode(AimMode.Snapshot);
					fireModes.TrySetFireMode(FireMode.AutoFire);
				}
			}
			else if (num2 < (float)verb_ShootCE.CompAmmo.MagSize && num > 50f)
			{
				fireModes.TrySetAimMode(AimMode.AimedShot);
				fireModes.TrySetFireMode(FireMode.SingleFire);
			}
			else if (num2 < (float)verb_ShootCE.CompAmmo.MagSize * 1.5f && num > 35f)
			{
				fireModes.TrySetAimMode(AimMode.AimedShot);
				fireModes.TrySetFireMode(FireMode.BurstFire);
			}
			else
			{
				fireModes.TrySetAimMode(AimMode.AimedShot);
				fireModes.TrySetFireMode(FireMode.AutoFire);
			}
		}
	}
}
