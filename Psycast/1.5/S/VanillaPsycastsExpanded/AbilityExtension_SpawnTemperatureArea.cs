using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_SpawnTemperatureArea : AbilityExtension_AbilityMod
{
	public float fixedTemperature;

	public FleckDef fleckToSpawnInArea;

	public float spawnRate;

	public override void Cast(GlobalTargetInfo[] targets, Ability ability)
	{
		((AbilityExtension_AbilityMod)this).Cast(targets, ability);
		foreach (GlobalTargetInfo globalTargetInfo in targets)
		{
			ability.pawn.Map.GetComponent<MapComponent_PsycastsManager>().temperatureZones.Add(new FixedTemperatureZone
			{
				fixedTemperature = fixedTemperature,
				radius = ability.GetRadiusForPawn(),
				center = globalTargetInfo.Cell,
				expiresIn = Find.TickManager.TicksGame + ability.GetDurationForPawn(),
				fleckToSpawn = fleckToSpawnInArea,
				spawnRate = spawnRate
			});
		}
	}
}
