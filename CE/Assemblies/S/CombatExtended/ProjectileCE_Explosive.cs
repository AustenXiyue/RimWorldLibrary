using CombatExtended.Utilities;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ProjectileCE_Explosive : ProjectileCE
{
	private int ticksToDetonation;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
	}

	public override void Tick()
	{
		base.Tick();
		if (ticksToDetonation <= 0)
		{
			return;
		}
		ticksToDetonation--;
		if (ticksToDetonation <= 0)
		{
			base.Impact(null);
		}
		else
		{
			if (!((def.projectile as ProjectilePropertiesCE).suppressionFactor > 0f) || !landed)
			{
				return;
			}
			foreach (Pawn item in ExactPosition.ToIntVec3().PawnsInRange(base.Map, 3f + def.projectile.explosionRadius + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f)))
			{
				ApplySuppression(item, 1f - (float)(ticksToDetonation / def.projectile.explosionDelay));
			}
		}
	}

	public override void Impact(Thing hitThing)
	{
		if (hitThing is Pawn)
		{
			Vector3 drawPos = hitThing.DrawPos;
			drawPos.y = ExactPosition.y;
			ExactPosition = drawPos;
			base.Position = ExactPosition.ToIntVec3();
		}
		if (def.projectile.explosionDelay == 0)
		{
			base.Impact(hitThing);
			return;
		}
		landed = true;
		ticksToDetonation = def.projectile.explosionDelay;
		float dangerFactor = (def.projectile as ProjectilePropertiesCE).dangerFactor;
		if (dangerFactor > 0f)
		{
			base.DangerTracker.Notify_DangerRadiusAt(base.Position, def.projectile.explosionRadius + (def.projectile.applyDamageToExplosionCellsNeighbors ? 1.5f : 0f), (float)def.projectile.damageAmountBase * dangerFactor);
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher?.Faction);
		}
	}
}
