using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class CompSuppressable : ThingComp
{
	private const float maxSuppression = 1050f;

	private const float SuppressionMultiplier = 2f;

	private const int TicksForDecayStart = 30;

	private const float SuppressionDecayRate = 4f;

	private const int TicksPerMote = 150;

	private const int MinTicksUntilMentalBreak = 600;

	private const float ChanceBreakPerTick = 0.001f;

	private const int HelpRequestCooldown = 2400;

	private int lastHelpRequestAt = -1;

	private IntVec3 suppressorLoc;

	private float locSuppressionAmount = 0f;

	private float currentSuppression = 0f;

	public bool isSuppressed = false;

	private int ticksUntilDecay = 0;

	private int ticksHunkered;

	private bool isCrouchWalking;

	private float breakThresholdMajorCached;

	private int breakThresholdMajorTickCheck;

	private float currentMoodCached;

	private int currentMoodTickCheck;

	private CompInventory _compInventory = null;

	public CompProperties_Suppressable Props => (CompProperties_Suppressable)props;

	public IntVec3 SuppressorLoc => suppressorLoc;

	public float CurrentSuppression => currentSuppression;

	private float SuppressionThreshold
	{
		get
		{
			float result = 0f;
			if (parent is Pawn pawn)
			{
				float num = BreakThresholdMajorCached(pawn);
				float num2 = CurrentMoodCached(pawn);
				result = Mathf.Sqrt(Mathf.Max(0f, num2 - num)) * 1050f * 0.125f;
			}
			else
			{
				Log.Error("CE tried to get suppression threshold of non-pawn");
			}
			return result;
		}
	}

	private CompInventory CompInventory
	{
		get
		{
			if (_compInventory == null)
			{
				_compInventory = parent.TryGetComp<CompInventory>();
			}
			return _compInventory;
		}
	}

	public bool IsHunkering
	{
		get
		{
			if (currentSuppression > SuppressionThreshold * 10f)
			{
				if (isSuppressed)
				{
					return true;
				}
				Log.Error("CE hunkering without suppression, this should never happen");
			}
			return false;
		}
	}

	public bool CanReactToSuppression
	{
		get
		{
			Pawn pawn = parent as Pawn;
			return !pawn.Downed && !pawn.InMentalState && (pawn.stances?.curStance as Stance_Busy)?.verb?.IsMeleeAttack != true;
		}
	}

	public bool IsCrouchWalking => CanReactToSuppression && isCrouchWalking;

	private float BreakThresholdMajorCached(Pawn pawn)
	{
		if (breakThresholdMajorTickCheck == 0 || Find.TickManager.TicksGame > breakThresholdMajorTickCheck + 30)
		{
			breakThresholdMajorCached = (pawn.mindState?.mentalBreaker?.BreakThresholdMajor).GetValueOrDefault();
			breakThresholdMajorTickCheck = Find.TickManager.TicksGame;
		}
		return breakThresholdMajorCached;
	}

	private float CurrentMoodCached(Pawn pawn)
	{
		if (currentMoodTickCheck == 0 || Find.TickManager.TicksGame > currentMoodTickCheck + 30)
		{
			currentMoodCached = pawn.needs?.mood?.CurLevel ?? 0.5f;
			currentMoodTickCheck = Find.TickManager.TicksGame;
		}
		return currentMoodCached;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref currentSuppression, "currentSuppression", 0f);
		Scribe_Values.Look(ref suppressorLoc, "suppressorLoc");
		Scribe_Values.Look(ref locSuppressionAmount, "locSuppression", 0f);
		Scribe_Values.Look(ref isSuppressed, "isSuppressed", defaultValue: false);
		Scribe_Values.Look(ref ticksUntilDecay, "ticksUntilDecay", 0);
		Scribe_Values.Look(ref lastHelpRequestAt, "lastHelpRequestAt", -1);
	}

	public void AddSuppression(float amount, IntVec3 origin)
	{
		if (!(parent is Pawn pawn))
		{
			Log.Error("CE trying to suppress non-pawn " + parent.ToString() + ", this should never happen");
			return;
		}
		if (!CanReactToSuppression)
		{
			currentSuppression = 0f;
			isSuppressed = false;
			return;
		}
		float num = amount * pawn.GetStatValue(CE_StatDefOf.Suppressability) * 2f;
		currentSuppression += num;
		if (Controller.settings.DebugShowSuppressionBuildup)
		{
			MoteMakerCE.ThrowText(pawn.DrawPos, pawn.Map, num.ToString());
		}
		ticksUntilDecay = 30;
		if (currentSuppression > 1050f)
		{
			currentSuppression = 1050f;
		}
		if (suppressorLoc == origin)
		{
			locSuppressionAmount += num;
		}
		else if (locSuppressionAmount < SuppressionThreshold || num > SuppressionThreshold)
		{
			suppressorLoc = origin;
			locSuppressionAmount = currentSuppression;
		}
		if (!(currentSuppression > SuppressionThreshold))
		{
			return;
		}
		isSuppressed = true;
		JobDef curJobDef = pawn.CurJobDef;
		if (curJobDef != CE_JobDefOf.RunForCover)
		{
			Job job = SuppressionUtility.GetRunForCoverJob(pawn);
			if (job == null && IsHunkering)
			{
				job = JobMaker.MakeJob(CE_JobDefOf.HunkerDown, pawn);
				LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Hunkering, pawn, OpportunityType.Critical);
			}
			if (job != null && job.def != curJobDef)
			{
				pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.A).Cell);
				pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, pawn.jobs.curJob?.def == JobDefOf.ManTurret, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_SuppressionReaction, pawn, OpportunityType.Critical);
			}
			else
			{
				isCrouchWalking = true;
			}
		}
		if (Rand.Chance(0.01f))
		{
			((TauntThrower)pawn.Map.GetComponent(typeof(TauntThrower)))?.TryThrowTaunt(CE_RulePackDefOf.SuppressedMote, pawn);
		}
	}

	public bool IgnoreSuppresion(IntVec3 origin)
	{
		return BlockerRegistry.PawnUnsuppressableFromCallback(parent as Pawn, origin) || (from x in SuppressionUtility.InterceptorZonesFor((Pawn)parent)
			where x.Contains(parent.Position)
			select x).Any((IEnumerable<IntVec3> x) => !x.Contains(origin));
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!isSuppressed)
		{
			ticksHunkered = 0;
		}
		else if (IsHunkering)
		{
			ticksHunkered++;
		}
		if (ticksHunkered > 600 && Rand.Chance(0.001f))
		{
			Pawn pawn = (Pawn)parent;
			if (pawn.mindState != null && !pawn.mindState.mentalStateHandler.InMentalState)
			{
				List<MentalStateDef> possibleBreaks = SuppressionUtility.GetPossibleBreaks(pawn);
				if (possibleBreaks.Any())
				{
					pawn.mindState.mentalStateHandler.TryStartMentalState(possibleBreaks.RandomElement());
				}
			}
		}
		if (ticksUntilDecay > 0)
		{
			ticksUntilDecay--;
		}
		else if (currentSuppression > 0f)
		{
			if (Controller.settings.DebugShowSuppressionBuildup && parent.IsHashIntervalTick(30))
			{
				MoteMakerCE.ThrowText(parent.DrawPos, parent.Map, "-" + 120f, Color.red);
			}
			currentSuppression -= Mathf.Min(4f, currentSuppression);
			isSuppressed = currentSuppression > 0f;
			if (!isSuppressed)
			{
				isCrouchWalking = false;
			}
			locSuppressionAmount -= Mathf.Min(4f, locSuppressionAmount);
		}
		if (parent.IsHashIntervalTick(150) && CanReactToSuppression)
		{
			if (IsHunkering)
			{
				CE_Utility.MakeIconOverlay((Pawn)parent, CE_ThingDefOf.Mote_HunkerIcon);
			}
			else if (isSuppressed)
			{
				CE_Utility.MakeIconOverlay((Pawn)parent, CE_ThingDefOf.Mote_SuppressIcon);
			}
		}
		if (!parent.Faction.IsPlayerSafe() && parent.IsHashIntervalTick(120) && isSuppressed && GenTicks.TicksGame - lastHelpRequestAt > 2400)
		{
			lastHelpRequestAt = GenTicks.TicksGame;
			SuppressionUtility.TryRequestHelp(parent as Pawn);
		}
	}
}
