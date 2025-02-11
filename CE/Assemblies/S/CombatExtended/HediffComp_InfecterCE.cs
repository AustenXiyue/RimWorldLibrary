using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class HediffComp_InfecterCE : HediffComp
{
	private const float InfectionInnerModifier = 3f;

	private const float TreatmentQualityExponential = 3f;

	private const float TreatmentQualityBase = 0.75f;

	private const float DamageThreshold = 10f;

	private static readonly IntRange InfectionDelayHours = new IntRange(6, 12);

	private bool _alreadyCausedInfection = false;

	private int _ticksUntilInfect = -1;

	private float _infectionModifier = 1f;

	private int _ticksTended = 0;

	private bool _tendedOutside;

	public HediffCompProperties_InfecterCE Props => (HediffCompProperties_InfecterCE)props;

	private bool IsInternal => parent.Part.depth == BodyPartDepth.Inside;

	private void CheckMakeInfection()
	{
		_ticksUntilInfect = -1;
		_infectionModifier *= base.Pawn.health.immunity.DiseaseContractChanceFactor(HediffDefOf.WoundInfection, parent.Part);
		if (base.Pawn.def.race.Animal)
		{
			_infectionModifier *= 0.5f;
		}
		if (base.Pawn.Faction == Faction.OfPlayer)
		{
			_infectionModifier *= Find.Storyteller.difficulty.playerPawnInfectionChanceFactor;
		}
		HediffComp_TendDuration hediffComp_TendDuration = parent.TryGetComp<HediffComp_TendDuration>();
		int num = parent.ageTicks;
		if (hediffComp_TendDuration != null && hediffComp_TendDuration.IsTended)
		{
			num -= _ticksTended;
			_infectionModifier /= Mathf.Pow(hediffComp_TendDuration.tendQuality + 0.75f, 3f);
		}
		float num2 = Props.infectionChancePerHourUntended * ((float)num / 2500f);
		if (IsInternal)
		{
			num2 *= 3f;
		}
		num2 *= parent.Severity / 10f;
		if (Rand.Value < num2 * _infectionModifier)
		{
			_alreadyCausedInfection = true;
			base.Pawn.health.AddHediff(HediffDefOf.WoundInfection, parent.Part, null);
		}
	}

	public override string CompDebugString()
	{
		if (_alreadyCausedInfection)
		{
			return "already caused infection";
		}
		if (_ticksUntilInfect <= 0)
		{
			return "no infection will appear";
		}
		return "infection may appear after: " + _ticksUntilInfect + " ticks (infection chance factor: " + _infectionModifier + ")";
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref _alreadyCausedInfection, "alreadyCausedInfection", defaultValue: false);
		Scribe_Values.Look(ref _ticksUntilInfect, "ticksUntilInfect", -1);
		Scribe_Values.Look(ref _infectionModifier, "infectionModifier", 1f);
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		if (!_alreadyCausedInfection && !parent.Part.def.IsSolid(parent.Part, base.Pawn.health.hediffSet.hediffs) && !base.Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part) && !parent.IsPermanent())
		{
			_ticksUntilInfect = InfectionDelayHours.RandomInRange * 2500;
		}
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if ((!_tendedOutside || !IsInternal) && parent.TryGetComp<HediffComp_TendDuration>().IsTended)
		{
			_ticksTended++;
		}
		if (!_alreadyCausedInfection && _ticksUntilInfect > 0)
		{
			_ticksUntilInfect--;
			if (_ticksUntilInfect == 0)
			{
				CheckMakeInfection();
			}
		}
	}

	public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
	{
		if (base.Pawn.Spawned)
		{
			Room room = base.Pawn.GetRoom();
			_tendedOutside = room == null;
			_infectionModifier *= ((room == null) ? RoomStatDefOf.InfectionChanceFactor.roomlessScore : RoomStatDefOf.InfectionChanceFactor.Worker.GetScore(room));
		}
	}
}
