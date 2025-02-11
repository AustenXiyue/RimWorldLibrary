using System.Collections.Generic;
using System.Linq;
using CombatExtended.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.AI;

public class CompOpportunisticSwitch : ICompTactics
{
	private enum TargetType
	{
		None,
		Pawn,
		Turret
	}

	private const int COOLDOWN_OPPORTUNISTIC_TICKS = 1400;

	private const int COOLDOWN_TICKS = 2000;

	private const int COOLDOWN_FACTION_TICKS = 600;

	private int lastFlared = -1;

	private int lastUsedAEOWeapon = -1;

	private int lastOptimizedWeapon = -1;

	private int lastOpportunisticSwitch = -1;

	private static Dictionary<Faction, int> factionLastFlare;

	private int _NVEfficiencyAge = -1;

	private float _NVEfficiency = -1f;

	public override int Priority => 500;

	public LightingTracker LightingTracker => base.Map.GetLightingTracker();

	public float NightVisionEfficiency
	{
		get
		{
			if (_NVEfficiency == -1f || GenTicks.TicksGame - _NVEfficiencyAge > 250)
			{
				_NVEfficiency = SelPawn.GetStatValue(CE_StatDefOf.NightVisionEfficiency);
				_NVEfficiencyAge = GenTicks.TicksGame;
			}
			return _NVEfficiency;
		}
	}

	public bool OpportunisticallySwitchedRecently => GenTicks.TicksGame - lastOpportunisticSwitch < 1400;

	public bool FlaredRecently => lastFlared != -1 && GenTicks.TicksGame - lastFlared < 2000;

	public bool OptimizedWeaponRecently => lastOptimizedWeapon != -1 && GenTicks.TicksGame - lastOptimizedWeapon < 2000;

	public virtual bool ShouldRun => !(SelPawn.Faction?.IsPlayer ?? false);

	public bool FlaredRecentlyByFaction
	{
		get
		{
			if (((Thing)SelPawn).factionInt != null && factionLastFlare.TryGetValue(((Thing)SelPawn).factionInt, out var value) && GenTicks.TicksGame - value < 600)
			{
				return true;
			}
			return false;
		}
	}

	public bool UsedAOEWeaponRecently => lastUsedAEOWeapon != -1 && GenTicks.TicksGame - lastUsedAEOWeapon < 2000;

	static CompOpportunisticSwitch()
	{
		factionLastFlare = new Dictionary<Faction, int>();
		CacheClearComponent.AddClearCacheAction(delegate
		{
			factionLastFlare.Clear();
		});
	}

	public override bool StartCastChecks(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		if (!ShouldRun)
		{
			return true;
		}
		if (OpportunisticallySwitchedRecently)
		{
			return true;
		}
		if (TryFlare(verb, castTarg, destTarg))
		{
			return false;
		}
		return true;
	}

	public bool TryUseAOE(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		if (!UsedAOEWeaponRecently && !(verb.EquipmentSource?.def.IsAOEWeapon() ?? false))
		{
			TargetType targetType2 = TargetType.None;
			float distance2 = castTarg.Cell.DistanceTo(SelPawn.Position);
			if (castTarg.HasThing && (TargetingPawns(castTarg.Thing, distance2, out targetType2) || TargetingTurrets(castTarg.Thing, distance2, out targetType2) || Rand.Chance(0.1f)))
			{
				if (targetType2 == TargetType.Turret)
				{
				}
				if (CompInventory.TryFindRandomAOEWeapon(out var weapon, (ThingWithComps g) => g.def.Verbs?.Any((VerbProperties t) => t.range >= distance2 + 3f) ?? false, checkAmmo: true))
				{
					lastOpportunisticSwitch = GenTicks.TicksGame;
					VerbProperties verbProperties = weapon.def.verbs.First((VerbProperties v) => !v.IsMeleeAttack);
					IntVec3 intVec = AI_Utility.FindAttackedClusterCenter(SelPawn, castTarg.Cell, weapon.def.verbs.Max((VerbProperties v) => v.range), 4f, (IntVec3 pos) => GenSight.LineOfSight(SelPawn.Position, pos, base.Map, skipFirstCell: true));
					Job job = JobMaker.MakeJob(CE_JobDefOf.OpportunisticAttack, weapon, intVec.IsValid ? intVec : castTarg.Cell);
					job.maxNumStaticAttacks = 1;
					SelPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
					return true;
				}
			}
		}
		return false;
		bool TargetingPawns(Thing thing, float distance, out TargetType targetType)
		{
			targetType = TargetType.None;
			if (thing is Pawn pawn && (distance > 8f || SelPawn.HiddingBehindCover(((Thing)pawn).positionInt)) && TargetIsSquad(pawn))
			{
				targetType = TargetType.Pawn;
				return true;
			}
			return false;
		}
		bool TargetingTurrets(Thing thing, float distance, out TargetType targetType)
		{
			targetType = TargetType.None;
			if (thing is Building_Turret && (distance > 8f || SelPawn.HiddingBehindCover(thing.positionInt)))
			{
				targetType = TargetType.Turret;
				return true;
			}
			return false;
		}
	}

