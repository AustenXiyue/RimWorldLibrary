using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

internal class Plant_Blazebulb : Plant
{
	private const int IgnitionTemp = 28;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_ObtainingPrometheum, KnowledgeAmount.Total);
	}

	public override void TickLong()
	{
		base.TickLong();
		if (base.Destroyed)
		{
			return;
		}
		float temperature = base.Position.GetTemperature(base.Map);
		if (temperature > 28f)
		{
			float num = 0.005f * Mathf.Pow(temperature - 28f, 2f);
			if (Rand.Value < num)
			{
				FireUtility.TryStartFireIn(base.Position, base.Map, 0.1f, this);
			}
		}
	}

	public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (dinfo.Def != DamageDefOf.Rotting && base.SpawnedOrAnyParentSpawned)
		{
			Thing thing = base.PositionHeld.GetThingList(base.MapHeld).FirstOrDefault((Thing x) => x.def == CE_ThingDefOf.FilthPrometheum);
			int num = Mathf.CeilToInt((float)CE_ThingDefOf.FilthPrometheum.BaseMaxHitPoints * Mathf.Clamp01(totalDamageDealt / (float)base.MaxHitPoints));
			if (thing != null)
			{
				thing.HitPoints = Mathf.Min(thing.MaxHitPoints, thing.HitPoints + num);
			}
			else
			{
				thing = ThingMaker.MakeThing(CE_ThingDefOf.FilthPrometheum);
				GenSpawn.Spawn(thing, base.PositionHeld, base.MapHeld);
				thing.HitPoints = num;
			}
		}
		base.PostApplyDamage(dinfo, totalDamageDealt);
	}
}
