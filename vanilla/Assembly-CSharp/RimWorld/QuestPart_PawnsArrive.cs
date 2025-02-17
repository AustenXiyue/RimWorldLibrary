using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_PawnsArrive : QuestPart
{
	public string inSignal;

	public MapParent mapParent;

	public List<Pawn> pawns = new List<Pawn>();

	public PawnsArrivalModeDef arrivalMode;

	public IntVec3 spawnNear = IntVec3.Invalid;

	public bool joinPlayer;

	public bool addPawnsToLookTargets = true;

	public string customLetterText;

	public string customLetterLabel;

	public LetterDef customLetterDef;

	public bool sendStandardLetter = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
			if (!addPawnsToLookTargets)
			{
				yield break;
			}
			foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns))
			{
				yield return questLookTarget2;
			}
		}
	}

	public override bool IncreasesPopulation => PawnsArriveQuestPartUtility.IncreasesPopulation(pawns, joinPlayer, makePrisoners: false);

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		pawns.RemoveAll((Pawn x) => x.Destroyed);
		if (mapParent == null || !mapParent.HasMap || !pawns.Any())
		{
			return;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (joinPlayer && pawns[i].Faction != Faction.OfPlayer)
			{
				pawns[i].SetFaction(Faction.OfPlayer);
			}
		}
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = mapParent.Map;
		incidentParms.spawnCenter = spawnNear;
		PawnsArrivalModeDef obj = arrivalMode ?? PawnsArrivalModeDefOf.EdgeWalkIn;
		obj.Worker.TryResolveRaidSpawnCenter(incidentParms);
		obj.Worker.Arrive(pawns, incidentParms);
		if (!sendStandardLetter)
		{
			return;
		}
		TaggedString letterLabel;
		TaggedString letterText;
		if (joinPlayer && pawns.Count == 1 && pawns[0].RaceProps.Humanlike)
		{
			letterText = "LetterRefugeeJoins".Translate(pawns[0].Named("PAWN"));
			letterLabel = "LetterLabelRefugeeJoins".Translate(pawns[0].Named("PAWN"));
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref letterLabel, pawns[0]);
		}
		else
		{
			if (joinPlayer)
			{
				letterText = "LetterPawnsArriveAndJoin".Translate(GenLabel.ThingsLabel(pawns.Cast<Thing>()));
				letterLabel = "LetterLabelPawnsArriveAndJoin".Translate();
			}
			else
			{
				letterText = "LetterPawnsArrive".Translate(GenLabel.ThingsLabel(pawns.Cast<Thing>()));
				letterLabel = "LetterLabelPawnsArrive".Translate();
			}
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
		}
		letterLabel = (customLetterLabel.NullOrEmpty() ? letterLabel : customLetterLabel.Formatted(letterLabel.Named("BASELABEL")));
		letterText = (customLetterText.NullOrEmpty() ? letterText : customLetterText.Formatted(letterText.Named("BASETEXT")));
		Find.LetterStack.ReceiveLetter(letterLabel, letterText, customLetterDef ?? LetterDefOf.PositiveEvent, pawns, null, quest);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref spawnNear, "spawnNear");
		Scribe_Values.Look(ref joinPlayer, "joinPlayer", defaultValue: false);
		Scribe_Values.Look(ref addPawnsToLookTargets, "addPawnsToLookTargets", defaultValue: true);
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
		Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null));
			pawn.relations.everSeenByPlayer = true;
			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
			pawns.Add(pawn);
			arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			mapParent = Find.RandomPlayerHomeMap.Parent;
			joinPlayer = true;
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		return pawns.Contains(p);
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
