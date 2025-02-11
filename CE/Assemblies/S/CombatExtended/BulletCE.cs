using System;
using System.Collections.Generic;
using CombatExtended.AI;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended;

public class BulletCE : ProjectileCE
{
	private static RulePackDef cookOffDamageEvent;

	private static RulePackDef shellingDamageEvent;

	public static RulePackDef CookOff => cookOffDamageEvent ?? (cookOffDamageEvent = DefDatabase<RulePackDef>.GetNamed("DamageEvent_CookOff"));

	public static RulePackDef Shelling => shellingDamageEvent ?? (shellingDamageEvent = DefDatabase<RulePackDef>.GetNamed("DamageEvent_Shelling"));

	public virtual float PenetrationAmount
	{
		get
		{
			ProjectilePropertiesCE projectilePropertiesCE = (ProjectilePropertiesCE)def.projectile;
			bool flag = def.projectile.damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp;
			float num = (equipment?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f) * (flag ? projectilePropertiesCE.armorPenetrationSharp : projectilePropertiesCE.armorPenetrationBlunt);
			return lerpPosition ? num : (num * base.RemainingKineticEnergyPct);
		}
	}

	private void LogImpact(Thing hitThing, out LogEntry_DamageResult logEntry)
	{
		ThingDef weaponDef = equipmentDef ?? ThingDef.Named("Gun_Autopistol");
		logEntry = new BattleLogEntry_RangedImpact(launcher, hitThing, base.intendedTargetThing, weaponDef, def, null);
		if (!(launcher is AmmoThing))
		{
			Find.BattleLog.Add(logEntry);
		}
	}

	public override void Impact(Thing hitThing)
	{
		bool flag = launcher is AmmoThing;
		Map map = base.Map;
		LogEntry_DamageResult logEntry = null;
		if (!flag && launcher != null && (logMisses || hitThing is Pawn || hitThing is Building_Turret))
		{
			LogImpact(hitThing, out logEntry);
		}
		if (hitThing != null)
		{
			float amount = DamageAmount;
			ProjectilePropertiesCE projectilePropertiesCE = (ProjectilePropertiesCE)def.projectile;
			bool flag2 = def.projectile.damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp;
			float penetrationAmount = PenetrationAmount;
			DamageDefExtensionCE damageDefExtensionCE = def.projectile.damageDef.GetModExtension<DamageDefExtensionCE>() ?? new DamageDefExtensionCE();
			DamageInfo damageInfo = new DamageInfo(def.projectile.damageDef, amount, penetrationAmount, ExactRotation.eulerAngles.y, launcher, null, def, DamageInfo.SourceCategory.ThingOrUnknown, null, base.InstigatorGuilty);
			BodyPartDepth depth = (damageDefExtensionCE.harmOnlyOutsideLayers ? BodyPartDepth.Outside : BodyPartDepth.Undefined);
			BodyPartHeight collisionBodyHeight = new CollisionVertical(hitThing).GetCollisionBodyHeight(ExactPosition.y);
			damageInfo.SetBodyRegion(collisionBodyHeight, depth);
			if (damageDefExtensionCE.harmOnlyOutsideLayers)
			{
				damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			}
			if (flag && hitThing is Pawn recipient)
			{
				logEntry = new BattleLogEntry_DamageTaken(recipient, CookOff);
				Find.BattleLog.Add(logEntry);
			}
			if (launcher == null && hitThing is Pawn recipient2)
			{
				logEntry = new BattleLogEntry_DamageTaken(recipient2, Shelling);
				Find.BattleLog.Add(logEntry);
			}
			try
			{
				DamageWorker.DamageResult damageResult = hitThing.TakeDamage(damageInfo);
				if (launcher != null)
				{
					damageResult.AssociateWithLog(logEntry);
				}
				if (!(hitThing is Pawn) && projectilePropertiesCE != null && !projectilePropertiesCE.secondaryDamage.NullOrEmpty())
				{
					foreach (SecondaryDamage item in projectilePropertiesCE.secondaryDamage)
					{
						if (hitThing.Destroyed || !Rand.Chance(item.chance))
						{
							break;
						}
						DamageInfo dinfo = item.GetDinfo(damageInfo);
						hitThing.TakeDamage(dinfo).AssociateWithLog(logEntry);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error($"CombatExtended :: BulletCE impacting thing {hitThing.LabelCap} of def {hitThing.def.LabelCap} added by mod {hitThing.def.modContentPack.Name}.\n{ex}");
				throw ex;
			}
			finally
			{
				base.Impact(hitThing);
			}
		}
		else
		{
			SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
			if (castShadow)
			{
				Rand.PushState();
				FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
				if (base.Position.GetTerrain(map).takeSplashes)
				{
					FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(DamageAmount), 4f);
				}
				Rand.PopState();
			}
			base.Impact(null);
		}
		NotifyImpact(hitThing, map, base.Position);
	}

	private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
	{
		Bullet bullet = GenerateVanillaBullet();
		BulletImpactData bulletImpactData = default(BulletImpactData);
		bulletImpactData.bullet = bullet;
		bulletImpactData.hitThing = hitThing;
		bulletImpactData.impactPosition = position;
		BulletImpactData impactData = bulletImpactData;
		hitThing?.Notify_BulletImpactNearby(impactData);
		for (int i = 0; i < 9; i++)
		{
			IntVec3 c = position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] != hitThing)
				{
					thingList[j].Notify_BulletImpactNearby(impactData);
				}
				if (thingList[j] is Pawn pawn)
				{
					pawn.GetTacticalManager()?.Notify_BulletImpactNearby();
				}
			}
		}
		bullet.Destroy();
	}

	private Bullet GenerateVanillaBullet()
	{
		Bullet bullet = new Bullet
		{
			def = def,
			intendedTarget = base.intendedTargetThing
		};
		((Projectile)bullet).launcher = launcher;
		return bullet;
	}
}
