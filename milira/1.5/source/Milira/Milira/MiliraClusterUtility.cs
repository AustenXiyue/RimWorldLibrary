using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Milira;

public static class MiliraClusterUtility
{
	private const int CloseToColonyRadius = 40;

	private const int MinDistanceToColony = 5;

	private const float RetrySpawnEntityRadius = 12f;

	private const float MaxClusterPositionScore = 100f;

	private const float InitiationChance = 0.6f;

	private static readonly FloatRange InitiationDelay = new FloatRange(0.1f, 15f);

	private static readonly FloatRange MechAssemblerInitialDelayDays = new FloatRange(0.5f, 1.5f);

	public const string DefeatedSignal = "MechClusterDefeated";

	public static IntVec3 FindClusterPosition(Map map, MechClusterSketch sketch, int maxTries = 100, float spawnCloseToColonyChance = 0f)
	{
		IntVec3 result = IntVec3.Invalid;
		float num = float.MinValue;
		if (Rand.Chance(spawnCloseToColonyChance))
		{
			for (int i = 0; i < 20; i++)
			{
				if (!DropCellFinder.TryFindRaidDropCenterClose(out var spot, map, canRoofPunch: true, allowIndoors: true, closeWalk: false, 40))
				{
					break;
				}
				float clusterPositionScore = GetClusterPositionScore(spot, map, sketch);
				if (clusterPositionScore >= 100f || Mathf.Approximately(clusterPositionScore, 100f))
				{
					return spot;
				}
				if (clusterPositionScore > num)
				{
					result = spot;
					num = clusterPositionScore;
				}
			}
		}
		for (int j = 0; j < maxTries; j++)
		{
			IntVec3 intVec = CellFinderLoose.RandomCellWith((IntVec3 x) => x.Standable(map), map);
			if (!intVec.IsValid)
			{
				continue;
			}
			IntVec3 intVec2 = RCellFinder.FindSiegePositionFrom(intVec, map, allowRoofed: false, errorOnFail: false);
			if (intVec2.IsValid)
			{
				float clusterPositionScore2 = GetClusterPositionScore(intVec2, map, sketch);
				if (clusterPositionScore2 >= 100f || Mathf.Approximately(clusterPositionScore2, 100f))
				{
					return intVec2;
				}
				if (clusterPositionScore2 > num)
				{
					result = intVec;
					num = clusterPositionScore2;
				}
			}
		}
		if (!result.IsValid)
		{
			return CellFinder.RandomCell(map);
		}
		return result;
	}

	public static float GetClusterPositionScore(IntVec3 center, Map map, MechClusterSketch sketch)
	{
		if (sketch.buildingsSketch.AnyThingOutOfBounds(map, center))
		{
			return -100f;
		}
		if (sketch.pawns != null)
		{
			for (int i = 0; i < sketch.pawns.Count; i++)
			{
				if (!(sketch.pawns[i].position + center).InBounds(map))
				{
					return -100f;
				}
			}
		}
		List<Building> colonyBuildings = map.listerBuildings.allBuildingsColonist;
		List<Pawn> colonists = map.mapPawns.FreeColonistsAndPrisonersSpawned;
		int num = 0;
		int fogged = 0;
		int roofed = 0;
		int indoors = 0;
		bool tooCloseToColony = false;
		List<SketchEntity> entities = sketch.buildingsSketch.Entities;
		for (int j = 0; j < entities.Count; j++)
		{
			if (entities[j].IsSpawningBlocked(center, map))
			{
				num++;
			}
			if (!CheckCell(entities[j].pos + center))
			{
				return -100f;
			}
		}
		if (sketch.pawns != null)
		{
			for (int k = 0; k < sketch.pawns.Count; k++)
			{
				if (!(sketch.pawns[k].position + center).Standable(map))
				{
					num++;
				}
				if (!CheckCell(sketch.pawns[k].position + center))
				{
					return -100f;
				}
			}
		}
		int num2 = sketch.buildingsSketch.Entities.Count + ((sketch.pawns != null) ? sketch.pawns.Count : 0);
		float num3 = (float)num / (float)num2;
		float num4 = (float)fogged / (float)num2;
		float a = (float)roofed / (float)num2;
		float b = (float)indoors / (float)num2;
		return 100f * (1f - num3) * (1f - Mathf.Max(a, b)) * (1f - num4) * (tooCloseToColony ? 0.5f : 1f);
		bool CheckCell(IntVec3 x)
		{
			if (x.Fogged(map))
			{
				fogged++;
			}
			if (x.Roofed(map))
			{
				roofed++;
			}
			if (x.GetRoom(map) != null && !x.GetRoom(map).PsychologicallyOutdoors)
			{
				indoors++;
			}
			foreach (Thing thing in x.GetThingList(map))
			{
				if (thing.def.preventSkyfallersLandingOn)
				{
					return false;
				}
			}
			if (!tooCloseToColony)
			{
				for (int l = 0; l < colonyBuildings.Count; l++)
				{
					if (x.InHorDistOf(colonyBuildings[l].Position, 5f))
					{
						tooCloseToColony = true;
						break;
					}
				}
				if (!tooCloseToColony)
				{
					for (int m = 0; m < colonists.Count; m++)
					{
						if (x.InHorDistOf(colonists[m].Position, 5f))
						{
							tooCloseToColony = true;
							break;
						}
					}
				}
			}
			return true;
		}
	}

