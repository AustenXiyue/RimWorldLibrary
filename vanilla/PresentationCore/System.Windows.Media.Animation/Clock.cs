using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Maintains run-time timing state for a <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
public class Clock : DispatcherObject
{
	[Flags]
	private enum ClockFlags : uint
	{
		IsTimeManager = 1u,
		IsRoot = 2u,
		IsBackwardsProgressingGlobal = 4u,
		IsInteractivelyPaused = 8u,
		IsInteractivelyStopped = 0x10u,
		PendingInteractivePause = 0x20u,
		PendingInteractiveResume = 0x40u,
		PendingInteractiveStop = 0x80u,
		PendingInteractiveRemove = 0x100u,
		CanGrow = 0x200u,
		CanSlip = 0x400u,
		CurrentStateInvalidatedEventRaised = 0x800u,
		CurrentTimeInvalidatedEventRaised = 0x1000u,
		CurrentGlobalSpeedInvalidatedEventRaised = 0x2000u,
		CompletedEventRaised = 0x4000u,
		RemoveRequestedEventRaised = 0x8000u,
		IsInEventQueue = 0x10000u,
		NeedsTicksWhenActive = 0x20000u,
		NeedsPostfixTraversal = 0x40000u,
		PauseStateChangedDuringTick = 0x80000u,
		RootBeginPending = 0x100000u,
		HasControllableRoot = 0x200000u,
		HasResolvedDuration = 0x400000u,
		HasDesiredFrameRate = 0x800000u,
		HasDiscontinuousTimeMovementOccured = 0x1000000u,
		HasDescendantsWithUnresolvedDuration = 0x2000000u,
		HasSeekOccuredAfterLastTick = 0x4000000u
	}

	private enum ResolveCode
	{
		NoChanges,
		NewTimes,
		NeedNewChildResolve
	}

	private class SubtreeFinalizer
	{
		private TimeManager _timeManager;

		internal SubtreeFinalizer(TimeManager timeManager)
		{
			_timeManager = timeManager;
		}

		~SubtreeFinalizer()
		{
			_timeManager.ScheduleClockCleanup();
		}
	}

	internal class SyncData
	{
		private Clock _syncClock;

		private double _syncClockSpeedRatio;

		private bool _isInSyncPeriod;

		private bool _syncClockDiscontinuousEvent;

		private Duration _syncClockResolvedDuration = Duration.Automatic;

		private TimeSpan? _syncClockEffectiveDuration;

		private TimeSpan? _syncClockBeginTime;

		private TimeSpan _previousSyncClockTime;

		private TimeSpan _previousRepeatTime;

		internal Clock SyncClock => _syncClock;

		internal Duration SyncClockResolvedDuration
		{
			get
			{
				if (!_syncClockResolvedDuration.HasTimeSpan)
				{
					_syncClockEffectiveDuration = _syncClock.ComputeEffectiveDuration();
					_syncClockResolvedDuration = _syncClock._resolvedDuration;
				}
				return _syncClockResolvedDuration;
			}
		}

		internal bool SyncClockHasReachedEffectiveDuration
		{
			get
			{
				if (_syncClockEffectiveDuration.HasValue)
				{
					return _previousRepeatTime + _syncClock.GetCurrentTimeCore() >= _syncClockEffectiveDuration.Value;
				}
				return false;
			}
		}

		internal TimeSpan? SyncClockEffectiveDuration => _syncClockEffectiveDuration;

		internal double SyncClockSpeedRatio => _syncClockSpeedRatio;

		internal bool IsInSyncPeriod
		{
			get
			{
				return _isInSyncPeriod;
			}
			set
			{
				_isInSyncPeriod = value;
			}
		}

		internal bool SyncClockDiscontinuousEvent
		{
			get
			{
				return _syncClockDiscontinuousEvent;
			}
			set
			{
				_syncClockDiscontinuousEvent = value;
			}
		}

		internal TimeSpan PreviousSyncClockTime
		{
			get
			{
				return _previousSyncClockTime;
			}
			set
			{
				_previousSyncClockTime = value;
			}
		}

		internal TimeSpan PreviousRepeatTime
		{
			get
			{
				return _previousRepeatTime;
			}
			set
			{
				_previousRepeatTime = value;
			}
		}

		internal TimeSpan SyncClockBeginTime => _syncClockBeginTime.Value;

		internal SyncData(Clock syncClock)
		{
			_syncClock = syncClock;
			UpdateClockBeginTime();
		}

		internal void UpdateClockBeginTime()
		{
			_syncClockBeginTime = _syncClock._beginTime;
			_syncClockSpeedRatio = _syncClock._appliedSpeedRatio;
			_syncClockResolvedDuration = SyncClockResolvedDuration;
		}
	}

	internal class RootData
	{
		private int _desiredFrameRate;

		private double _interactiveSpeedRatio = 1.0;

		private double? _pendingSpeedRatio;

		private TimeSpan _currentAdjustedGlobalTime;

		private TimeSpan _lastAdjustedGlobalTime;

		private TimeSpan? _pendingSeekDestination;

		internal TimeSpan CurrentAdjustedGlobalTime
		{
			get
			{
				return _currentAdjustedGlobalTime;
			}
			set
			{
				_currentAdjustedGlobalTime = value;
			}
		}

		internal int DesiredFrameRate
		{
			get
			{
				return _desiredFrameRate;
			}
			set
			{
				_desiredFrameRate = value;
			}
		}

		internal double InteractiveSpeedRatio
		{
			get
			{
				return _interactiveSpeedRatio;
			}
			set
			{
				_interactiveSpeedRatio = value;
			}
		}

		internal TimeSpan LastAdjustedGlobalTime
		{
			get
			{
				return _lastAdjustedGlobalTime;
			}
			set
			{
				_lastAdjustedGlobalTime = value;
			}
		}

		internal TimeSpan? PendingSeekDestination
		{
			get
			{
				return _pendingSeekDestination;
			}
			set
			{
				_pendingSeekDestination = value;
			}
		}

		internal double? PendingSpeedRatio
		{
			get
			{
				return _pendingSpeedRatio;
			}
			set
			{
				_pendingSpeedRatio = value;
			}
		}

		internal RootData()
		{
		}
	}

	private ClockFlags _flags;

	private int? _currentIteration;

	private double? _currentProgress;

	private double? _currentGlobalSpeed;

	private TimeSpan? _currentTime;

	private ClockState _currentClockState;

	private RootData _rootData;

	internal SyncData _syncData;

	internal TimeSpan? _beginTime;

	private TimeSpan? _currentIterationBeginTime;

	internal TimeSpan? _nextTickNeededTime;

	private WeakReference _weakReference;

	private SubtreeFinalizer _subtreeFinalizer;

	private EventHandlersStore _eventHandlersStore;

	internal Duration _resolvedDuration;

	internal Duration _currentDuration;

	private double _appliedSpeedRatio;

	internal Timeline _timeline;

	internal TimeManager _timeManager;

	internal ClockGroup _parent;

	internal int _childIndex;

	internal int _depth;

	private static long s_TimeSpanTicksPerSecond = TimeSpan.FromSeconds(1.0).Ticks;

	internal bool CanGrow => GetFlag(ClockFlags.CanGrow);

	internal bool CanSlip => GetFlag(ClockFlags.CanSlip);

	/// <summary>Gets a <see cref="T:System.Windows.Media.Animation.ClockController" /> that can be used to start, pause, resume, seek, skip, stop, or remove this <see cref="T:System.Windows.Media.Animation.Clock" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.ClockController" /> if this is a root clock; otherwise, null. </returns>
	public ClockController Controller
	{
		get
		{
			if (IsRoot && HasControllableRoot)
			{
				return new ClockController(this);
			}
			return null;
		}
	}

	/// <summary>Get the current iteration of this clock. </summary>
	/// <returns>This clock's current iteration within its current active period, or null if this clock is stopped. </returns>
	public int? CurrentIteration => _currentIteration;

	/// <summary>Gets the rate at which the clock's time is currently progressing, compared to real-world time.</summary>
	/// <returns>The rate at which this clock's time is current progressing, compared to real-world time. If the clock is stopped, this property returns null.</returns>
	public double? CurrentGlobalSpeed => _currentGlobalSpeed;

	/// <summary>Gets the current progress of this <see cref="T:System.Windows.Media.Animation.Clock" /> within its current iteration. </summary>
	/// <returns>null if this clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />, or 0.0 if this clock is active and its <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> has a <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of <see cref="P:System.Windows.Duration.Forever" />; otherwise, a value between 0.0 and 1.0 that indicates the current progress of this Clock within its current iteration. A value of 0.0 indicates no progress, and a value of 1.0 indicates that the clock is at the end of its current iteration. </returns>
	public double? CurrentProgress => _currentProgress;

	/// <summary>Gets a value indicating whether the clock is currently <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />. </summary>
	/// <returns>The current state of the clock: <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</returns>
	public ClockState CurrentState => _currentClockState;

	/// <summary>Gets this clock's current time within its current iteration.</summary>
	/// <returns>null if this clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />; otherwise, the current time of this clock.</returns>
	public TimeSpan? CurrentTime => _currentTime;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Animation.Clock" /> is part of a controllable clock tree.</summary>
	/// <returns>true if this clock belongs to a clock tree with a controllable root clock or if this clock is itself a controllable root; otherwise, false.</returns>
	public bool HasControllableRoot => GetFlag(ClockFlags.HasControllableRoot);

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Animation.Clock" />, or any of its parents, is paused. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.Animation.Clock" /> or any of its parents is paused; otherwise, false.</returns>
	public bool IsPaused => IsInteractivelyPaused;

