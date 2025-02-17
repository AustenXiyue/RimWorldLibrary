using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class DebugOutputsWorldPawns
{
	[DebugOutput("World pawns", true)]
	public static void ColonistRelativeChance()
	{
		HashSet<Pawn> hashSet = new HashSet<Pawn>(Find.WorldPawns.AllPawnsAliveOrDead);
		List<Pawn> list = new List<Pawn>();
		for (int i = 0; i < 500; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null));
			list.Add(pawn);
			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
			}
		}
		int num = list.Count((Pawn x) => PawnRelationUtility.GetMostImportantColonyRelative(x) != null);
		Log.Message("Colony relatives: " + ((float)num / 500f).ToStringPercent() + " (" + num + " of " + 500 + ")");
		foreach (Pawn item in Find.WorldPawns.AllPawnsAliveOrDead.ToList())
		{
			if (!hashSet.Contains(item))
			{
				Find.WorldPawns.RemovePawn(item);
				Find.WorldPawns.PassToWorld(item, PawnDiscardDecideMode.Discard);
			}
		}
	}

	[DebugOutput("World pawns", true)]
	public static void KidnappedPawns()
	{
		Find.FactionManager.LogKidnappedPawns();
	}

	[DebugOutput("World pawns", true)]
	public static void WorldPawnList()
	{
		Find.WorldPawns.LogWorldPawns();
	}

	[DebugOutput("World pawns", true)]
	public static void WorldPawnMothballInfo()
	{
		Find.WorldPawns.LogWorldPawnMothballPrevention();
	}

	[DebugOutput("World pawns", true)]
	public static void WorldPawnGcBreakdown()
	{
		Find.WorldPawns.gc.LogGC();
	}

	[DebugOutput("World pawns", true)]
	public static void WorldPawnDotgraph()
	{
		Find.WorldPawns.gc.LogDotgraph();
	}

	[DebugOutput("World pawns", true)]
	public static void RunWorldPawnGc()
	{
		Find.WorldPawns.gc.RunGC();
	}

	[DebugOutput("World pawns", true)]
	public static void RunWorldPawnMothball()
	{
		Find.WorldPawns.DebugRunMothballProcessing();
	}
}