	[Obsolete]
	public static bool CanSpawnClusterAt(MechClusterSketch sketch, IntVec3 center, Map map, bool desperate = false)
	{
		return false;
	}

	[Obsolete]
	public static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator, int maxTries = 100, float spawnCloseToColonyChance = 0f)
	{
		return IntVec3.Invalid;
	}

	[Obsolete]
	private static bool TryFindRaidDropCenterClose(out IntVec3 result, Map map, Predicate<IntVec3> validator, int maxTries = 100)
	{
		result = IntVec3.Invalid;
		return false;
	}

	[Obsolete]
	public static IntVec3 TryFindMechClusterPosInRect(CellRect rect, Map map, MechClusterSketch sketch)
	{
		return IntVec3.Invalid;
	}

	public static List<Thing> SpawnCluster(IntVec3 center, Map map, MechClusterSketch sketch, bool dropInPods = true, bool canAssaultColony = false, string questTag = "MilianCluster")
	{
		List<Thing> spawnedThings = new List<Thing>();
		Faction miliraFaction = Find.FactionManager.FirstFactionOfDef(MiliraDefOf.Milira_Faction);
		if (miliraFaction == null)
		{
			Log.Warning("Could not spawn milira cluster, no world milira faction found.");
			return spawnedThings;
		}
		foreach (IntVec3 item in sketch.buildingsSketch.OccupiedRect)
		{
			IntVec3 c = item + center;
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			Thing thing = null;
			foreach (Thing item2 in thingList)
			{
				if (item2.def.IsBlueprint)
				{
					thing = item2;
					break;
				}
			}
			thing?.Destroy();
		}
		Sketch.SpawnMode spawnMode = ((!dropInPods) ? Sketch.SpawnMode.Normal : Sketch.SpawnMode.TransportPod);
		sketch.buildingsSketch.Spawn(map, center, miliraFaction, Sketch.SpawnPosType.Unchanged, spawnMode, wipeIfCollides: false, clearEdificeWhereFloor: false, spawnedThings, sketch.startDormant, buildRoofsInstantly: false, CanSpawnThing, delegate(IntVec3 spot, SketchEntity entity)
		{
			if (entity is SketchThing sketchThing && sketchThing.def != ThingDefOf.Wall && sketchThing.def != ThingDefOf.Barricade)
			{
				entity.SpawnNear(spot, map, 12f, miliraFaction, spawnMode, wipeIfCollides: false, spawnedThings, sketch.startDormant, CanSpawnThing);
			}
		});
		float defendRadius = Mathf.Sqrt(sketch.buildingsSketch.OccupiedSize.x * sketch.buildingsSketch.OccupiedSize.x + sketch.buildingsSketch.OccupiedSize.z * sketch.buildingsSketch.OccupiedSize.z) / 2f + 6f;
		LordJob_MechanoidDefendBase lordJob_MechanoidDefendBase = null;
		lordJob_MechanoidDefendBase = ((!sketch.startDormant) ? ((LordJob_MechanoidDefendBase)new LordJob_MechanoidsDefend(spawnedThings, miliraFaction, defendRadius, center, canAssaultColony, isMechCluster: true)) : ((LordJob_MechanoidDefendBase)new LordJob_SleepThenMechanoidsDefend(spawnedThings, miliraFaction, defendRadius, center, canAssaultColony, isMechCluster: true)));
		Lord lord = LordMaker.MakeNewLord(miliraFaction, lordJob_MechanoidDefendBase, map);
		QuestUtility.AddQuestTag(lord, questTag);
		float randomInRange = InitiationDelay.RandomInRange;
		int num = (int)(MechAssemblerInitialDelayDays.RandomInRange * 60000f);
		for (int i = 0; i < spawnedThings.Count; i++)
		{
			Thing thing2 = spawnedThings[i];
			thing2.TryGetComp<CompSpawnerPawn>()?.CalculateNextPawnSpawnTick(num);
			if (thing2.TryGetComp<CompProjectileInterceptor>() != null)
			{
				lordJob_MechanoidDefendBase.AddThingToNotifyOnDefeat(thing2);
			}
			if (thing2 is Building b && IsBuildingThreat(b))
			{
				lord.AddBuilding(b);
			}
			thing2.SetFaction(miliraFaction);
		}
		if (!sketch.pawns.NullOrEmpty())
		{
			foreach (MechClusterSketch.Mech pawn2 in sketch.pawns)
			{
				IntVec3 result = pawn2.position + center;
				if (!result.Standable(map) && !CellFinder.TryFindRandomCellNear(result, map, 12, (IntVec3 x) => x.Standable(map), out result))
				{
					continue;
				}
				Pawn pawn = PawnGenerator.GeneratePawn(pawn2.kindDef, miliraFaction);
				CompCanBeDormant compCanBeDormant = pawn.TryGetComp<CompCanBeDormant>();
				if (compCanBeDormant != null)
				{
					if (sketch.startDormant)
					{
						compCanBeDormant.ToSleep();
					}
					else
					{
						compCanBeDormant.WakeUp();
					}
				}
				lord.AddPawn(pawn);
				spawnedThings.Add(pawn);
				if (dropInPods)
				{
					ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
					activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
					activeDropPodInfo.openDelay = 0;
					activeDropPodInfo.leaveSlag = false;
					activeDropPodInfo.despawnPodBeforeSpawningThing = true;
					activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
					DropPodUtility.MakeDropPodAt(result, map, activeDropPodInfo, miliraFaction);
				}
				else
				{
					GenSpawn.Spawn(pawn, result, map);
				}
			}
		}
		foreach (Thing item3 in spawnedThings)
		{
			if (!sketch.startDormant)
			{
				item3.TryGetComp<CompWakeUpDormant>()?.Activate(null, sendSignal: true, silent: true);
			}
		}
		return spawnedThings;
		bool CanSpawnThing(SketchEntity ent, IntVec3 cell)
		{
			foreach (IntVec3 item4 in ent.OccupiedRect.MovedBy(cell))
			{
				if (item4.InBounds(map))
				{
					foreach (Thing thing3 in item4.GetThingList(map))
					{
						if (thing3.def.preventSkyfallersLandingOn)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	private static bool IsBuildingThreat(Thing b)
	{
		CompPawnSpawnOnWakeup compPawnSpawnOnWakeup = b.TryGetComp<CompPawnSpawnOnWakeup>();
		if (compPawnSpawnOnWakeup != null && compPawnSpawnOnWakeup.CanSpawn)
		{
			return true;
		}
		CompSpawnerPawn compSpawnerPawn = b.TryGetComp<CompSpawnerPawn>();
		if (compSpawnerPawn != null && compSpawnerPawn.pawnsLeftToSpawn != 0)
		{
			return true;
		}
		if (!b.def.building.IsTurret)
		{
			return b.TryGetComp<CompCauseGameCondition>() != null;
		}
		return true;
	}

	public static bool AnyThreatBuilding(List<Thing> things)
	{
		for (int i = 0; i < things.Count; i++)
		{
			Thing thing = things[i];
			if (thing is Building && !thing.Destroyed && IsBuildingThreat(thing))
			{
				return true;
			}
		}
		return false;
	}
}
