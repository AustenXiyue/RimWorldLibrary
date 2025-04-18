using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class MapGenerator
{
	public static Map mapBeingGenerated;

	private static Dictionary<string, object> data = new Dictionary<string, object>();

	private static IntVec3 playerStartSpotInt = IntVec3.Invalid;

	public static List<IntVec3> rootsToUnfog = new List<IntVec3>();

	private static List<GenStepWithParams> tmpGenSteps = new List<GenStepWithParams>();

	public const string ElevationName = "Elevation";

	public const string FertilityName = "Fertility";

	public const string CavesName = "Caves";

	public const string RectOfInterestName = "RectOfInterest";

	public const string UsedRectsName = "UsedRects";

	public const string RectOfInterestTurretsGenStepsCount = "RectOfInterestTurretsGenStepsCount";

	public static MapGenFloatGrid Elevation => FloatGridNamed("Elevation");

	public static MapGenFloatGrid Fertility => FloatGridNamed("Fertility");

	public static MapGenFloatGrid Caves => FloatGridNamed("Caves");

	public static IntVec3 PlayerStartSpot
	{
		get
		{
			if (!playerStartSpotInt.IsValid)
			{
				Log.Error("Accessing player start spot before setting it.");
				return IntVec3.Zero;
			}
			return playerStartSpotInt;
		}
		set
		{
			playerStartSpotInt = value;
		}
	}

	public static Map GenerateMap(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null, bool isPocketMap = false)
	{
		ProgramState programState = Current.ProgramState;
		Current.ProgramState = ProgramState.MapInitializing;
		playerStartSpotInt = IntVec3.Invalid;
		rootsToUnfog.Clear();
		data.Clear();
		mapBeingGenerated = null;
		DeepProfiler.Start("InitNewGeneratedMap");
		Rand.PushState();
		int seed = Gen.HashCombineInt(Find.World.info.Seed, parent?.Tile ?? 0);
		if (isPocketMap)
		{
			seed = Rand.Int;
		}
		Rand.Seed = seed;
		try
		{
			if (parent != null && parent.HasMap)
			{
				Log.Error(string.Concat("Tried to generate a new map and set ", parent, " as its parent, but this world object already has a map. One world object can't have more than 1 map."));
				parent = null;
			}
			DeepProfiler.Start("Set up map");
			Map map = new Map();
			map.uniqueID = Find.UniqueIDsManager.GetNextMapID();
			map.generationTick = GenTicks.TicksGame;
			mapBeingGenerated = map;
			map.info.Size = mapSize;
			map.info.parent = parent;
			if (mapGenerator == null)
			{
				Log.Error("Attempted to generate map without generator; falling back on encounter map");
				mapGenerator = MapGeneratorDefOf.Encounter;
			}
			map.generatorDef = mapGenerator;
			map.info.disableSunShadows = mapGenerator.disableShadows;
			if (isPocketMap)
			{
				map.info.isPocketMap = true;
				map.pocketTileInfo = new Tile
				{
					biome = mapGenerator.pocketMapProperties.biome
				};
			}
			map.ConstructComponents();
			DeepProfiler.End();
			Current.Game.AddMap(map);
			if (mapGenerator.isUnderground)
			{
				foreach (IntVec3 allCell in map.AllCells)
				{
					map.roofGrid.SetRoof(allCell, mapGenerator.roofDef ?? RoofDefOf.RoofRockThick);
				}
			}
			extraInitBeforeContentGen?.Invoke(map);
			IEnumerable<GenStepWithParams> enumerable = from g in mapGenerator.genSteps
				where !Find.Scenario.parts.Any((ScenPart p) => typeof(ScenPart_DisableMapGen).IsAssignableFrom(p.def.scenPartClass) && p.def.genStep == g)
				select g into x
				select new GenStepWithParams(x, default(GenStepParams));
			if (extraGenStepDefs != null)
			{
				enumerable = enumerable.Concat(extraGenStepDefs);
			}
			map.areaManager.AddStartingAreas();
			map.weatherDecider.StartInitialWeather();
			DeepProfiler.Start("Generate contents into map");
			GenerateContentsIntoMap(enumerable, map, seed);
			DeepProfiler.End();
			Find.Scenario.PostMapGenerate(map);
			DeepProfiler.Start("Finalize map init");
			map.FinalizeInit();
			DeepProfiler.End();
			DeepProfiler.Start("MapComponent.MapGenerated()");
			MapComponentUtility.MapGenerated(map);
			DeepProfiler.End();
			parent?.PostMapGenerate();
			return map;
		}
		finally
		{
			DeepProfiler.End();
			mapBeingGenerated = null;
			Current.ProgramState = programState;
			Rand.PopState();
		}
	}

	public static void GenerateContentsIntoMap(IEnumerable<GenStepWithParams> genStepDefs, Map map, int seed)
	{
		data.Clear();
		Rand.PushState();
		try
		{
			Rand.Seed = seed;
			RockNoises.Init(map);
			tmpGenSteps.Clear();
			tmpGenSteps.AddRange(from x in genStepDefs
				orderby x.def.order, x.def.index
				select x);
			for (int i = 0; i < tmpGenSteps.Count; i++)
			{
				DeepProfiler.Start("GenStep - " + tmpGenSteps[i].def);
				try
				{
					Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(tmpGenSteps, i));
					tmpGenSteps[i].def.genStep.Generate(map, tmpGenSteps[i].parms);
				}
				catch (Exception ex)
				{
					Log.Error("Error in GenStep: " + ex);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}
		finally
		{
			Rand.PopState();
			RockNoises.Reset();
			data.Clear();
		}
	}

	public static T GetVar<T>(string name)
	{
		if (data.TryGetValue(name, out var value))
		{
			return (T)value;
		}
		return default(T);
	}

	public static bool TryGetVar<T>(string name, out T var)
	{
		if (data.TryGetValue(name, out var value))
		{
			var = (T)value;
			return true;
		}
		var = default(T);
		return false;
	}

	public static void SetVar<T>(string name, T var)
	{
		data[name] = var;
	}

	public static MapGenFloatGrid FloatGridNamed(string name)
	{
		MapGenFloatGrid var = GetVar<MapGenFloatGrid>(name);
		if (var != null)
		{
			return var;
		}
		MapGenFloatGrid mapGenFloatGrid = new MapGenFloatGrid(mapBeingGenerated);
		SetVar(name, mapGenFloatGrid);
		return mapGenFloatGrid;
	}

	private static int GetSeedPart(List<GenStepWithParams> genSteps, int index)
	{
		int seedPart = genSteps[index].def.genStep.SeedPart;
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			if (tmpGenSteps[i].def.genStep.SeedPart == seedPart)
			{
				num++;
			}
		}
		return seedPart + num;
	}
}
