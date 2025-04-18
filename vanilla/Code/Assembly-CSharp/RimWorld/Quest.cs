using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Quest : IExposable, ILoadReferenceable, ISignalReceiver
{
	public int id;

	private List<QuestPart> parts = new List<QuestPart>();

	public string name;

	public TaggedString description;

	public float points;

	public int challengeRating = -1;

	public List<string> tags = new List<string>();

	public string lastSlateStateDebug;

	public QuestScriptDef root;

	public bool hidden;

	public Quest parent;

	public int appearanceTick = -1;

	public int acceptanceTick = -1;

	public bool initiallyAccepted;

	public bool dismissed;

	public bool hiddenInUI;

	public int ticksUntilAcceptanceExpiry = -1;

	private Pawn accepterPawn;

	private string accepterPawnLabel;

	public bool charity;

	public List<string> signalsReceivedDebug;

	private bool ended;

	private QuestEndOutcome endOutcome;

	private bool cleanedUp;

	public int cleanupTick = -1;

	public const int MaxSignalsReceivedDebugCount = 25;

	private const int RemoveAllQuestPartsAfterTicksSinceCleanup = 1800000;

	public List<QuestPart> PartsListForReading => parts;

	public int TicksSinceAppeared => Find.TickManager.TicksGame - appearanceTick;

	public int TicksSinceAccepted
	{
		get
		{
			if (acceptanceTick >= 0)
			{
				return Find.TickManager.TicksGame - acceptanceTick;
			}
			return -1;
		}
	}

	public int TicksSinceCleanup
	{
		get
		{
			if (!cleanedUp)
			{
				return -1;
			}
			return Find.TickManager.TicksGame - cleanupTick;
		}
	}

	public string AccepterPawnLabelCap
	{
		get
		{
			if (accepterPawn == null)
			{
				return accepterPawnLabel;
			}
			return accepterPawn.LabelCap;
		}
	}

	public string AddedSignal => "Quest" + id + ".Added";

	public string InitiateSignal => "Quest" + id + ".Initiate";

	public bool EverAccepted
	{
		get
		{
			if (!initiallyAccepted)
			{
				return acceptanceTick >= 0;
			}
			return true;
		}
	}

	public Pawn AccepterPawn => accepterPawn;

	public bool RequiresAccepter
	{
		get
		{
			for (int i = 0; i < parts.Count; i++)
			{
				if (parts[i].RequiresAccepter)
				{
					return true;
				}
			}
			return false;
		}
	}

	public QuestState State
	{
		get
		{
			if (ticksUntilAcceptanceExpiry == 0)
			{
				return QuestState.EndedOfferExpired;
			}
			if (ended)
			{
				if (endOutcome == QuestEndOutcome.Success)
				{
					return QuestState.EndedSuccess;
				}
				if (endOutcome == QuestEndOutcome.Fail)
				{
					return QuestState.EndedFailed;
				}
				if (endOutcome == QuestEndOutcome.InvalidPreAcceptance)
				{
					return QuestState.EndedInvalid;
				}
				return QuestState.EndedUnknownOutcome;
			}
			if (acceptanceTick < 0)
			{
				return QuestState.NotYetAccepted;
			}
			return QuestState.Ongoing;
		}
	}

	public IEnumerable<GlobalTargetInfo> QuestLookTargets => parts.SelectMany((QuestPart x) => x.QuestLookTargets).Distinct();

	public IEnumerable<GlobalTargetInfo> QuestSelectTargets => parts.SelectMany((QuestPart x) => x.QuestSelectTargets).Distinct();

	public IEnumerable<Faction> InvolvedFactions => parts.SelectMany((QuestPart x) => x.InvolvedFactions).Distinct();

	public IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			if (parent != null)
			{
				yield return new Dialog_InfoCard.Hyperlink(parent);
			}
			foreach (Quest item in from q in this.GetSubquests(null)
				orderby q.Historical
				select q)
			{
				yield return new Dialog_InfoCard.Hyperlink(item);
			}
			foreach (Dialog_InfoCard.Hyperlink item2 in parts.SelectMany((QuestPart x) => x.Hyperlinks).Distinct())
			{
				yield return item2;
			}
		}
	}

	public bool Historical
	{
		get
		{
			if (State != 0)
			{
				return State != QuestState.Ongoing;
			}
			return false;
		}
	}

	public bool IncreasesPopulation
	{
		get
		{
			for (int i = 0; i < parts.Count; i++)
			{
				if (parts[i].IncreasesPopulation)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static Quest MakeRaw()
	{
		return new Quest
		{
			id = Find.UniqueIDsManager.GetNextQuestID(),
			appearanceTick = Find.TickManager.TicksGame,
			name = "Unnamed quest"
		};
	}

	public void QuestTick()
	{
		if (Historical)
		{
			if (!cleanedUp)
			{
				CleanupQuestParts();
			}
			if (TicksSinceCleanup >= 1800000)
			{
				parts.Clear();
			}
			return;
		}
		if (ticksUntilAcceptanceExpiry > 0 && State == QuestState.NotYetAccepted)
		{
			ticksUntilAcceptanceExpiry--;
			if (ticksUntilAcceptanceExpiry == 0 && !cleanedUp)
			{
				CleanupQuestParts();
			}
		}
		if (Historical)
		{
			return;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i] is QuestPartActivable { State: QuestPartState.Enabled } questPartActivable)
			{
				try
				{
					questPartActivable.QuestPartTick();
				}
				catch (Exception ex)
				{
					Log.Error("Exception ticking QuestPart: " + ex);
				}
				if (Historical)
				{
					break;
				}
			}
		}
	}

	public void AddPart(QuestPart part)
	{
		if (parts.Contains(part))
		{
			Log.Error("Tried to add the same QuestPart twice: " + part.ToStringSafe() + ", quest=" + this.ToStringSafe());
			return;
		}
		part.quest = this;
		parts.Add(part);
	}

	public T AddPart<T>() where T : QuestPart
	{
		T val = Activator.CreateInstance<T>();
		AddPart(val);
		return val;
	}

	public T GetFirstOrAddPart<T>() where T : QuestPart
	{
		return GetFirstPartOfType<T>() ?? AddPart<T>();
	}

	public T GetFirstPartOfType<T>() where T : QuestPart
	{
		foreach (QuestPart part in parts)
		{
			if (part is T result)
			{
				return result;
			}
		}
		return null;
	}

	public bool TryGetFirstPartOfType<T>(out T part) where T : QuestPart
	{
		part = GetFirstPartOfType<T>();
		return part != null;
	}

	public void RemovePart(QuestPart part)
	{
		if (!parts.Contains(part))
		{
			Log.Error("Tried to remove QuestPart which doesn't exist: " + part.ToStringSafe() + ", quest=" + this.ToStringSafe());
			return;
		}
		part.quest = null;
		parts.Remove(part);
	}

	public void Accept(Pawn by)
	{
		if (State == QuestState.NotYetAccepted)
		{
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].PreQuestAccept();
			}
			acceptanceTick = Find.TickManager.TicksGame;
			accepterPawn = by;
			dismissed = false;
			Initiate();
		}
	}

	public void End(QuestEndOutcome outcome, bool sendLetter = true, bool playSound = true)
	{
		if (Historical)
		{
			Log.Error("Tried to resolve a historical quest. id=" + id);
			return;
		}
		ended = true;
		endOutcome = outcome;
		CleanupQuestParts();
		if ((!EverAccepted && State == QuestState.EndedOfferExpired) || !sendLetter || hidden)
		{
			return;
		}
		string key = null;
		string key2 = null;
		LetterDef textLetterDef = null;
		switch (State)
		{
		case QuestState.EndedFailed:
			key2 = "LetterQuestFailedLabel";
			key = "LetterQuestCompletedFail";
			textLetterDef = LetterDefOf.NegativeEvent;
			if (playSound)
			{
				SoundDefOf.Quest_Failed.PlayOneShotOnCamera();
			}
			break;
		case QuestState.EndedSuccess:
			key2 = "LetterQuestCompletedLabel";
			key = "LetterQuestCompletedSuccess";
			textLetterDef = LetterDefOf.PositiveEvent;
			if (playSound)
			{
				SoundDefOf.Quest_Succeded.PlayOneShotOnCamera();
			}
			break;
		case QuestState.EndedUnknownOutcome:
			key2 = "LetterQuestConcludedLabel";
			key = "LetterQuestCompletedConcluded";
			textLetterDef = LetterDefOf.NeutralEvent;
			if (playSound)
			{
				SoundDefOf.Quest_Concluded.PlayOneShotOnCamera();
			}
			break;
		}
		Find.LetterStack.ReceiveLetter(key2.Translate(), key.Translate(name.CapitalizeFirst()), textLetterDef, null, null, this, null, null, 0, playSound);
	}

	public bool QuestReserves(Pawn p)
	{
		if (Historical)
		{
			return false;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i].QuestPartReserves(p))
			{
				return true;
			}
		}
		return false;
	}

	public bool QuestReserves(Faction f)
	{
		if (Historical)
		{
			return false;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i].QuestPartReserves(f))
			{
				return true;
			}
		}
		return false;
	}

	public bool QuestReserves(TransportShip ship)
	{
		if (Historical)
		{
			return false;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i].QuestPartReserves(ship))
			{
				return true;
			}
		}
		return false;
	}

	public void SetInitiallyAccepted()
	{
		acceptanceTick = Find.TickManager.TicksGame;
		ticksUntilAcceptanceExpiry = -1;
		initiallyAccepted = true;
	}

	public void SetNotYetAccepted()
	{
		acceptanceTick = -1;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref appearanceTick, "appearanceTick", -1);
		Scribe_Values.Look(ref acceptanceTick, "acceptanceTick", -1);
		Scribe_Values.Look(ref ticksUntilAcceptanceExpiry, "ticksUntilAcceptanceExpiry", -1);
		Scribe_References.Look(ref accepterPawn, "acceptedBy");
		Scribe_Values.Look(ref accepterPawnLabel, "acceptedByLabel");
		Scribe_Values.Look(ref ended, "ended", defaultValue: false);
		Scribe_Values.Look(ref endOutcome, "endOutcome", QuestEndOutcome.Unknown);
		Scribe_Values.Look(ref cleanedUp, "cleanedUp", defaultValue: false);
		Scribe_Values.Look(ref cleanupTick, "cleanupTick", -1);
		Scribe_Values.Look(ref initiallyAccepted, "initiallyAccepted", defaultValue: false);
		Scribe_Values.Look(ref dismissed, "dismissed", defaultValue: false);
		Scribe_Values.Look(ref hiddenInUI, "hiddenInUI", defaultValue: false);
		Scribe_Values.Look(ref challengeRating, "challengeRating", 0);
		Scribe_Values.Look(ref description, "description");
		Scribe_Values.Look(ref lastSlateStateDebug, "lastSlateStateDebug");
		Scribe_Defs.Look(ref root, "root");
		Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
		Scribe_Collections.Look(ref signalsReceivedDebug, "signalsReceivedDebug", LookMode.Undefined);
		Scribe_Collections.Look(ref parts, "parts", LookMode.Deep);
		Scribe_Collections.Look(ref tags, "tags", LookMode.Value);
		Scribe_Values.Look(ref charity, "charity", defaultValue: false);
		Scribe_References.Look(ref parent, "parent");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (parts.RemoveAll((QuestPart x) => x == null) != 0)
			{
				Log.Error("Some quest parts were null after loading.");
			}
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].quest = this;
			}
		}
	}

	public void Notify_PawnDiscarded(Pawn pawn)
	{
		if (accepterPawn == pawn)
		{
			accepterPawn = null;
			accepterPawnLabel = pawn.LabelCap;
		}
		foreach (QuestPart item in PartsListForReading)
		{
			item.Notify_PawnDiscarded(pawn);
		}
	}

	public void Notify_SignalReceived(Signal signal)
	{
		if (!signal.global && !signal.tag.StartsWith("Quest" + id + "."))
		{
			return;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			try
			{
				if (parts[i].signalListenMode switch
				{
					QuestPart.SignalListenMode.OngoingOnly => State == QuestState.Ongoing, 
					QuestPart.SignalListenMode.NotYetAcceptedOnly => State == QuestState.NotYetAccepted, 
					QuestPart.SignalListenMode.OngoingOrNotYetAccepted => State == QuestState.Ongoing || State == QuestState.NotYetAccepted, 
					QuestPart.SignalListenMode.HistoricalOnly => Historical, 
					QuestPart.SignalListenMode.Always => true, 
					_ => false, 
				})
				{
					parts[i].Notify_QuestSignalReceived(signal);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error while processing a quest signal: " + ex);
			}
		}
	}

	public void PostAdded()
	{
		Find.SignalManager.SendSignal(new Signal(AddedSignal));
	}

	public void Initiate()
	{
		Find.SignalManager.SendSignal(new Signal(InitiateSignal));
	}

	public void CleanupQuestParts()
	{
		if (cleanedUp)
		{
			return;
		}
		cleanupTick = Find.TickManager.TicksGame;
		for (int i = 0; i < parts.Count; i++)
		{
			try
			{
				parts[i].Notify_PreCleanup();
			}
			catch (Exception ex)
			{
				Log.Error("Error in QuestPart Notify_PreCleanup: " + ex);
			}
		}
		for (int j = 0; j < parts.Count; j++)
		{
			try
			{
				parts[j].Cleanup();
			}
			catch (Exception ex2)
			{
				Log.Error("Error in QuestPart cleanup: " + ex2);
			}
		}
		cleanedUp = true;
		Find.FactionManager.Notify_QuestCleanedUp(this);
		IdeoUtility.Notify_QuestCleanedUp(this, State);
		if (root.hideOnCleanup)
		{
			hiddenInUI = true;
		}
	}

	public void Notify_ThingsProduced(Pawn worker, List<Thing> things)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Notify_ThingsProduced(worker, things);
		}
	}

	public void Notify_PlantHarvested(Pawn worker, Thing harvested)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Notify_PlantHarvested(worker, harvested);
		}
	}

	public void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Notify_PawnKilled(pawn, dinfo);
		}
	}

	public void Notify_PawnBorn(Thing baby, Thing birther, Pawn mother, Pawn father)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Notify_PawnBorn(baby, birther, mother, father);
		}
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Notify_FactionRemoved(faction);
		}
	}

	public string GetUniqueLoadID()
	{
		return "Quest_" + id;
	}
}
