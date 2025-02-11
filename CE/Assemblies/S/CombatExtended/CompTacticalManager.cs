using System;
using System.Collections.Generic;
using CombatExtended.AI;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class CompTacticalManager : ThingComp
{
	private Job curJob = null;

	private List<Verse.WeakReference<Pawn>> targetedBy = new List<Verse.WeakReference<Pawn>>();

	private Pawn _pawn = null;

	private List<ICompTactics> _tacticalComps = new List<ICompTactics>();

	private CompSuppressable _compSuppressable = null;

	private CompInventory _compInventory = null;

	private int _targetedByTick = -1;

	private List<Pawn> _targetedByCache = new List<Pawn>();

	private int _targetedByEnemyTick = -1;

	private List<Pawn> _targetedByEnemyCache = new List<Pawn>();

	private readonly TargetIndex[] _targetIndices = new TargetIndex[3]
	{
		TargetIndex.A,
		TargetIndex.B,
		TargetIndex.C
	};

	private int _counter = 0;

	public Pawn SelPawn => _pawn ?? (_pawn = parent as Pawn);

	public List<ICompTactics> TacticalComps
	{
		get
		{
			List<ICompTactics> tacticalComps = _tacticalComps;
			if (tacticalComps == null || tacticalComps.Count == 0)
			{
				ValidateComps();
			}
			return _tacticalComps;
		}
	}

	public virtual CompSuppressable CompSuppressable
	{
		get
		{
			if (_compSuppressable == null)
			{
				_compSuppressable = SelPawn.TryGetComp<CompSuppressable>();
			}
			return _compSuppressable;
		}
	}

	public virtual CompInventory CompInventory
	{
		get
		{
			if (_compInventory == null)
			{
				_compInventory = SelPawn.TryGetComp<CompInventory>();
			}
			return _compInventory;
		}
	}

	public List<Pawn> TargetedBy => _targetedByCache;

	public List<Pawn> TargetedByEnemy
	{
		get
		{
			if (_targetedByEnemyTick != GenTicks.TicksGame || _targetedByEnemyTick == -1)
			{
				_targetedByEnemyTick = GenTicks.TicksGame;
				_targetedByEnemyCache.Clear();
				List<Pawn> list = TargetedBy;
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					if (pawn.HostileTo(parent))
					{
						_targetedByEnemyCache.Add(pawn);
					}
				}
			}
			return _targetedByEnemyCache;
		}
	}

	public bool DraftedColonist
	{
		get
		{
			Faction faction = SelPawn.Faction;
			return faction != null && faction.IsPlayer && SelPawn.Drafted;
		}
	}

	public bool Active
	{
		get
		{
			Pawn_MutantTracker mutant = SelPawn.mutant;
			return (mutant == null || !mutant.HasTurned) && !SelPawn.Crawling;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!parent.IsHashIntervalTick(120) || !Active)
		{
			return;
		}
		if (_targetedByTick != -1 && GenTicks.TicksGame - _targetedByTick > 300)
		{
			_targetedByCache.Clear();
			_targetedByEnemyCache.Clear();
			_targetedByTick = -1;
			_targetedByEnemyTick = -1;
		}
		Job job;
		if (!parent.Spawned || curJob == (job = SelPawn.jobs?.curJob) || job == null || job.def.alwaysShowWeapon)
		{
			return;
		}
		if (SelPawn.mindState?.enemyTarget is Pawn { Spawned: not false } pawn)
		{
			pawn.GetTacticalManager()?.Notify_BeingTargetedBy(pawn);
		}
		HashSet<Pawn> hashSet = new HashSet<Pawn>();
		for (int i = 0; i < _targetIndices.Length; i++)
		{
			LocalTargetInfo target = job.GetTarget(_targetIndices[i]);
			if (target.HasThing && target.Thing is Pawn { Spawned: not false } pawn2)
			{
				hashSet.Add(pawn2);
			}
		}
		if (job.targetQueueA != null)
		{
			for (int j = 0; j < job.targetQueueA.Count; j++)
			{
				LocalTargetInfo localTargetInfo = job.targetQueueA[j];
				if (localTargetInfo.HasThing && localTargetInfo.Thing is Pawn { Spawned: not false } pawn3)
				{
					hashSet.Add(pawn3);
				}
			}
		}
		if (job.targetQueueB != null)
		{
			for (int k = 0; k < job.targetQueueB.Count; k++)
			{
				LocalTargetInfo localTargetInfo2 = job.targetQueueB[k];
				if (localTargetInfo2.HasThing && localTargetInfo2.Thing is Pawn { Spawned: not false } pawn4)
				{
					hashSet.Add(pawn4);
				}
			}
		}
		foreach (Pawn item in hashSet)
		{
			if (item.thingIDNumber != parent.thingIDNumber)
			{
				item.GetTacticalManager()?.Notify_BeingTargetedBy(item);
			}
		}
		hashSet.Clear();
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		if (Active)
		{
			TryGiveTacticalJobs();
			if (_counter++ % 2 == 0)
			{
				TickRarer();
			}
		}
	}

	public void Notify_BeingTargetedBy(Pawn pawn)
	{
		for (int i = 0; i < targetedBy.Count; i++)
		{
			if (targetedBy[i].Target == pawn)
			{
				return;
			}
		}
		targetedBy.Add(new Verse.WeakReference<Pawn>(pawn));
	}

	public bool TryStartCastChecks(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg)
	{
		if (CompSuppressable == null || SelPawn.MentalState != null || CompSuppressable.IsHunkering)
		{
			return true;
		}
		ICompTactics failedComp2 = null;
		if (!CompSuppressable.IsHunkering && (SelPawn.jobs.curDriver is IJobDriver_Tactical || AllChecksPassed(verb, castTarg, destTarg, out failedComp2)))
		{
			foreach (ICompTactics tacticalComp in TacticalComps)
			{
				tacticalComp.Notify_StartCastChecksSuccess(verb);
			}
			return true;
		}
		foreach (ICompTactics tacticalComp2 in TacticalComps)
		{
			tacticalComp2.Notify_StartCastChecksFailed(failedComp2);
		}
		return false;
		bool AllChecksPassed(Verb verb, LocalTargetInfo castTarg, LocalTargetInfo destTarg, out ICompTactics failedComp)
		{
			foreach (ICompTactics tacticalComp3 in TacticalComps)
			{
				if (!tacticalComp3.StartCastChecks(verb, castTarg, destTarg))
				{
					failedComp = tacticalComp3;
					return false;
				}
			}
			failedComp = null;
			return true;
		}
	}

	public void Notify_BulletImpactNearby()
	{
		if (!Active)
		{
			return;
		}
		foreach (ICompTactics tacticalComp in TacticalComps)
		{
			try
			{
				tacticalComp.Notify_BulletImpactNearBy();
			}
			catch (Exception arg)
			{
				Log.Error($"CE: Error running Notify_BulletImpactNearBy {tacticalComp.GetType()} with error {arg}");
			}
		}
	}

	public T GetTacticalComp<T>() where T : ICompTactics
	{
		return (T)TacticalComps.FirstOrFallback((ICompTactics c) => c is T);
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		try
		{
			Scribe_Collections.Look(ref _tacticalComps, "tacticalComps", LookMode.Deep);
		}
		catch (Exception arg)
		{
			Log.Error($"CE: Error scribing {parent} {arg}");
		}
		finally
		{
			ValidateComps();
		}
	}

	private void TickRarer()
	{
		foreach (ICompTactics tacticalComp in TacticalComps)
		{
			try
			{
				tacticalComp.TickRarer();
			}
			catch (Exception arg)
			{
				Log.Error($"CE: Error ticking comp {tacticalComp.GetType()} with error {arg}");
			}
		}
	}

	private void TryGiveTacticalJobs()
	{
		if (CompSuppressable == null || CompSuppressable.IsHunkering || !SelPawn.Spawned || SelPawn.Downed)
		{
			return;
		}
		foreach (ICompTactics tacticalComp in TacticalComps)
		{
			Job job = tacticalComp.TryGiveTacticalJob();
			if (job != null)
			{
				SelPawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				break;
			}
		}
	}

	private void ValidateComps()
	{
		if (_tacticalComps == null)
		{
			_tacticalComps = new List<ICompTactics>();
		}
		foreach (Type type in typeof(ICompTactics).AllSubclassesNonAbstract())
		{
			ICompTactics compTactics;
			if ((compTactics = _tacticalComps.FirstOrFallback((ICompTactics t) => t.GetType() == type)) == null)
			{
				_tacticalComps.Add(compTactics = (ICompTactics)Activator.CreateInstance(type, new object[0]));
			}
			compTactics.Initialize(SelPawn);
		}
		_tacticalComps.SortBy((ICompTactics t) => -1f * (float)t.Priority);
	}
}
