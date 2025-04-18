using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace Milira;

public static class MiliraClusterGenerator
{
	public const string MechClusterMemberTag = "MiliraClusterMember";

	public const string MechClusterMemberGoodTag = "MechClusterMemberGood";

	public const string MechClusterMemberLampTag = "MechClusterMemberLamp";

	public const string MechClusterActivatorTag = "MechClusterActivator";

	public const string MiliraClusterCombatThreatTag = "MiliraClusterCombatThreat";

	public const string MechClusterProblemCauserTag = "MechClusterProblemCauser";

	public const float MaxPoints = 10000f;

	public static readonly SimpleCurve PointsToPawnsChanceCurve = new SimpleCurve { new CurvePoint(400f, 1f) };

	public static readonly SimpleCurve PawnPointsRandomPercentOfTotalCurve = new SimpleCurve
	{
		new CurvePoint(0.2f, 0f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(0.8f, 0f)
	};

	private static readonly FloatRange SizeRandomFactorRange = new FloatRange(0.8f, 2f);

	private static readonly SimpleCurve PointsToSizeCurve = new SimpleCurve
	{
		new CurvePoint(400f, 7f),
		new CurvePoint(1000f, 10f),
		new CurvePoint(2000f, 20f),
		new CurvePoint(5000f, 25f)
	};

	private static readonly SimpleCurve ProblemCauserCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0.5f),
		new CurvePoint(800f, 0.9f),
		new CurvePoint(1200f, 0.95f)
	};

	private static readonly SimpleCurve WallsChanceCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0.35f),
		new CurvePoint(1000f, 0.5f)
	};

	private const float ActivatorCountdownChance = 0.5f;

	private const float ActivatorProximityChance = 0.5f;

	private static readonly SimpleCurve ActivatorProximitysCountCurve = new SimpleCurve
	{
		new CurvePoint(600f, 1f),
		new CurvePoint(1800f, 2f),
		new CurvePoint(3000f, 3f),
		new CurvePoint(5000f, 4f)
	};

	private static readonly SimpleCurve GoodBuildingChanceCurve = new SimpleCurve { new CurvePoint(400f, 3f) };

	private static readonly SimpleCurve GoodBuildingMaxCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 5f),
		new CurvePoint(700f, 6f),
		new CurvePoint(1000f, 7f),
		new CurvePoint(1300f, 8f),
		new CurvePoint(2000f, 9f),
		new CurvePoint(3000f, 10f),
		new CurvePoint(5000f, 12f)
	};

	private static readonly SimpleCurve LampBuildingMinCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 1f),
		new CurvePoint(1000f, 2f),
		new CurvePoint(3000f, 3f),
		new CurvePoint(5000f, 4f),
		new CurvePoint(10000f, 6f)
	};

	private static readonly SimpleCurve LampBuildingMaxCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 1f),
		new CurvePoint(1000f, 4f),
		new CurvePoint(3000f, 5f),
		new CurvePoint(5000f, 7f),
		new CurvePoint(10000f, 10f)
	};

	private static readonly SimpleCurve BulletShieldChanceCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0.1f),
		new CurvePoint(1000f, 0.4f),
		new CurvePoint(2200f, 0.5f)
	};

	private const float BulletShieldTotalPointsFactor = 0.85f;

	private static readonly SimpleCurve BulletShieldMaxCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 1f),
		new CurvePoint(3000f, 1.5f)
	};

	private const float MortarShieldTotalPointsFactor = 0.9f;

	private static readonly SimpleCurve MortarShieldChanceCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0.1f),
		new CurvePoint(1000f, 0.4f),
		new CurvePoint(2200f, 0.5f)
	};

	private static readonly SimpleCurve ResonatorChanceCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0f),
		new CurvePoint(1000f, 0f),
		new CurvePoint(3000f, 0.1f),
		new CurvePoint(5000f, 0.6f),
		new CurvePoint(10000f, 0.9f)
	};

	private static readonly SimpleCurve ResonatorMaxCountCurve = new SimpleCurve
	{
		new CurvePoint(400f, 0f),
		new CurvePoint(1000f, 0f),
		new CurvePoint(2000f, 3f),
		new CurvePoint(5000f, 6f),
		new CurvePoint(10000f, 10f)
	};

	private const float BuildingRechooseWeight = 200f;

	public static MechClusterSketch GenerateClusterSketch(float points, Map map, bool startDormant = true, bool forceNoConditionCauser = true)
	{
		if (!ModLister.CheckRoyalty("Mech cluster"))
		{
			return new MechClusterSketch(new Sketch(), new List<MechClusterSketch.Mech>(), startDormant);
		}
		points = ((!MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_ClusterFortress) ? Mathf.Min(points, 10000f) : Mathf.Min(points, 50000f));
		points *= MiliraRaceSettings.DifficultyScale(MiliraRaceSettings.currentGameDifficulty);
		float num = points;
		List<MechClusterSketch.Mech> list = null;
		if (Rand.Chance(PointsToPawnsChanceCurve.Evaluate(points)))
		{
			List<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(MilianSuitableForCluster).ToList();
			list = new List<MechClusterSketch.Mech>();
			float a = Rand.ByCurve(PawnPointsRandomPercentOfTotalCurve) * num;
			float pawnPointsLeft;
			a = (pawnPointsLeft = Mathf.Max(a, source.Min((PawnKindDef x) => x.combatPower)));
			PawnKindDef result;
			while (pawnPointsLeft > 0f && source.Where((PawnKindDef def) => def.combatPower <= pawnPointsLeft).TryRandomElement(out result))
			{
				pawnPointsLeft -= result.combatPower;
				list.Add(new MechClusterSketch.Mech(result));
			}
			num -= a - pawnPointsLeft;
		}
		Sketch buildingsSketch = SketchGen.Generate(MiliraDefOf.Milira_Cluster, new ResolveParams
		{
			points = num,
			totalPoints = points,
			mechClusterDormant = startDormant,
			sketch = new Sketch(),
			mechClusterForMap = map,
			forceNoConditionCauser = forceNoConditionCauser
		});
		if (list != null)
		{
			List<IntVec3> pawnUsedSpots = new List<IntVec3>();
			for (int i = 0; i < list.Count; i++)
			{
				MechClusterSketch.Mech pawn = list[i];
				if (!buildingsSketch.OccupiedRect.Where((IntVec3 c) => !buildingsSketch.ThingsAt(c).Any() && !pawnUsedSpots.Contains(c)).TryRandomElement(out var result2))
				{
					CellRect cellRect = buildingsSketch.OccupiedRect;
					do
					{
						cellRect = cellRect.ExpandedBy(1);
					}
					while (!cellRect.Where((IntVec3 x) => !buildingsSketch.WouldCollide(pawn.kindDef.race, x, Rot4.North) && !pawnUsedSpots.Contains(x)).TryRandomElement(out result2));
				}
				pawnUsedSpots.Add(result2);
				pawn.position = result2;
				list[i] = pawn;
			}
		}
		return new MechClusterSketch(buildingsSketch, list, startDormant);
	}

	public static bool MilianSuitableForCluster(PawnKindDef def)
	{
		if (def.defName == "Milian_Mechanoid_PawnI" || def.defName == "Milian_Mechanoid_PawnII" || def.defName == "Milian_Mechanoid_PawnIII" || def.defName == "Milian_Mechanoid_PawnIV" || def.defName == "Milian_Mechanoid_KnightI" || def.defName == "Milian_Mechanoid_KnightII" || def.defName == "Milian_Mechanoid_KnightIII" || def.defName == "Milian_Mechanoid_KnightIV" || def.defName == "Milian_Mechanoid_BishopI" || def.defName == "Milian_Mechanoid_RookI" || def.defName == "Milian_Mechanoid_RookII")
		{
			return true;
		}
		return false;
	}

	public static void ResolveSketch(ResolveParams parms)
	{
		if (!ModLister.CheckRoyalty("Mech cluster"))
		{
			return;
		}
		bool canBeDormant = !parms.mechClusterDormant.HasValue || parms.mechClusterDormant.Value;
		float num;
		if (parms.points.HasValue)
		{
			num = parms.points.Value;
		}
		else
		{
			num = 2000f;
			Log.Error("No points given for mech cluster generation. Default to " + num);
		}
		float value = (parms.totalPoints.HasValue ? parms.totalPoints.Value : num);
		IntVec2 intVec;
		if (parms.mechClusterSize.HasValue)
		{
			intVec = parms.mechClusterSize.Value;
		}
		else
		{
			int num2 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
			int num3 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
			if (parms.mechClusterForMap != null)
			{
				CellRect cellRect = LargestAreaFinder.FindLargestRect(parms.mechClusterForMap, (IntVec3 x) => !x.Impassable(parms.mechClusterForMap) && x.GetTerrain(parms.mechClusterForMap).affordances.Contains(TerrainAffordanceDefOf.Heavy), Mathf.Max(num2, num3));
				num2 = Mathf.Min(num2, cellRect.Width);
				num3 = Mathf.Min(num3, cellRect.Height);
			}
			intVec = new IntVec2(num2, num3);
		}
		Sketch sketch = new Sketch();
		if (Rand.Chance(WallsChanceCurve.Evaluate(num)))
		{
			ResolveParams parms2 = parms;
			parms2.sketch = sketch;
			parms2.mechClusterSize = intVec;
			MiliraDefOf.Milira_ClusterWalls.Resolve(parms2);
		}
		List<ThingDef> buildingDefsForCluster = GetBuildingDefsForCluster(num, intVec, canBeDormant, value, parms.forceNoConditionCauser.HasValue && parms.forceNoConditionCauser.Value);
		AddBuildingsToSketch(sketch, intVec, buildingDefsForCluster);
		parms.sketch.MergeAt(sketch, default(IntVec3), Sketch.SpawnPosType.OccupiedCenter);
	}

	private static List<ThingDef> GetBuildingDefsForCluster(float points, IntVec2 size, bool canBeDormant, float? totalPoints, bool forceNoConditionCauser)
	{
		List<ThingDef> list = new List<ThingDef>();
		List<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.building != null && def.building.buildingTags != null && def.building.buildingTags.Contains("MechClusterMember") && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
		List<ThingDef> source2 = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.building != null && def.building.buildingTags != null && def.building.buildingTags.Contains("MiliraClusterMember") && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
		if (!forceNoConditionCauser)
		{
			int num = GenMath.RoundRandom(ProblemCauserCountCurve.Evaluate(points));
			for (int i = 0; i < num; i++)
			{
				if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterProblemCauser")).TryRandomElementByWeight((ThingDef t) => t.generateCommonality, out var result))
				{
					break;
				}
				list.Add(result);
			}
		}
		if (canBeDormant)
		{
			if (Rand.Chance(0.5f))
			{
				list.Add(MiliraDefOf.Milira_ActivatorCountdown);
			}
			if (Rand.Chance(0.5f))
			{
				int num2 = GenMath.RoundRandom(ActivatorProximitysCountCurve.Evaluate(points));
				for (int j = 0; j < num2; j++)
				{
					list.Add(MiliraDefOf.Milira_ActivatorProximity);
				}
			}
		}
		if (Rand.Chance(GoodBuildingChanceCurve.Evaluate(points)))
		{
			int num3 = Rand.RangeInclusive(0, GenMath.RoundRandom(GoodBuildingMaxCountCurve.Evaluate(points)));
			for (int k = 0; k < num3; k++)
			{
				if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MiliraClusterMemberGood")).TryRandomElement(out var result2))
				{
					break;
				}
				list.Add(result2);
			}
		}
		int num4 = Rand.RangeInclusive(Mathf.FloorToInt(LampBuildingMinCountCurve.Evaluate(points)), Mathf.CeilToInt(LampBuildingMaxCountCurve.Evaluate(points)));
		for (int l = 0; l < num4; l++)
		{
			if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MiliraClusterMemberLamp")).TryRandomElement(out var result3))
			{
				break;
			}
			list.Add(result3);
		}
		if (Rand.Chance(ResonatorChanceCurve.Evaluate(points)) && MiliraRaceSettings.MiliraRace_ModSetting_MilianDifficulty_ClusterResonator)
		{
			int minInclusive = Rand.RangeInclusive(0, GenMath.RoundRandom(ResonatorMaxCountCurve.Evaluate(points)));
			minInclusive = Rand.RangeInclusive(minInclusive, GenMath.RoundRandom(ResonatorMaxCountCurve.Evaluate(points)) + 1);
			for (int m = 0; m < minInclusive; m++)
			{
				if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MiliraClusterMemberResonator")).TryRandomElement(out var result4))
				{
					break;
				}
				list.Add(result4);
			}
		}
		if (Rand.Chance(BulletShieldChanceCurve.Evaluate(points)))
		{
			points *= 0.85f;
			int num5 = Rand.RangeInclusive(0, GenMath.RoundRandom(BulletShieldMaxCountCurve.Evaluate(points)));
			for (int n = 0; n < num5; n++)
			{
				list.Add(MiliraDefOf.Milira_ShieldGeneratorBullet);
			}
		}
		if (Rand.Chance(MortarShieldChanceCurve.Evaluate(points)))
		{
			points *= 0.9f;
			list.Add(MiliraDefOf.Milira_ShieldGeneratorMortar);
		}
		float pointsLeft = points;
		ThingDef thingDef = source2.Where((ThingDef x) => x.building.buildingTags.Contains("MiliraClusterCombatThreat")).MinBy((ThingDef x) => x.building.combatPower);
		if (thingDef == null)
		{
			Log.Message("thingDef == null");
		}
		ThingDef result5;
		for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower); pointsLeft > 0f && source2.Where((ThingDef x) => x.building.combatPower <= pointsLeft && x.building.buildingTags.Contains("MiliraClusterCombatThreat")).TryRandomElement(out result5); pointsLeft -= result5.building.combatPower)
		{
			list.Add(result5);
		}
		return list;
	}

	private static bool TryRandomBuildingWithTag(string tag, List<ThingDef> allowedBuildings, List<ThingDef> generatedBuildings, IntVec2 size, out ThingDef result)
	{
		return allowedBuildings.Where((ThingDef x) => x.building.buildingTags.Contains(tag)).TryRandomElement(out result);
	}

	private static void AddBuildingsToSketch(Sketch sketch, IntVec2 size, List<ThingDef> buildings)
	{
		List<CellRect> edgeWallRects = new List<CellRect>
		{
			new CellRect(0, 0, size.x, 1),
			new CellRect(0, 0, 1, size.z),
			new CellRect(size.x - 1, 0, 1, size.z),
			new CellRect(0, size.z - 1, size.x, 1)
		};
		foreach (ThingDef item in buildings.OrderBy((ThingDef x) => x.building.IsTurret && !x.building.IsMortar))
		{
			bool flag = item.building.IsTurret && !item.building.IsMortar;
			if (!TryFindRandomPlaceFor(item, sketch, size, out var pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects) && !TryFindRandomPlaceFor(item, sketch, size + new IntVec2(6, 6), out pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects))
			{
				continue;
			}
			sketch.AddThing(item, pos, Rot4.North, GenStuff.RandomStuffByCommonalityFor(item), 1, null, null);
			if (item != MiliraDefOf.MiliraTurret_Plasma && item != MiliraDefOf.Milira_DropBeaconCluster)
			{
				continue;
			}
			if (pos.x < size.x / 2)
			{
				if (pos.z < size.z / 2)
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z - 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				}
				else
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z + 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				}
			}
			else if (pos.z < size.z / 2)
			{
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z - 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
			}
			else
			{
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z + 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
				sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, MiliraDefOf.Milira_SunPlateSteel, 1, null, null, wipeIfCollides: false);
			}
		}
	}

	private static bool TryFindRandomPlaceFor(ThingDef thingDef, Sketch sketch, IntVec2 size, out IntVec3 pos, bool lowerLeftQuarterOnly, bool avoidCenter, bool requireLOSToEdge, bool avoidEdge, List<CellRect> edgeWallRects)
	{
		for (int i = 0; i < 200; i++)
		{
			CellRect cellRect = new CellRect(0, 0, size.x, size.z);
			if (lowerLeftQuarterOnly)
			{
				cellRect = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Width / 2, cellRect.Height / 2);
			}
			IntVec3 randomCell = cellRect.RandomCell;
			if (avoidCenter)
			{
				CellRect cellRect2 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, size.x / 2, size.z / 2);
				for (int j = 0; j < 5; j++)
				{
					if (!cellRect2.Contains(randomCell))
					{
						break;
					}
					randomCell = cellRect.RandomCell;
				}
			}
			if (avoidEdge)
			{
				CellRect cellRect3 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, Mathf.RoundToInt((float)size.x * 0.75f), Mathf.RoundToInt((float)size.z * 0.75f));
				for (int k = 0; k < 5; k++)
				{
					if (cellRect3.Contains(randomCell))
					{
						break;
					}
					randomCell = cellRect.RandomCell;
				}
			}
			if (requireLOSToEdge)
			{
				IntVec3 end = randomCell;
				end.x += size.x + 1;
				IntVec3 end2 = randomCell;
				end2.x -= size.x + 1;
				IntVec3 end3 = randomCell;
				end3.z -= size.z + 1;
				IntVec3 end4 = randomCell;
				end4.z += size.z + 1;
				if (!sketch.LineOfSight(randomCell, end) && !sketch.LineOfSight(randomCell, end2) && !sketch.LineOfSight(randomCell, end3) && !sketch.LineOfSight(randomCell, end4))
				{
					continue;
				}
			}
			if (thingDef.building.minDistanceToSameTypeOfBuilding > 0)
			{
				bool flag = false;
				for (int l = 0; l < sketch.Things.Count; l++)
				{
					if (sketch.Things[l].def == thingDef && sketch.Things[l].pos.InHorDistOf(randomCell, thingDef.building.minDistanceToSameTypeOfBuilding))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			bool flag2 = false;
			CellRect cellRect4 = GenAdj.OccupiedRect(randomCell, Rot4.North, thingDef.Size);
			for (int m = 0; m < 4; m++)
			{
				if (cellRect4.Overlaps(edgeWallRects[m]))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2 && !sketch.WouldCollide(thingDef, randomCell, Rot4.North))
			{
				pos = randomCell;
				return true;
			}
		}
		pos = IntVec3.Invalid;
		return false;
	}
}
