using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended;

public class AmmoThing : ThingWithComps
{
	private int numToCookOff;

	private int shouldDestroy = -1;

	public AmmoDef AmmoDef => def as AmmoDef;

	public bool IsCookingOff => numToCookOff > 0 && CanCookOffOrDetonate();

	public bool ShouldDestroy
	{
		get
		{
			if (shouldDestroy == -1)
			{
				shouldDestroy = ((!def.IsWeapon && AmmoDef?.tradeTags?.Contains("CE_AmmoInjector") == true) ? 1 : 0);
			}
			return shouldDestroy == 1 && !AmmoUtility.IsAmmoSystemActive(AmmoDef);
		}
	}

	public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		base.PreApplyDamage(ref dinfo, out absorbed);
		if (!absorbed && base.Spawned && CanCookOffOrDetonate() && dinfo.Def.ExternalViolenceFor(this))
		{
			if ((float)HitPoints - dinfo.Amount > 0f)
			{
				numToCookOff += Mathf.RoundToInt((float)def.stackLimit * (dinfo.Amount / (float)HitPoints) * (def.smallVolume ? Rand.Range(1f, 2f) : Rand.Range(0f, 1f)));
			}
			else if (this.TryGetComp<CompExplosive>() == null || !this.TryGetComp<CompExplosive>().Props.explodeOnKilled)
			{
				TryDetonate(stackCount);
			}
		}
	}

	public override void Tick()
	{
		if (ShouldDestroy)
		{
			Destroy();
		}
		base.Tick();
		if (numToCookOff > 0 && Rand.Chance((float)numToCookOff / (float)def.stackLimit) && (TryLaunchCookOffProjectile() || TryDetonate()))
		{
			if (stackCount > 1)
			{
				numToCookOff--;
				stackCount--;
			}
			else
			{
				numToCookOff = 0;
				Destroy(DestroyMode.KillFinalize);
			}
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		if (AmmoUtility.IsAmmoSystemActive(AmmoDef))
		{
			int num = AmmoDef?.Users.Count ?? 0;
			if (num >= 1)
			{
				stringBuilder.AppendLine("CE_UsedBy".Translate() + ": " + AmmoDef.Users.FirstOrDefault().LabelCap + ((AmmoDef.Users.Count > 1) ? (" (+" + (AmmoDef.Users.Count - 1) + ")") : ""));
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private bool TryDetonate(float stackCountScale = 1f)
	{
		if (Find.Maps.IndexOf(base.Map) < 0)
		{
			return false;
		}
		CompExplosiveCE compExplosiveCE = this.TryGetComp<CompExplosiveCE>();
		ProjectileProperties projectileProperties = AmmoDef?.detonateProjectile?.projectile;
		if (compExplosiveCE != null || projectileProperties != null)
		{
			if (Rand.Chance(Mathf.Clamp01(0.75f - Mathf.Pow(HitPoints / base.MaxHitPoints, 2f))))
			{
				if (compExplosiveCE != null)
				{
					compExplosiveCE.Explode(this, base.Position.ToVector3Shifted(), base.Map, Mathf.Pow(stackCountScale, 0.333f), null, new List<Thing> { this });
				}
				else
				{
					this.TryGetComp<CompFragments>()?.Throw(base.Position.ToVector3Shifted(), base.Map, this);
				}
				if (projectileProperties != null)
				{
					GenExplosionCE.DoExplosion(base.Position, base.Map, projectileProperties.explosionRadius, projectileProperties.damageDef, this, projectileProperties.GetDamageAmount(1f), projectileProperties.GetExplosionArmorPenetration(), projectileProperties.soundExplode, null, def, null, projectileProperties.postExplosionSpawnThingDef, projectileProperties.postExplosionSpawnChance, projectileProperties.postExplosionSpawnThingCount, projectileProperties.postExplosionGasType, projectileProperties.applyDamageToExplosionCellsNeighbors, projectileProperties.preExplosionSpawnThingDef, projectileProperties.preExplosionSpawnChance, projectileProperties.preExplosionSpawnThingCount, projectileProperties.explosionChanceToStartFire, projectileProperties.explosionDamageFalloff, null, new List<Thing> { this }, null, doVisualEffects: true, 1f, 0f, doSoundEffects: true, projectileProperties.postExplosionSpawnThingDefWater, projectileProperties.screenShakeFactor, null, null, 0f, Mathf.Pow(stackCountScale, 0.333f));
				}
			}
			return true;
		}
		return false;
	}

	private bool TryLaunchCookOffProjectile()
	{
		if (AmmoDef == null || AmmoDef.cookOffProjectile == null || Find.Maps.IndexOf(base.Map) < 0)
		{
			return false;
		}
		if (!Controller.settings.RealisticCookOff)
		{
			ProjectileCE projectileCE = (ProjectileCE)ThingMaker.MakeThing(AmmoDef.cookOffProjectile);
			GenSpawn.Spawn(projectileCE, base.PositionHeld, base.MapHeld);
			projectileCE.canTargetSelf = true;
			projectileCE.minCollisionDistance = 0f;
			projectileCE.logMisses = false;
			projectileCE.Launch(this, new Vector2(DrawPos.x, DrawPos.z), Mathf.Acos(2f * Rand.Range(0.5f, 1f) - 1f), Rand.Range(0, 360), 0.1f, AmmoDef.cookOffProjectile.projectile.speed * AmmoDef.cookOffSpeed, this);
		}
		if ((double)AmmoDef.cookOffFlashScale > 0.01)
		{
			FleckMakerCE.Static(base.Position, base.Map, FleckDefOf.ShotFlash, AmmoDef.cookOffFlashScale);
		}
		if (AmmoDef.cookOffSound != null)
		{
			AmmoDef.cookOffSound.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		if (AmmoDef.cookOffTailSound != null)
		{
			AmmoDef.cookOffTailSound.PlayOneShotOnCamera();
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref numToCookOff, "numToCookOff", 0);
	}

	private bool CanCookOffOrDetonate()
	{
		return AmmoDef?.cookOffProjectile != null || AmmoDef?.detonateProjectile != null;
	}
}