	/// <summary>Gets the natural duration of this clock's <see cref="P:System.Windows.Media.Animation.Clock.Timeline" />.</summary>
	/// <returns>The natural duration of this clock, as determined by its <see cref="P:System.Windows.Media.Animation.Clock.Timeline" />. </returns>
	public Duration NaturalDuration => _timeline.GetNaturalDuration(this);

	/// <summary>Gets the clock that is the parent of this clock. </summary>
	/// <returns>The parent of this clock or null if this clock is a root.</returns>
	public Clock Parent
	{
		get
		{
			if (IsRoot)
			{
				return null;
			}
			return _parent;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> from which this <see cref="T:System.Windows.Media.Animation.Clock" /> was created.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> from which this <see cref="T:System.Windows.Media.Animation.Clock" /> was created.</returns>
	public Timeline Timeline => _timeline;

	/// <summary>Gets the current global time, as established by the WPF timing system. </summary>
	/// <returns>The current global time for the WPF timing system. </returns>
	protected TimeSpan CurrentGlobalTime
	{
		get
		{
			if (_timeManager == null)
			{
				return TimeSpan.Zero;
			}
			if (IsTimeManager)
			{
				return _timeManager.InternalCurrentGlobalTime;
			}
			Clock clock = this;
			while (!clock.IsRoot)
			{
				clock = clock._parent;
			}
			if (clock.HasDesiredFrameRate)
			{
				return clock._rootData.CurrentAdjustedGlobalTime;
			}
			return _timeManager.InternalCurrentGlobalTime;
		}
	}

	internal virtual Duration CurrentDuration => Duration.Automatic;

	internal int Depth => _depth;

	internal Duration EndOfActivePeriod
	{
		get
		{
			if (!HasResolvedDuration)
			{
				return Duration.Automatic;
			}
			ComputeExpirationTime(out var expirationTime);
			if (expirationTime.HasValue)
			{
				return expirationTime.Value;
			}
			return Duration.Forever;
		}
	}

	internal virtual Clock FirstChild => null;

	internal ClockState InternalCurrentClockState
	{
		get
		{
			return _currentClockState;
		}
		set
		{
			_currentClockState = value;
		}
	}

	internal double? InternalCurrentGlobalSpeed
	{
		get
		{
			return _currentGlobalSpeed;
		}
		set
		{
			_currentGlobalSpeed = value;
		}
	}

	internal int? InternalCurrentIteration
	{
		get
		{
			return _currentIteration;
		}
		set
		{
			_currentIteration = value;
		}
	}

	internal double? InternalCurrentProgress
	{
		get
		{
			return _currentProgress;
		}
		set
		{
			_currentProgress = value;
		}
	}

	internal TimeSpan? InternalNextTickNeededTime
	{
		get
		{
			return _nextTickNeededTime;
		}
		set
		{
			_nextTickNeededTime = value;
		}
	}

	internal ClockGroup InternalParent => _parent;

	internal Duration ResolvedDuration
	{
		get
		{
			ResolveDuration();
			return _resolvedDuration;
		}
	}

	internal Clock NextSibling
	{
		get
		{
			List<Clock> internalChildren = _parent.InternalChildren;
			if (_childIndex == internalChildren.Count - 1)
			{
				return null;
			}
			return internalChildren[_childIndex + 1];
		}
	}

	internal WeakReference WeakReference
	{
		get
		{
			WeakReference weakReference = _weakReference;
			if (weakReference == null)
			{
				weakReference = (_weakReference = new WeakReference(this));
			}
			return weakReference;
		}
	}

	internal int? DesiredFrameRate
	{
		get
		{
			int? result = null;
			if (HasDesiredFrameRate)
			{
				result = _rootData.DesiredFrameRate;
			}
			return result;
		}
	}

	internal bool CompletedEventRaised
	{
		get
		{
			return GetFlag(ClockFlags.CompletedEventRaised);
		}
		set
		{
			SetFlag(ClockFlags.CompletedEventRaised, value);
		}
	}

	internal bool CurrentGlobalSpeedInvalidatedEventRaised
	{
		get
		{
			return GetFlag(ClockFlags.CurrentGlobalSpeedInvalidatedEventRaised);
		}
		set
		{
			SetFlag(ClockFlags.CurrentGlobalSpeedInvalidatedEventRaised, value);
		}
	}

	internal bool CurrentStateInvalidatedEventRaised
	{
		get
		{
			return GetFlag(ClockFlags.CurrentStateInvalidatedEventRaised);
		}
		set
		{
			SetFlag(ClockFlags.CurrentStateInvalidatedEventRaised, value);
		}
	}

	internal bool CurrentTimeInvalidatedEventRaised
	{
		get
		{
			return GetFlag(ClockFlags.CurrentTimeInvalidatedEventRaised);
		}
		set
		{
			SetFlag(ClockFlags.CurrentTimeInvalidatedEventRaised, value);
		}
	}

	private bool HasDesiredFrameRate
	{
		get
		{
			return GetFlag(ClockFlags.HasDesiredFrameRate);
		}
		set
		{
			SetFlag(ClockFlags.HasDesiredFrameRate, value);
		}
	}

	internal bool HasResolvedDuration
	{
		get
		{
			return GetFlag(ClockFlags.HasResolvedDuration);
		}
		set
		{
			SetFlag(ClockFlags.HasResolvedDuration, value);
		}
	}

	internal bool IsBackwardsProgressingGlobal
	{
		get
		{
			return GetFlag(ClockFlags.IsBackwardsProgressingGlobal);
		}
		set
		{
			SetFlag(ClockFlags.IsBackwardsProgressingGlobal, value);
		}
	}

	internal bool IsInEventQueue
	{
		get
		{
			return GetFlag(ClockFlags.IsInEventQueue);
		}
		set
		{
			SetFlag(ClockFlags.IsInEventQueue, value);
		}
	}

	internal bool IsInteractivelyPaused
	{
		get
		{
			return GetFlag(ClockFlags.IsInteractivelyPaused);
		}
		set
		{
			SetFlag(ClockFlags.IsInteractivelyPaused, value);
		}
	}

	internal bool IsInteractivelyStopped
	{
		get
		{
			return GetFlag(ClockFlags.IsInteractivelyStopped);
		}
		set
		{
			SetFlag(ClockFlags.IsInteractivelyStopped, value);
		}
	}

	internal bool IsRoot
	{
		get
		{
			return GetFlag(ClockFlags.IsRoot);
		}
		set
		{
			SetFlag(ClockFlags.IsRoot, value);
		}
	}

	internal bool IsTimeManager
	{
		get
		{
			return GetFlag(ClockFlags.IsTimeManager);
		}
		set
		{
			SetFlag(ClockFlags.IsTimeManager, value);
		}
	}

	internal bool NeedsPostfixTraversal
	{
		get
		{
			return GetFlag(ClockFlags.NeedsPostfixTraversal);
		}
		set
		{
			SetFlag(ClockFlags.NeedsPostfixTraversal, value);
		}
	}

	internal virtual bool NeedsTicksWhenActive
	{
		get
		{
			return GetFlag(ClockFlags.NeedsTicksWhenActive);
		}
		set
		{
			SetFlag(ClockFlags.NeedsTicksWhenActive, value);
		}
	}

	internal bool PauseStateChangedDuringTick
	{
		get
		{
			return GetFlag(ClockFlags.PauseStateChangedDuringTick);
		}
		set
		{
			SetFlag(ClockFlags.PauseStateChangedDuringTick, value);
		}
	}

	internal bool PendingInteractivePause
	{
		get
		{
			return GetFlag(ClockFlags.PendingInteractivePause);
		}
		set
		{
			SetFlag(ClockFlags.PendingInteractivePause, value);
		}
	}

	internal bool PendingInteractiveRemove
	{
		get
		{
			return GetFlag(ClockFlags.PendingInteractiveRemove);
		}
		set
		{
			SetFlag(ClockFlags.PendingInteractiveRemove, value);
		}
	}

	internal bool PendingInteractiveResume
	{
		get
		{
			return GetFlag(ClockFlags.PendingInteractiveResume);
		}
		set
		{
			SetFlag(ClockFlags.PendingInteractiveResume, value);
		}
	}

	internal bool PendingInteractiveStop
	{
		get
		{
			return GetFlag(ClockFlags.PendingInteractiveStop);
		}
		set
		{
			SetFlag(ClockFlags.PendingInteractiveStop, value);
		}
	}

	internal bool RemoveRequestedEventRaised
	{
		get
		{
			return GetFlag(ClockFlags.RemoveRequestedEventRaised);
		}
		set
		{
			SetFlag(ClockFlags.RemoveRequestedEventRaised, value);
		}
	}

	private bool HasDiscontinuousTimeMovementOccured
	{
		get
		{
			return GetFlag(ClockFlags.HasDiscontinuousTimeMovementOccured);
		}
		set
		{
			SetFlag(ClockFlags.HasDiscontinuousTimeMovementOccured, value);
		}
	}

	internal bool HasDescendantsWithUnresolvedDuration
	{
		get
		{
			return GetFlag(ClockFlags.HasDescendantsWithUnresolvedDuration);
		}
		set
		{
			SetFlag(ClockFlags.HasDescendantsWithUnresolvedDuration, value);
		}
	}

	private bool HasSeekOccuredAfterLastTick
	{
		get
		{
			return GetFlag(ClockFlags.HasSeekOccuredAfterLastTick);
		}
		set
		{
			SetFlag(ClockFlags.HasSeekOccuredAfterLastTick, value);
		}
	}

	private bool IsInTimingTree
	{
		get
		{
			if (_timeManager != null)
			{
				return _timeManager.State != TimeState.Stopped;
			}
			return false;
		}
	}

	private bool RootBeginPending
	{
		get
		{
			return GetFlag(ClockFlags.RootBeginPending);
		}
		set
		{
			SetFlag(ClockFlags.RootBeginPending, value);
		}
	}

	/// <summary>Occurs when this clock has completely finished playing.</summary>
	public event EventHandler Completed
	{
		add
		{
			AddEventHandler(Timeline.CompletedKey, value);
		}
		remove
		{
			RemoveEventHandler(Timeline.CompletedKey, value);
		}
	}

	/// <summary>Occurs when the clock's speed is updated.</summary>
	public event EventHandler CurrentGlobalSpeedInvalidated
	{
		add
		{
			AddEventHandler(Timeline.CurrentGlobalSpeedInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(Timeline.CurrentGlobalSpeedInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when the clock's <see cref="P:System.Windows.Media.Animation.Clock.CurrentState" /> property is updated. </summary>
	public event EventHandler CurrentStateInvalidated
	{
		add
		{
			AddEventHandler(Timeline.CurrentStateInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(Timeline.CurrentStateInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when this clock's <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> becomes invalid. </summary>
	public event EventHandler CurrentTimeInvalidated
	{
		add
		{
			AddEventHandler(Timeline.CurrentTimeInvalidatedKey, value);
		}
		remove
		{
			RemoveEventHandler(Timeline.CurrentTimeInvalidatedKey, value);
		}
	}

	/// <summary>Occurs when the <see cref="M:System.Windows.Media.Animation.ClockController.Remove" /> method is called on this <see cref="T:System.Windows.Media.Animation.Clock" /> or one of its parent clocks. </summary>
	public event EventHandler RemoveRequested
	{
		add
		{
			AddEventHandler(Timeline.RemoveRequestedKey, value);
		}
		remove
		{
			RemoveEventHandler(Timeline.RemoveRequestedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Clock" /> class, using the specified <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> as a template. The new <see cref="T:System.Windows.Media.Animation.Clock" /> object has no children. </summary>
	/// <param name="timeline">The <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> from which this clock should be constructed. Clocks are not created for any child <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> objects, if they exist.</param>
	protected internal Clock(Timeline timeline)
	{
		_timeline = (Timeline)timeline.GetCurrentValueAsFrozen();
		_eventHandlersStore = timeline.InternalEventHandlersStore;
		SetFlag(ClockFlags.NeedsTicksWhenActive, _eventHandlersStore != null);
		_beginTime = _timeline.BeginTime;
		_resolvedDuration = _timeline.Duration;
		if (_resolvedDuration == Duration.Automatic)
		{
			_resolvedDuration = Duration.Forever;
		}
		else
		{
			HasResolvedDuration = true;
		}
		_currentDuration = _resolvedDuration;
		_appliedSpeedRatio = _timeline.SpeedRatio;
		_currentClockState = ClockState.Stopped;
		if (_beginTime.HasValue)
		{
			_nextTickNeededTime = TimeSpan.Zero;
		}
	}

	/// <summary>When implemented in a derived class, will be invoked whenever a clock repeats, skips, or seeks. </summary>
	protected virtual void DiscontinuousTimeMovement()
	{
	}

	/// <summary>Returns whether the <see cref="T:System.Windows.Media.Animation.Clock" /> has its own external time source, which may require synchronization with the timing system.</summary>
	/// <returns>Returns true if the <see cref="T:System.Windows.Media.Animation.Clock" /> has its own external source for time, which may require synchronization with the timing system; otherwise, false.</returns>
	protected virtual bool GetCanSlip()
	{
		return false;
	}

	/// <summary>Gets this clock's current time within its current iteration.</summary>
	/// <returns>The current time of this clock if it is active or filling; otherwise, <see cref="F:System.TimeSpan.Zero" />. </returns>
	protected virtual TimeSpan GetCurrentTimeCore()
	{
		if (!_currentTime.HasValue)
		{
			return TimeSpan.Zero;
		}
		return _currentTime.Value;
	}

	/// <summary>When implemented in a derived class, will be invoked whenever a clock begins, skips, pauses, resumes, or when the clock's <see cref="P:System.Windows.Media.Animation.ClockController.SpeedRatio" /> is modified.</summary>
	protected virtual void SpeedChanged()
	{
	}

	/// <summary>When implemented in a derived class, will be invoked whenever a clock is stopped using the <see cref="M:System.Windows.Media.Animation.ClockController.Stop" /> method.</summary>
	protected virtual void Stopped()
	{
	}

	internal virtual void AddNullPointToCurrentIntervals()
	{
	}

	internal static Clock AllocateClock(Timeline timeline, bool hasControllableRoot)
	{
		Clock clock = timeline.AllocateClock();
		ClockGroup clockGroup = clock as ClockGroup;
		if (clock._parent != null || (clockGroup != null && clockGroup.InternalChildren != null))
		{
			throw new InvalidOperationException(SR.Format(SR.Timing_CreateClockMustReturnNewClock, timeline.GetType().Name));
		}
		clock.SetFlag(ClockFlags.HasControllableRoot, hasControllableRoot);
		return clock;
	}

	internal virtual void BuildClockSubTreeFromTimeline(Timeline timeline, bool hasControllableRoot)
	{
		SetFlag(ClockFlags.CanSlip, GetCanSlip());
		if (!CanSlip || (!IsRoot && !_timeline.BeginTime.HasValue))
		{
			return;
		}
		ResolveDuration();
		if (_resolvedDuration.HasTimeSpan && !(_resolvedDuration.TimeSpan > TimeSpan.Zero))
		{
			return;
		}
		if (_timeline.AutoReverse || _timeline.AccelerationRatio > 0.0 || _timeline.DecelerationRatio > 0.0)
		{
			throw new NotSupportedException(SR.Timing_CanSlipOnlyOnSimpleTimelines);
		}
		_syncData = new SyncData(this);
		HasDescendantsWithUnresolvedDuration = !HasResolvedDuration;
		for (Clock parent = _parent; parent != null; parent = parent._parent)
		{
			if (parent._timeline.AutoReverse || parent._timeline.AccelerationRatio > 0.0 || parent._timeline.DecelerationRatio > 0.0)
			{
				throw new InvalidOperationException(SR.Timing_SlipBehavior_SyncOnlyWithSimpleParents);
			}
			parent.SetFlag(ClockFlags.CanGrow, value: true);
			if (!HasResolvedDuration)
			{
				parent.HasDescendantsWithUnresolvedDuration = true;
			}
			parent._currentIterationBeginTime = parent._beginTime;
		}
	}

	internal static Clock BuildClockTreeFromTimeline(Timeline rootTimeline, bool hasControllableRoot)
	{
		Clock clock = AllocateClock(rootTimeline, hasControllableRoot);
		clock.IsRoot = true;
		clock._rootData = new RootData();
		clock.BuildClockSubTreeFromTimeline(clock.Timeline, hasControllableRoot);
		clock.AddToTimeManager();
		return clock;
	}

	internal virtual void ClearCurrentIntervalsToNull()
	{
	}

	internal void ClipNextTickByParent()
	{
		if (!IsTimeManager && !_parent.IsTimeManager && (!InternalNextTickNeededTime.HasValue || (_parent.InternalNextTickNeededTime.HasValue && _parent.InternalNextTickNeededTime.Value < InternalNextTickNeededTime.Value)))
		{
			InternalNextTickNeededTime = _parent.InternalNextTickNeededTime;
		}
	}

	internal virtual void ComputeCurrentIntervals(TimeIntervalCollection parentIntervalCollection, TimeSpan beginTime, TimeSpan? endTime, Duration fillDuration, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
	}

	internal virtual void ComputeCurrentFillInterval(TimeIntervalCollection parentIntervalCollection, TimeSpan beginTime, TimeSpan endTime, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
	}

	internal void ComputeLocalState()
	{
		ClockState currentClockState = _currentClockState;
		_ = _currentTime;
		double? currentGlobalSpeed = _currentGlobalSpeed;
		_ = _currentProgress;
		_ = _currentIteration;
		PauseStateChangedDuringTick = false;
		ComputeLocalStateHelper(performTickOperations: true, seekedAlignedToLastTick: false);
		if (currentClockState != _currentClockState)
		{
			RaiseCurrentStateInvalidated();
			RaiseCurrentGlobalSpeedInvalidated();
			RaiseCurrentTimeInvalidated();
		}
		if (_currentGlobalSpeed != currentGlobalSpeed)
		{
			RaiseCurrentGlobalSpeedInvalidated();
		}
		if (HasDiscontinuousTimeMovementOccured)
		{
			DiscontinuousTimeMovement();
			HasDiscontinuousTimeMovementOccured = false;
		}
	}

	internal void InternalBegin()
	{
		InternalSeek(TimeSpan.Zero);
	}

	internal double InternalGetSpeedRatio()
	{
		return _rootData.InteractiveSpeedRatio;
	}

	internal void InternalPause()
	{
		if (PendingInteractiveResume)
		{
			PendingInteractiveResume = false;
		}
		else if (!IsInteractivelyPaused)
		{
			PendingInteractivePause = true;
		}
		NotifyNewEarliestFutureActivity();
	}

	internal void InternalRemove()
	{
		PendingInteractiveRemove = true;
		InternalStop();
	}

	internal void InternalResume()
	{
		if (PendingInteractivePause)
		{
			PendingInteractivePause = false;
		}
		else if (IsInteractivelyPaused)
		{
			PendingInteractiveResume = true;
		}
		NotifyNewEarliestFutureActivity();
	}

	internal void InternalSeek(TimeSpan destination)
	{
		IsInteractivelyStopped = false;
		PendingInteractiveStop = false;
		ResetNodesWithSlip();
		_rootData.PendingSeekDestination = destination;
		RootBeginPending = false;
		NotifyNewEarliestFutureActivity();
	}

	internal void InternalSeekAlignedToLastTick(TimeSpan destination)
	{
		if (_timeManager == null || HasDescendantsWithUnresolvedDuration)
		{
			return;
		}
		_beginTime = CurrentGlobalTime - DivideTimeSpan(destination, _appliedSpeedRatio);
		if (CanGrow)
		{
			_currentIteration = null;
			_currentIterationBeginTime = _beginTime;
			ResetSlipOnSubtree();
			UpdateSyncBeginTime();
		}
		IsInteractivelyStopped = false;
		PendingInteractiveStop = false;
		RootBeginPending = false;
		ResetNodesWithSlip();
		_timeManager.InternalCurrentIntervals = TimeIntervalCollection.Empty;
		PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
		while (prefixSubtreeEnumerator.MoveNext())
		{
			prefixSubtreeEnumerator.Current.ComputeLocalStateHelper(performTickOperations: false, seekedAlignedToLastTick: true);
			if (HasDiscontinuousTimeMovementOccured)
			{
				DiscontinuousTimeMovement();
				HasDiscontinuousTimeMovementOccured = false;
			}
			prefixSubtreeEnumerator.Current.ClipNextTickByParent();
			prefixSubtreeEnumerator.Current.NeedsPostfixTraversal = prefixSubtreeEnumerator.Current is ClockGroup;
		}
		_parent.ComputeTreeStateRoot();
		prefixSubtreeEnumerator.Reset();
		while (prefixSubtreeEnumerator.MoveNext())
		{
			prefixSubtreeEnumerator.Current.CurrentTimeInvalidatedEventRaised = true;
			prefixSubtreeEnumerator.Current.CurrentStateInvalidatedEventRaised = true;
			prefixSubtreeEnumerator.Current.CurrentGlobalSpeedInvalidatedEventRaised = true;
			prefixSubtreeEnumerator.Current.RaiseAccumulatedEvents();
		}
	}

	internal void InternalSetSpeedRatio(double ratio)
	{
		_rootData.PendingSpeedRatio = ratio;
	}

	internal void InternalSkipToFill()
	{
		TimeSpan? timeSpan = ComputeEffectiveDuration();
		if (!timeSpan.HasValue)
		{
			throw new InvalidOperationException(SR.Timing_SkipToFillDestinationIndefinite);
		}
		IsInteractivelyStopped = false;
		PendingInteractiveStop = false;
		ResetNodesWithSlip();
		RootBeginPending = false;
		_rootData.PendingSeekDestination = timeSpan.Value;
		NotifyNewEarliestFutureActivity();
	}

	internal void InternalStop()
	{
		PendingInteractiveStop = true;
		_rootData.PendingSeekDestination = null;
		RootBeginPending = false;
		ResetNodesWithSlip();
		NotifyNewEarliestFutureActivity();
	}

	internal void RaiseAccumulatedEvents()
	{
		try
		{
			if (CurrentTimeInvalidatedEventRaised)
			{
				FireCurrentTimeInvalidatedEvent();
			}
			if (CurrentGlobalSpeedInvalidatedEventRaised)
			{
				FireCurrentGlobalSpeedInvalidatedEvent();
				SpeedChanged();
			}
			if (CurrentStateInvalidatedEventRaised)
			{
				FireCurrentStateInvalidatedEvent();
				if (!CurrentGlobalSpeedInvalidatedEventRaised)
				{
					DiscontinuousTimeMovement();
				}
			}
			if (CompletedEventRaised)
			{
				FireCompletedEvent();
			}
			if (RemoveRequestedEventRaised)
			{
				FireRemoveRequestedEvent();
			}
		}
		finally
		{
			CurrentTimeInvalidatedEventRaised = false;
			CurrentGlobalSpeedInvalidatedEventRaised = false;
			CurrentStateInvalidatedEventRaised = false;
			CompletedEventRaised = false;
			RemoveRequestedEventRaised = false;
			IsInEventQueue = false;
		}
	}

	internal void RaiseCompleted()
	{
		CompletedEventRaised = true;
		if (!IsInEventQueue)
		{
			_timeManager.AddToEventQueue(this);
			IsInEventQueue = true;
		}
	}

	internal void RaiseCurrentGlobalSpeedInvalidated()
	{
		CurrentGlobalSpeedInvalidatedEventRaised = true;
		if (!IsInEventQueue)
		{
			_timeManager.AddToEventQueue(this);
			IsInEventQueue = true;
		}
	}

	internal void RaiseCurrentStateInvalidated()
	{
		if (_currentClockState == ClockState.Stopped)
		{
			Stopped();
		}
		CurrentStateInvalidatedEventRaised = true;
		if (!IsInEventQueue)
		{
			_timeManager.AddToEventQueue(this);
			IsInEventQueue = true;
		}
	}

	internal void RaiseCurrentTimeInvalidated()
	{
		CurrentTimeInvalidatedEventRaised = true;
		if (!IsInEventQueue)
		{
			_timeManager.AddToEventQueue(this);
			IsInEventQueue = true;
		}
	}

	internal void RaiseRemoveRequested()
	{
		RemoveRequestedEventRaised = true;
		if (!IsInEventQueue)
		{
			_timeManager.AddToEventQueue(this);
			IsInEventQueue = true;
		}
	}

	internal void ResetCachedStateToStopped()
	{
		_currentGlobalSpeed = null;
		_currentIteration = null;
		IsBackwardsProgressingGlobal = false;
		_currentProgress = null;
		_currentTime = null;
		_currentClockState = ClockState.Stopped;
	}

	internal virtual void ResetNodesWithSlip()
	{
		if (_syncData != null)
		{
			_syncData.IsInSyncPeriod = false;
		}
	}

	internal virtual void UpdateDescendantsWithUnresolvedDuration()
	{
		if (HasResolvedDuration)
		{
			HasDescendantsWithUnresolvedDuration = false;
		}
	}

	private void AdjustBeginTime()
	{
		if (_rootData.PendingSeekDestination.HasValue && !HasDescendantsWithUnresolvedDuration)
		{
			_beginTime = CurrentGlobalTime - DivideTimeSpan(_rootData.PendingSeekDestination.Value, _appliedSpeedRatio);
			if (CanGrow)
			{
				_currentIterationBeginTime = _beginTime;
				_currentIteration = null;
				ResetSlipOnSubtree();
			}
			UpdateSyncBeginTime();
			_rootData.PendingSeekDestination = null;
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				prefixSubtreeEnumerator.Current.RaiseCurrentStateInvalidated();
				prefixSubtreeEnumerator.Current.RaiseCurrentTimeInvalidated();
				prefixSubtreeEnumerator.Current.RaiseCurrentGlobalSpeedInvalidated();
			}
		}
		else if (RootBeginPending)
		{
			TimeSpan currentGlobalTime = CurrentGlobalTime;
			TimeSpan? beginTime = _timeline.BeginTime;
			_beginTime = currentGlobalTime + beginTime;
			if (CanGrow)
			{
				_currentIterationBeginTime = _beginTime;
			}
			UpdateSyncBeginTime();
			RootBeginPending = false;
		}
		else if ((IsInteractivelyPaused || _rootData.InteractiveSpeedRatio == 0.0) && (_syncData == null || !_syncData.IsInSyncPeriod) && _beginTime.HasValue)
		{
			_beginTime += _timeManager.LastTickDelta;
			UpdateSyncBeginTime();
			if (_currentIterationBeginTime.HasValue)
			{
				_currentIterationBeginTime += _timeManager.LastTickDelta;
			}
		}
		if (_rootData.PendingSpeedRatio.HasValue)
		{
			double num = _rootData.PendingSpeedRatio.Value * _timeline.SpeedRatio;
			if (num == 0.0)
			{
				num = 1.0;
			}
			TimeSpan currentGlobalTime2 = CurrentGlobalTime;
			if (_currentIterationBeginTime.HasValue)
			{
				_currentIterationBeginTime = currentGlobalTime2 - MultiplyTimeSpan(currentGlobalTime2 - _currentIterationBeginTime.Value, _appliedSpeedRatio / num);
			}
			else
			{
				_beginTime = currentGlobalTime2 - MultiplyTimeSpan(currentGlobalTime2 - _beginTime.Value, _appliedSpeedRatio / num);
			}
			RaiseCurrentGlobalSpeedInvalidated();
			_appliedSpeedRatio = num;
			_rootData.InteractiveSpeedRatio = _rootData.PendingSpeedRatio.Value;
			_rootData.PendingSpeedRatio = null;
			UpdateSyncBeginTime();
		}
	}

	internal void ApplyDesiredFrameRateToGlobalTime()
	{
		if (HasDesiredFrameRate)
		{
			_rootData.LastAdjustedGlobalTime = _rootData.CurrentAdjustedGlobalTime;
			_rootData.CurrentAdjustedGlobalTime = GetCurrentDesiredFrameTime(_timeManager.InternalCurrentGlobalTime);
		}
	}

	internal void ApplyDesiredFrameRateToNextTick()
	{
		if (HasDesiredFrameRate && InternalNextTickNeededTime.HasValue)
		{
			TimeSpan time = ((InternalNextTickNeededTime == TimeSpan.Zero) ? _rootData.CurrentAdjustedGlobalTime : InternalNextTickNeededTime.Value);
			InternalNextTickNeededTime = GetNextDesiredFrameTime(time);
		}
	}

	private bool ComputeCurrentIteration(TimeSpan parentTime, double parentSpeed, TimeSpan? expirationTime, out TimeSpan localProgress)
	{
		RepeatBehavior repeatBehavior = _timeline.RepeatBehavior;
		TimeSpan timeSpan = (_currentIterationBeginTime.HasValue ? _currentIterationBeginTime.Value : _beginTime.Value);
		TimeSpan timeSpan2 = MultiplyTimeSpan(parentTime - timeSpan, _appliedSpeedRatio);
		IsBackwardsProgressingGlobal = _parent.IsBackwardsProgressingGlobal;
		if (_currentDuration.HasTimeSpan)
		{
			if (_currentDuration.TimeSpan == TimeSpan.Zero)
			{
				localProgress = TimeSpan.Zero;
				_currentTime = TimeSpan.Zero;
				double num;
				if (repeatBehavior.HasCount)
				{
					double count = repeatBehavior.Count;
					if (count <= 1.0)
					{
						num = count;
						_currentIteration = 1;
					}
					else
					{
						double num2 = (int)count;
						if (count == num2)
						{
							num = 1.0;
							_currentIteration = (int)count;
						}
						else
						{
							num = count - num2;
							_currentIteration = (int)(count + 1.0);
						}
					}
				}
				else
				{
					_currentIteration = 1;
					num = 1.0;
				}
				if (_timeline.AutoReverse)
				{
					num = ((num == 1.0) ? 0.0 : ((!(num < 0.5)) ? (1.0 - (num - 0.5) * 2.0) : (num * 2.0)));
				}
				_currentProgress = num;
				return true;
			}
			if (_currentClockState == ClockState.Filling && repeatBehavior.HasCount && !_currentIterationBeginTime.HasValue)
			{
				double num3 = repeatBehavior.Count;
				if (_timeline.AutoReverse)
				{
					num3 *= 2.0;
				}
				timeSpan2 = MultiplyTimeSpan(_resolvedDuration.TimeSpan, num3);
			}
			int newIteration;
			if (_currentIterationBeginTime.HasValue)
			{
				ComputeCurrentIterationWithGrow(parentTime, expirationTime, out localProgress, out newIteration);
			}
			else
			{
				localProgress = TimeSpan.FromTicks(timeSpan2.Ticks % _currentDuration.TimeSpan.Ticks);
				newIteration = (int)(timeSpan2.Ticks / _resolvedDuration.TimeSpan.Ticks);
			}
			if (localProgress == TimeSpan.Zero && newIteration > 0 && (_currentClockState == ClockState.Filling || _parent.IsBackwardsProgressingGlobal))
			{
				localProgress = _currentDuration.TimeSpan;
				newIteration--;
			}
			if (_timeline.AutoReverse)
			{
				if ((newIteration & 1) == 1)
				{
					if (localProgress == TimeSpan.Zero)
					{
						InternalNextTickNeededTime = TimeSpan.Zero;
					}
					localProgress = _currentDuration.TimeSpan - localProgress;
					IsBackwardsProgressingGlobal = !IsBackwardsProgressingGlobal;
					parentSpeed = 0.0 - parentSpeed;
				}
				newIteration /= 2;
			}
			_currentIteration = 1 + newIteration;
			if (_currentClockState == ClockState.Active && parentSpeed != 0.0 && !NeedsTicksWhenActive)
			{
				TimeSpan timeSpan3;
				if (localProgress == TimeSpan.Zero)
				{
					timeSpan3 = DivideTimeSpan(_currentDuration.TimeSpan, Math.Abs(parentSpeed));
				}
				else if (parentSpeed > 0.0)
				{
					TimeSpan timeSpan4 = MultiplyTimeSpan(_currentDuration.TimeSpan, 1.0 - _timeline.DecelerationRatio);
					timeSpan3 = DivideTimeSpan(timeSpan4 - localProgress, parentSpeed);
				}
				else
				{
					TimeSpan timeSpan5 = MultiplyTimeSpan(_currentDuration.TimeSpan, _timeline.AccelerationRatio);
					timeSpan3 = DivideTimeSpan(timeSpan5 - localProgress, parentSpeed);
				}
				TimeSpan timeSpan6 = CurrentGlobalTime + timeSpan3;
				if (!InternalNextTickNeededTime.HasValue || timeSpan6 < InternalNextTickNeededTime.Value)
				{
					InternalNextTickNeededTime = timeSpan6;
				}
			}
		}
		else
		{
			localProgress = timeSpan2;
			_currentIteration = 1;
		}
		return false;
	}

	private void ComputeCurrentIterationWithGrow(TimeSpan parentTime, TimeSpan? expirationTime, out TimeSpan localProgress, out int newIteration)
	{
		TimeSpan timeSpan = MultiplyTimeSpan(parentTime - _currentIterationBeginTime.Value, _appliedSpeedRatio);
		int num;
		if (timeSpan < _currentDuration.TimeSpan)
		{
			localProgress = timeSpan;
			num = 0;
		}
		else
		{
			long ticks = (timeSpan - _currentDuration.TimeSpan).Ticks;
			localProgress = TimeSpan.FromTicks(ticks % _resolvedDuration.TimeSpan.Ticks);
			num = 1 + (int)(ticks / _resolvedDuration.TimeSpan.Ticks);
			_currentIterationBeginTime += _currentDuration.TimeSpan + MultiplyTimeSpan(_resolvedDuration.TimeSpan, num - 1);
			if (_currentClockState == ClockState.Filling && expirationTime.HasValue && _currentIterationBeginTime >= expirationTime)
			{
				if (num > 1)
				{
					_currentIterationBeginTime -= _resolvedDuration.TimeSpan;
				}
				else
				{
					_currentIterationBeginTime -= _currentDuration.TimeSpan;
				}
			}
			else
			{
				ResetSlipOnSubtree();
			}
		}
		newIteration = (_currentIteration.HasValue ? (num + (_currentIteration.Value - 1)) : num);
	}

	private bool ComputeCurrentState(TimeSpan? expirationTime, ref TimeSpan parentTime, double parentSpeed, bool isInTick)
	{
		FillBehavior fillBehavior = _timeline.FillBehavior;
		TimeSpan value = parentTime;
		TimeSpan? beginTime = _beginTime;
		if (value < beginTime)
		{
			ResetCachedStateToStopped();
			return true;
		}
		if (expirationTime.HasValue)
		{
			value = parentTime;
			beginTime = expirationTime;
			if (value >= beginTime)
			{
				RaiseCompletedForRoot(isInTick);
				if (fillBehavior == FillBehavior.HoldEnd)
				{
					ResetCachedStateToFilling();
					parentTime = expirationTime.Value;
					goto IL_0099;
				}
				ResetCachedStateToStopped();
				return true;
			}
		}
		_currentClockState = ClockState.Active;
		goto IL_0099;
		IL_0099:
		if (parentSpeed != 0.0 && _currentClockState == ClockState.Active && NeedsTicksWhenActive)
		{
			InternalNextTickNeededTime = TimeSpan.Zero;
		}
		return false;
	}

	private bool ComputeCurrentSpeed(double localSpeed)
	{
		if (IsInteractivelyPaused)
		{
			_currentGlobalSpeed = 0.0;
		}
		else
		{
			localSpeed *= _appliedSpeedRatio;
			if (IsBackwardsProgressingGlobal)
			{
				localSpeed = 0.0 - localSpeed;
			}
			_currentGlobalSpeed = localSpeed * _parent._currentGlobalSpeed;
		}
		return false;
	}

	private bool ComputeCurrentTime(TimeSpan localProgress, out double localSpeed)
	{
		if (_currentDuration.HasTimeSpan)
		{
			double accelerationRatio = _timeline.AccelerationRatio;
			double decelerationRatio = _timeline.DecelerationRatio;
			double num = accelerationRatio + decelerationRatio;
			double num2 = _currentDuration.TimeSpan.Ticks;
			double num3 = (double)localProgress.Ticks / num2;
			if (num == 0.0)
			{
				localSpeed = 1.0;
				_currentTime = localProgress;
			}
			else
			{
				double num4 = 2.0 / (2.0 - num);
				if (num3 < accelerationRatio)
				{
					localSpeed = num4 * num3 / accelerationRatio;
					num3 = num4 * num3 * num3 / (2.0 * accelerationRatio);
					if (_currentClockState == ClockState.Active && _parent._currentClockState == ClockState.Active)
					{
						InternalNextTickNeededTime = TimeSpan.Zero;
					}
				}
				else if (num3 <= 1.0 - decelerationRatio)
				{
					localSpeed = num4;
					num3 = num4 * (num3 - accelerationRatio / 2.0);
				}
				else
				{
					double num5 = 1.0 - num3;
					localSpeed = num4 * num5 / decelerationRatio;
					num3 = 1.0 - num4 * num5 * num5 / (2.0 * decelerationRatio);
					if (_currentClockState == ClockState.Active && _parent._currentClockState == ClockState.Active)
					{
						InternalNextTickNeededTime = TimeSpan.Zero;
					}
				}
				_currentTime = TimeSpan.FromTicks((long)(num3 * num2 + 0.5));
			}
			_currentProgress = num3;
		}
		else
		{
			_currentTime = localProgress;
			_currentProgress = 0.0;
			localSpeed = 1.0;
		}
		return _currentClockState != ClockState.Active;
	}

	private void ResolveDuration()
	{
		if (!HasResolvedDuration)
		{
			Duration naturalDuration = NaturalDuration;
			if (naturalDuration != Duration.Automatic)
			{
				_resolvedDuration = naturalDuration;
				_currentDuration = naturalDuration;
				HasResolvedDuration = true;
			}
		}
		if (CanGrow)
		{
			_currentDuration = CurrentDuration;
			if (_currentDuration == Duration.Automatic)
			{
				_currentDuration = Duration.Forever;
			}
		}
		if (HasDescendantsWithUnresolvedDuration)
		{
			UpdateDescendantsWithUnresolvedDuration();
		}
	}

	private TimeSpan? ComputeEffectiveDuration()
	{
		ResolveDuration();
		RepeatBehavior repeatBehavior = _timeline.RepeatBehavior;
		TimeSpan? result;
		if (_currentDuration.HasTimeSpan && _currentDuration.TimeSpan == TimeSpan.Zero)
		{
			result = TimeSpan.Zero;
		}
		else if (!repeatBehavior.HasCount)
		{
			result = ((!repeatBehavior.HasDuration) ? ((TimeSpan?)null) : new TimeSpan?(repeatBehavior.Duration));
		}
		else if (repeatBehavior.Count == 0.0)
		{
			result = TimeSpan.Zero;
		}
		else if (_currentDuration == Duration.Forever)
		{
			result = null;
		}
		else if (!CanGrow)
		{
			double num = repeatBehavior.Count / _appliedSpeedRatio;
			if (_timeline.AutoReverse)
			{
				num *= 2.0;
			}
			result = MultiplyTimeSpan(_currentDuration.TimeSpan, num);
		}
		else
		{
			TimeSpan timeSpan = TimeSpan.Zero;
			double num2 = repeatBehavior.Count;
			if (CanGrow && _currentIterationBeginTime.HasValue && _currentIteration.HasValue)
			{
				num2 -= (double)(_currentIteration.Value - 1);
				timeSpan = _currentIterationBeginTime.Value - _beginTime.Value;
			}
			double num3 = ((!(num2 <= 1.0)) ? ((double)_currentDuration.TimeSpan.Ticks + (double)_resolvedDuration.TimeSpan.Ticks * (num2 - 1.0)) : ((double)_currentDuration.TimeSpan.Ticks * num2));
			if (_timeline.AutoReverse)
			{
				num3 *= 2.0;
			}
			result = TimeSpan.FromTicks((long)(num3 / _appliedSpeedRatio + 0.5)) + timeSpan;
		}
		return result;
	}

	private void ComputeEvents(TimeSpan? expirationTime, TimeIntervalCollection parentIntervalCollection)
	{
		ClearCurrentIntervalsToNull();
		if (_beginTime.HasValue && !(IsInteractivelyPaused ^ PauseStateChangedDuringTick))
		{
			Duration duration = ((!expirationTime.HasValue) ? ((Duration)TimeSpan.Zero) : Duration.Forever);
			if (!expirationTime.HasValue || expirationTime >= _beginTime)
			{
				TimeIntervalCollection timeIntervalCollection = ((!expirationTime.HasValue) ? TimeIntervalCollection.CreateInfiniteClosedInterval(_beginTime.Value) : ((!(expirationTime == _beginTime)) ? TimeIntervalCollection.CreateClosedOpenInterval(_beginTime.Value, expirationTime.Value) : TimeIntervalCollection.Empty));
				if (parentIntervalCollection.Intersects(timeIntervalCollection))
				{
					ComputeIntervalsWithParentIntersection(parentIntervalCollection, timeIntervalCollection, expirationTime, duration);
				}
				else if (duration != TimeSpan.Zero && _timeline.FillBehavior == FillBehavior.HoldEnd)
				{
					ComputeIntervalsWithHoldEnd(parentIntervalCollection, expirationTime);
				}
			}
		}
		if (PendingInteractiveRemove)
		{
			RaiseRemoveRequestedForRoot();
			RaiseCompletedForRoot(isInTick: true);
			PendingInteractiveRemove = false;
		}
	}

	private bool ComputeExpirationTime(out TimeSpan? expirationTime)
	{
		if (!_beginTime.HasValue)
		{
			expirationTime = null;
			return true;
		}
		TimeSpan? timeSpan = ComputeEffectiveDuration();
		if (timeSpan.HasValue)
		{
			expirationTime = _beginTime + timeSpan;
			if (_syncData != null && _syncData.IsInSyncPeriod && !_syncData.SyncClockHasReachedEffectiveDuration)
			{
				expirationTime += TimeSpan.FromMilliseconds(50.0);
			}
		}
		else
		{
			expirationTime = null;
		}
		return false;
	}

	private bool ComputeInteractiveValues()
	{
		bool result = false;
		if (PendingInteractiveStop)
		{
			PendingInteractiveStop = false;
			IsInteractivelyStopped = true;
			_beginTime = null;
			_currentIterationBeginTime = null;
			if (CanGrow)
			{
				ResetSlipOnSubtree();
			}
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				Clock current = prefixSubtreeEnumerator.Current;
				if (current._currentClockState != ClockState.Stopped)
				{
					current.ResetCachedStateToStopped();
					current.RaiseCurrentStateInvalidated();
					current.RaiseCurrentTimeInvalidated();
					current.RaiseCurrentGlobalSpeedInvalidated();
				}
				else
				{
					prefixSubtreeEnumerator.SkipSubtree();
				}
			}
		}
		if (IsInteractivelyStopped)
		{
			ResetCachedStateToStopped();
			InternalNextTickNeededTime = null;
			result = true;
		}
		else
		{
			AdjustBeginTime();
		}
		if (PendingInteractivePause)
		{
			PendingInteractivePause = false;
			RaiseCurrentGlobalSpeedInvalidated();
			PrefixSubtreeEnumerator prefixSubtreeEnumerator2 = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator2.MoveNext())
			{
				prefixSubtreeEnumerator2.Current.IsInteractivelyPaused = true;
				prefixSubtreeEnumerator2.Current.PauseStateChangedDuringTick = true;
			}
		}
		if (PendingInteractiveResume)
		{
			PendingInteractiveResume = false;
			if (_currentClockState != ClockState.Filling)
			{
				RaiseCurrentGlobalSpeedInvalidated();
			}
			PrefixSubtreeEnumerator prefixSubtreeEnumerator3 = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator3.MoveNext())
			{
				prefixSubtreeEnumerator3.Current.IsInteractivelyPaused = false;
				prefixSubtreeEnumerator3.Current.PauseStateChangedDuringTick = true;
			}
		}
		return result;
	}

	private void ComputeIntervalsWithHoldEnd(TimeIntervalCollection parentIntervalCollection, TimeSpan? endOfActivePeriod)
	{
		TimeIntervalCollection other = TimeIntervalCollection.CreateInfiniteClosedInterval(endOfActivePeriod.Value);
		if (parentIntervalCollection.Intersects(other))
		{
			TimeSpan beginTime = (_currentIterationBeginTime.HasValue ? _currentIterationBeginTime.Value : _beginTime.Value);
			ComputeCurrentFillInterval(parentIntervalCollection, beginTime, endOfActivePeriod.Value, _currentDuration, _appliedSpeedRatio, _timeline.AccelerationRatio, _timeline.DecelerationRatio, _timeline.AutoReverse);
			if (parentIntervalCollection.IntersectsInverseOf(other))
			{
				RaiseCurrentStateInvalidated();
				RaiseCurrentTimeInvalidated();
				RaiseCurrentGlobalSpeedInvalidated();
				AddNullPointToCurrentIntervals();
			}
		}
	}

	private void ComputeIntervalsWithParentIntersection(TimeIntervalCollection parentIntervalCollection, TimeIntervalCollection activePeriod, TimeSpan? endOfActivePeriod, Duration postFillDuration)
	{
		TimeSpan beginTime = (_currentIterationBeginTime.HasValue ? _currentIterationBeginTime.Value : _beginTime.Value);
		RaiseCurrentTimeInvalidated();
		if (parentIntervalCollection.IntersectsInverseOf(activePeriod))
		{
			RaiseCurrentStateInvalidated();
			RaiseCurrentGlobalSpeedInvalidated();
		}
		else if (parentIntervalCollection.IntersectsPeriodicCollection(beginTime, _currentDuration, _appliedSpeedRatio, _timeline.AccelerationRatio, _timeline.DecelerationRatio, _timeline.AutoReverse))
		{
			RaiseCurrentGlobalSpeedInvalidated();
		}
		else if (parentIntervalCollection.IntersectsMultiplePeriods(beginTime, _currentDuration, _appliedSpeedRatio))
		{
			HasDiscontinuousTimeMovementOccured = true;
			if (_syncData != null)
			{
				_syncData.SyncClockDiscontinuousEvent = true;
			}
		}
		ComputeCurrentIntervals(parentIntervalCollection, beginTime, endOfActivePeriod, postFillDuration, _currentDuration, _appliedSpeedRatio, _timeline.AccelerationRatio, _timeline.DecelerationRatio, _timeline.AutoReverse);
	}

	private void ComputeLocalStateHelper(bool performTickOperations, bool seekedAlignedToLastTick)
	{
		bool flag = false;
		if (ComputeParentParameters(out var parentTime, out var parentSpeed, out var parentIntervalCollection, seekedAlignedToLastTick))
		{
			flag = true;
		}
		if (_syncData != null && _syncData.IsInSyncPeriod && _parent.CurrentState != ClockState.Stopped)
		{
			ComputeSyncSlip(ref parentIntervalCollection, parentTime.Value, parentSpeed.Value);
		}
		ResolveDuration();
		if (performTickOperations && IsRoot && ComputeInteractiveValues())
		{
			flag = true;
		}
		if (_syncData != null && !_syncData.IsInSyncPeriod && _parent.CurrentState != ClockState.Stopped && (!parentIntervalCollection.IsEmptyOfRealPoints || HasSeekOccuredAfterLastTick))
		{
			ComputeSyncEnter(ref parentIntervalCollection, parentTime.Value);
		}
		if (ComputeExpirationTime(out var expirationTime))
		{
			flag = true;
		}
		if (performTickOperations)
		{
			ComputeEvents(expirationTime, parentIntervalCollection);
		}
		if (!flag)
		{
			TimeSpan parentTime2 = parentTime.Value;
			if (!ComputeNextTickNeededTime(expirationTime, parentTime2, parentSpeed.Value) && !ComputeCurrentState(expirationTime, ref parentTime2, parentSpeed.Value, performTickOperations) && !ComputeCurrentIteration(parentTime2, parentSpeed.Value, expirationTime, out var localProgress) && !ComputeCurrentTime(localProgress, out var localSpeed))
			{
				ComputeCurrentSpeed(localSpeed);
			}
		}
	}

	private bool ComputeNextTickNeededTime(TimeSpan? expirationTime, TimeSpan parentTime, double parentSpeed)
	{
		if (parentSpeed == 0.0)
		{
			InternalNextTickNeededTime = (IsInteractivelyPaused ? new TimeSpan?(TimeSpan.Zero) : ((TimeSpan?)null));
		}
		else
		{
			double factor = 1.0 / parentSpeed;
			TimeSpan? timeSpan = null;
			TimeSpan timeSpan2 = MultiplyTimeSpan(_beginTime.Value - parentTime, factor);
			if (timeSpan2 >= TimeSpan.Zero)
			{
				timeSpan = timeSpan2;
			}
			if (expirationTime.HasValue)
			{
				TimeSpan timeSpan3 = MultiplyTimeSpan(expirationTime.Value - parentTime, factor);
				if (timeSpan3 >= TimeSpan.Zero && (!timeSpan.HasValue || timeSpan3 < timeSpan.Value))
				{
					timeSpan = timeSpan3;
				}
			}
			if (timeSpan.HasValue)
			{
				TimeSpan currentGlobalTime = CurrentGlobalTime;
				TimeSpan? timeSpan4 = timeSpan;
				InternalNextTickNeededTime = currentGlobalTime + timeSpan4;
			}
			else
			{
				InternalNextTickNeededTime = null;
			}
		}
		return false;
	}

	private bool ComputeParentParameters(out TimeSpan? parentTime, out double? parentSpeed, out TimeIntervalCollection parentIntervalCollection, bool seekedAlignedToLastTick)
	{
		if (IsRoot)
		{
			HasSeekOccuredAfterLastTick = seekedAlignedToLastTick || _rootData.PendingSeekDestination.HasValue;
			if (_timeManager == null || _timeManager.InternalIsStopped)
			{
				ResetCachedStateToStopped();
				parentTime = null;
				parentSpeed = null;
				InternalNextTickNeededTime = TimeSpan.Zero;
				parentIntervalCollection = TimeIntervalCollection.Empty;
				return true;
			}
			parentSpeed = 1.0;
			parentIntervalCollection = _timeManager.InternalCurrentIntervals;
			if (HasDesiredFrameRate)
			{
				parentTime = _rootData.CurrentAdjustedGlobalTime;
				if (!parentIntervalCollection.IsEmptyOfRealPoints)
				{
					parentIntervalCollection = parentIntervalCollection.SetBeginningOfConnectedInterval(_rootData.LastAdjustedGlobalTime);
				}
			}
			else
			{
				parentTime = _timeManager.InternalCurrentGlobalTime;
			}
			return false;
		}
		HasSeekOccuredAfterLastTick = seekedAlignedToLastTick || _parent.HasSeekOccuredAfterLastTick;
		parentTime = _parent._currentTime;
		parentSpeed = _parent._currentGlobalSpeed;
		parentIntervalCollection = _parent.CurrentIntervals;
		if (_parent._currentClockState != ClockState.Stopped)
		{
			return false;
		}
		if (_currentClockState != ClockState.Stopped)
		{
			RaiseCurrentStateInvalidated();
			RaiseCurrentGlobalSpeedInvalidated();
			RaiseCurrentTimeInvalidated();
		}
		ResetCachedStateToStopped();
		InternalNextTickNeededTime = null;
		return true;
	}

	private void ComputeSyncEnter(ref TimeIntervalCollection parentIntervalCollection, TimeSpan currentParentTimePT)
	{
		if (!_beginTime.HasValue || !(currentParentTimePT >= _beginTime.Value))
		{
			return;
		}
		TimeSpan timeSpan = (_currentIterationBeginTime.HasValue ? _currentIterationBeginTime.Value : _beginTime.Value);
		TimeSpan timeSpan2 = currentParentTimePT - timeSpan;
		TimeSpan timeSpan3 = MultiplyTimeSpan(timeSpan2, _appliedSpeedRatio);
		if (_syncData.SyncClock != this && !(timeSpan3 >= _syncData.SyncClockBeginTime))
		{
			return;
		}
		if (HasSeekOccuredAfterLastTick)
		{
			ComputeExpirationTime(out var expirationTime);
			if (expirationTime.HasValue && !(currentParentTimePT < expirationTime.Value))
			{
				return;
			}
			TimeSpan timeSpan4 = ((_syncData.SyncClock == this) ? timeSpan3 : MultiplyTimeSpan(timeSpan3 - _syncData.SyncClockBeginTime, _syncData.SyncClockSpeedRatio));
			TimeSpan? syncClockEffectiveDuration = _syncData.SyncClockEffectiveDuration;
			if (_syncData.SyncClock != this && syncClockEffectiveDuration.HasValue)
			{
				TimeSpan value = timeSpan4;
				TimeSpan? timeSpan5 = syncClockEffectiveDuration;
				if (!(value < timeSpan5))
				{
					return;
				}
			}
			Duration syncClockResolvedDuration = _syncData.SyncClockResolvedDuration;
			if (syncClockResolvedDuration.HasTimeSpan)
			{
				_syncData.PreviousSyncClockTime = TimeSpan.FromTicks(timeSpan4.Ticks % syncClockResolvedDuration.TimeSpan.Ticks);
				_syncData.PreviousRepeatTime = timeSpan4 - _syncData.PreviousSyncClockTime;
			}
			else
			{
				if (!(syncClockResolvedDuration == Duration.Forever))
				{
					throw new InvalidOperationException(SR.Timing_SeekDestinationAmbiguousDueToSlip);
				}
				_syncData.PreviousSyncClockTime = timeSpan4;
				_syncData.PreviousRepeatTime = TimeSpan.Zero;
			}
			_syncData.IsInSyncPeriod = true;
			return;
		}
		TimeSpan? timeSpan6 = ((_syncData.SyncClock == this) ? new TimeSpan?(parentIntervalCollection.FirstNodeTime) : _currentTime);
		if (!timeSpan6.HasValue || _syncData.SyncClockDiscontinuousEvent || timeSpan6.Value <= _syncData.SyncClockBeginTime)
		{
			TimeSpan timeSpan7 = timeSpan2;
			if (_syncData.SyncClock != this)
			{
				timeSpan7 -= DivideTimeSpan(_syncData.SyncClockBeginTime, _appliedSpeedRatio);
			}
			if (_currentIterationBeginTime.HasValue)
			{
				_currentIterationBeginTime += timeSpan7;
			}
			else
			{
				_beginTime += timeSpan7;
			}
			UpdateSyncBeginTime();
			parentIntervalCollection = parentIntervalCollection.SlipBeginningOfConnectedInterval(timeSpan7);
			_syncData.IsInSyncPeriod = true;
			_syncData.PreviousSyncClockTime = TimeSpan.Zero;
			_syncData.PreviousRepeatTime = TimeSpan.Zero;
			_syncData.SyncClockDiscontinuousEvent = false;
		}
	}

	private void ComputeSyncSlip(ref TimeIntervalCollection parentIntervalCollection, TimeSpan currentParentTimePT, double currentParentSpeed)
	{
		TimeSpan timeSpan = (parentIntervalCollection.IsEmptyOfRealPoints ? currentParentTimePT : parentIntervalCollection.FirstNodeTime);
		TimeSpan timeSpan2 = currentParentTimePT - timeSpan;
		MultiplyTimeSpan(timeSpan2, _appliedSpeedRatio);
		TimeSpan currentTimeCore = _syncData.SyncClock.GetCurrentTimeCore();
		TimeSpan timeSpan3 = currentTimeCore - _syncData.PreviousSyncClockTime;
		if (timeSpan3 > TimeSpan.Zero)
		{
			TimeSpan? syncClockEffectiveDuration = _syncData.SyncClockEffectiveDuration;
			Duration syncClockResolvedDuration = _syncData.SyncClockResolvedDuration;
			if (syncClockEffectiveDuration.HasValue && _syncData.PreviousRepeatTime + currentTimeCore >= syncClockEffectiveDuration.Value)
			{
				_syncData.IsInSyncPeriod = false;
				_syncData.PreviousRepeatTime = TimeSpan.Zero;
				_syncData.SyncClockDiscontinuousEvent = false;
			}
			else if (syncClockResolvedDuration.HasTimeSpan && currentTimeCore >= syncClockResolvedDuration.TimeSpan)
			{
				_syncData.PreviousSyncClockTime = TimeSpan.Zero;
				_syncData.PreviousRepeatTime += syncClockResolvedDuration.TimeSpan;
			}
			else
			{
				_syncData.PreviousSyncClockTime = currentTimeCore;
			}
		}
		else
		{
			timeSpan3 = TimeSpan.Zero;
		}
		TimeSpan timeSpan4 = ((_syncData.SyncClock == this) ? timeSpan3 : DivideTimeSpan(timeSpan3, _syncData.SyncClockSpeedRatio));
		TimeSpan timeSpan5 = timeSpan2 - DivideTimeSpan(timeSpan4, _appliedSpeedRatio);
		if (_currentIterationBeginTime.HasValue)
		{
			_currentIterationBeginTime += timeSpan5;
		}
		else
		{
			_beginTime += timeSpan5;
		}
		UpdateSyncBeginTime();
		parentIntervalCollection = parentIntervalCollection.SlipBeginningOfConnectedInterval(timeSpan5);
	}

	private void ResetSlipOnSubtree()
	{
		PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: false);
		while (prefixSubtreeEnumerator.MoveNext())
		{
			Clock current = prefixSubtreeEnumerator.Current;
			if (current._syncData != null)
			{
				current._syncData.IsInSyncPeriod = false;
				current._syncData.SyncClockDiscontinuousEvent = true;
			}
			if (current.CanSlip)
			{
				current._beginTime = current._timeline.BeginTime;
				current._currentIteration = null;
				current.UpdateSyncBeginTime();
				current.HasDiscontinuousTimeMovementOccured = true;
			}
			else if (current.CanGrow)
			{
				current._currentIterationBeginTime = current._beginTime;
				current._currentDuration = current._resolvedDuration;
			}
			else
			{
				prefixSubtreeEnumerator.SkipSubtree();
			}
		}
	}

	private void AddEventHandler(EventPrivateKey key, Delegate handler)
	{
		if (_eventHandlersStore == null)
		{
			_eventHandlersStore = new EventHandlersStore();
		}
		_eventHandlersStore.Add(key, handler);
		VerifyNeedsTicksWhenActive();
	}

	private void FireCompletedEvent()
	{
		FireEvent(Timeline.CompletedKey);
	}

	private void FireCurrentGlobalSpeedInvalidatedEvent()
	{
		FireEvent(Timeline.CurrentGlobalSpeedInvalidatedKey);
	}

	private void FireCurrentStateInvalidatedEvent()
	{
		FireEvent(Timeline.CurrentStateInvalidatedKey);
	}

	private void FireCurrentTimeInvalidatedEvent()
	{
		FireEvent(Timeline.CurrentTimeInvalidatedKey);
	}

	private void FireEvent(EventPrivateKey key)
	{
		if (_eventHandlersStore != null)
		{
			((EventHandler)_eventHandlersStore.Get(key))?.Invoke(this, null);
		}
	}

	private void FireRemoveRequestedEvent()
	{
		FireEvent(Timeline.RemoveRequestedKey);
	}

	private TimeSpan GetCurrentDesiredFrameTime(TimeSpan time)
	{
		return GetDesiredFrameTime(time, 0);
	}

	private TimeSpan GetDesiredFrameTime(TimeSpan time, int frameOffset)
	{
		long num = _rootData.DesiredFrameRate;
		return TimeSpan.FromTicks((time.Ticks * num / s_TimeSpanTicksPerSecond + frameOffset) * s_TimeSpanTicksPerSecond / num);
	}

	private TimeSpan GetNextDesiredFrameTime(TimeSpan time)
	{
		return GetDesiredFrameTime(time, 1);
	}

	private void RemoveEventHandler(EventPrivateKey key, Delegate handler)
	{
		if (_eventHandlersStore != null)
		{
			_eventHandlersStore.Remove(key, handler);
			if (_eventHandlersStore.Count == 0)
			{
				_eventHandlersStore = null;
			}
		}
		UpdateNeedsTicksWhenActive();
	}

	private void AddToTimeManager()
	{
		TimeManager timeManager = MediaContext.From(base.Dispatcher).TimeManager;
		if (timeManager != null)
		{
			_parent = timeManager.TimeManagerClock;
			SetTimeManager(_parent._timeManager);
			int? desiredFrameRate = Timeline.GetDesiredFrameRate(_timeline);
			if (desiredFrameRate.HasValue)
			{
				HasDesiredFrameRate = true;
				_rootData.DesiredFrameRate = desiredFrameRate.Value;
			}
			_parent.InternalRootChildren.Add(WeakReference);
			_subtreeFinalizer = new SubtreeFinalizer(_timeManager);
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				Clock current = prefixSubtreeEnumerator.Current;
				current._depth = current._parent._depth + 1;
			}
			if (IsInTimingTree)
			{
				_timeManager.SetDirty();
			}
			TimeIntervalCollection internalCurrentIntervals = TimeIntervalCollection.CreatePoint(_timeManager.InternalCurrentGlobalTime);
			internalCurrentIntervals.AddNullPoint();
			_timeManager.InternalCurrentIntervals = internalCurrentIntervals;
			_beginTime = null;
			_currentIterationBeginTime = null;
			prefixSubtreeEnumerator.Reset();
			while (prefixSubtreeEnumerator.MoveNext())
			{
				prefixSubtreeEnumerator.Current.ComputeLocalState();
				prefixSubtreeEnumerator.Current.ClipNextTickByParent();
				prefixSubtreeEnumerator.Current.NeedsPostfixTraversal = prefixSubtreeEnumerator.Current is ClockGroup;
			}
			_parent.ComputeTreeStateRoot();
			if (_timeline.BeginTime.HasValue)
			{
				RootBeginPending = true;
			}
			NotifyNewEarliestFutureActivity();
		}
	}

	private TimeSpan DivideTimeSpan(TimeSpan timeSpan, double factor)
	{
		return TimeSpan.FromTicks((long)((double)timeSpan.Ticks / factor + 0.5));
	}

	private bool GetFlag(ClockFlags flagMask)
	{
		return (_flags & flagMask) == flagMask;
	}

	private static TimeSpan MultiplyTimeSpan(TimeSpan timeSpan, double factor)
	{
		return TimeSpan.FromTicks((long)(factor * (double)timeSpan.Ticks + 0.5));
	}

	private void NotifyNewEarliestFutureActivity()
	{
		PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
		while (prefixSubtreeEnumerator.MoveNext())
		{
			prefixSubtreeEnumerator.Current.InternalNextTickNeededTime = TimeSpan.Zero;
		}
		Clock parent = _parent;
		while (parent != null && parent.InternalNextTickNeededTime != TimeSpan.Zero)
		{
			parent.InternalNextTickNeededTime = TimeSpan.Zero;
			if (parent.IsTimeManager)
			{
				_timeManager.NotifyNewEarliestFutureActivity();
				break;
			}
			parent = parent._parent;
		}
		if (_timeManager != null)
		{
			_timeManager.SetDirty();
		}
	}

	private void ResetCachedStateToFilling()
	{
		_currentGlobalSpeed = 0.0;
		IsBackwardsProgressingGlobal = false;
		_currentClockState = ClockState.Filling;
	}

	private void RaiseCompletedForRoot(bool isInTick)
	{
		if (IsRoot && (CurrentStateInvalidatedEventRaised || !isInTick))
		{
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				prefixSubtreeEnumerator.Current.RaiseCompleted();
			}
		}
	}

	private void RaiseRemoveRequestedForRoot()
	{
		PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
		while (prefixSubtreeEnumerator.MoveNext())
		{
			prefixSubtreeEnumerator.Current.RaiseRemoveRequested();
		}
	}

	private void SetFlag(ClockFlags flagMask, bool value)
	{
		if (value)
		{
			_flags |= flagMask;
		}
		else
		{
			_flags &= ~flagMask;
		}
	}

	private void SetTimeManager(TimeManager timeManager)
	{
		if (_timeManager == timeManager)
		{
			return;
		}
		PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(this, processRoot: true);
		while (prefixSubtreeEnumerator.MoveNext())
		{
			prefixSubtreeEnumerator.Current._timeManager = timeManager;
		}
		if (timeManager != null)
		{
			prefixSubtreeEnumerator.Reset();
			while (prefixSubtreeEnumerator.MoveNext())
			{
				_ = prefixSubtreeEnumerator.Current;
			}
		}
	}

	private void UpdateNeedsTicksWhenActive()
	{
		if (_eventHandlersStore == null)
		{
			NeedsTicksWhenActive = false;
		}
		else
		{
			NeedsTicksWhenActive = true;
		}
	}

	private void UpdateSyncBeginTime()
	{
		if (_syncData != null)
		{
			_syncData.UpdateClockBeginTime();
		}
	}

	private void VerifyNeedsTicksWhenActive()
	{
		if (!NeedsTicksWhenActive)
		{
			NeedsTicksWhenActive = true;
			NotifyNewEarliestFutureActivity();
		}
	}

	[Conditional("DEBUG")]
	private void Debug_VerifyOffsetFromBegin(long inputTime, long optimizedInputTime)
	{
		Math.Max(_appliedSpeedRatio, 1.0);
	}
}