	public bool TryFlare(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		if (!FlaredRecently && !FlaredRecentlyByFaction && !base.Map.VisibilityGoodAt(SelPawn, castTarg.Cell, NightVisionEfficiency) && verb?.EquipmentSource?.def.IsIlluminationDevice() != true && CompInventory.TryFindFlare(out var flareGun, checkAmmo: true))
		{
			float num = flareGun.def.verbs.Max((VerbProperties v) => v.range);
			VerbProperties nextVerb = flareGun.def.verbs.First((VerbProperties v) => !v.IsMeleeAttack);
			if (num >= castTarg.Cell.DistanceTo(SelPawn.Position))
			{
				lastOpportunisticSwitch = GenTicks.TicksGame;
				IntVec3 intVec = AI_Utility.FindAttackedClusterCenter(SelPawn, castTarg.Cell, flareGun.def.verbs.Max((VerbProperties v) => v.range), 8f, (IntVec3 pos) => !nextVerb.requireLineOfSight || !pos.Roofed(base.Map));
				Job job = JobMaker.MakeJob(CE_JobDefOf.OpportunisticAttack, flareGun, intVec.IsValid ? intVec : castTarg.Cell);
				job.maxNumStaticAttacks = 1;
				SelPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				return true;
			}
		}
		if (verb?.EquipmentSource?.def.IsIlluminationDevice() == true && !(SelPawn.jobs?.curDriver is IJobDriver_Tactical) && CompInventory.TryFindViableWeapon(out var weapon))
		{
			SelPawn.jobs.StartJob(JobMaker.MakeJob(CE_JobDefOf.EquipFromInventory, weapon), JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
			return true;
		}
		return false;
	}

	public override void OnStartCastSuccess(Verb verb)
	{
		base.OnStartCastSuccess(verb);
		ThingWithComps equipmentSource = verb.EquipmentSource;
		if (equipmentSource != null && equipmentSource.def.IsIlluminationDevice())
		{
			lastFlared = GenTicks.TicksGame;
			if (SelPawn.Faction != null)
			{
				factionLastFlare[SelPawn.Faction] = lastFlared;
			}
		}
		ThingWithComps equipmentSource2 = verb.EquipmentSource;
		if (equipmentSource2 != null && equipmentSource2.def.IsAOEWeapon())
		{
			lastUsedAEOWeapon = GenTicks.TicksGame;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastUsedAEOWeapon, "lastUsedAEOWeapon", -1);
		Scribe_Values.Look(ref lastFlared, "lastFlared", -1);
		Scribe_Values.Look(ref lastOptimizedWeapon, "lastOptimizedWeapon", -1);
		Scribe_Values.Look(ref lastOpportunisticSwitch, "lastOpportunisticSwitch", -1);
	}

	private bool TargetIsSquad(Pawn pawn)
	{
		int num = 0;
		foreach (Pawn item in pawn.Position.PawnsInRange(pawn.Map, 4f))
		{
			if (item.Faction != null)
			{
				if (!item.Faction.HostileTo(SelPawn.Faction))
				{
					return false;
				}
				num++;
			}
		}
		return num > 1;
	}
}
