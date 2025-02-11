using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Animation;

internal sealed class TimeManager : DispatcherObject
{
	internal class GTCClock : IClock
	{
		TimeSpan IClock.CurrentTime => TimeSpan.FromTicks(DateTime.Now.Ticks);

		internal GTCClock()
		{
		}
	}

	internal class TestTimingClock : IClock
	{
		private TimeSpan _currentTime;

		public TimeSpan CurrentTime
		{
			get
			{
				return _currentTime;
			}
			set
			{
				_currentTime = value;
			}
		}
	}

	private TimeState _timeState;

	private TimeState _lastTimeState;

	private IClock _systemClock;

	private TimeSpan _globalTime;

	private TimeSpan _startTime;

	private TimeSpan _lastTickTime;

	private TimeSpan _pauseTime;

	private TimeIntervalCollection _currentTickInterval;

	private bool _nextTickTimeQueried;

	private bool _isDirty;

	private bool _isInTick;

	private bool _lockTickTime;

	private EventHandler _userNeedTickSooner;

	private ClockGroup _timeManagerClock;

	private Queue<WeakReference> _eventQueue;

	private bool _needClockCleanup;

	public IClock Clock
	{
		get
		{
			return _systemClock;
		}
		set
		{
			if (value != null)
			{
				_systemClock = value;
			}
			else if (MediaContext.IsClockSupported)
			{
				_systemClock = MediaContext.From(base.Dispatcher);
			}
			else
			{
				_systemClock = new GTCClock();
			}
		}
	}

	public TimeSpan? CurrentTime
	{
		get
		{
			if (_timeState == TimeState.Stopped)
			{
				return null;
			}
			return _globalTime;
		}
	}

	public bool IsDirty => _isDirty;

	internal TimeSpan InternalCurrentGlobalTime => _globalTime;

	internal bool InternalIsStopped => _timeState == TimeState.Stopped;

	internal TimeIntervalCollection InternalCurrentIntervals
	{
		get
		{
			return _currentTickInterval;
		}
		set
		{
			_currentTickInterval = value;
		}
	}

	internal TimeSpan LastTickDelta => _globalTime - _lastTickTime;

	internal TimeSpan LastTickTime => _lastTickTime;

	internal ClockGroup TimeManagerClock => _timeManagerClock;

	internal TimeState State => _timeState;

	internal event EventHandler NeedTickSooner
	{
		add
		{
			_userNeedTickSooner = (EventHandler)Delegate.Combine(_userNeedTickSooner, value);
		}
		remove
		{
			_userNeedTickSooner = (EventHandler)Delegate.Remove(_userNeedTickSooner, value);
		}
	}

	public TimeManager()
		: this(null)
	{
	}

	public TimeManager(IClock clock)
	{
		_eventQueue = new Queue<WeakReference>();
		Clock = clock;
		_timeState = TimeState.Stopped;
		_lastTimeState = TimeState.Stopped;
		_globalTime = new TimeSpan(-1L);
		_lastTickTime = new TimeSpan(-1L);
		_nextTickTimeQueried = false;
		_isInTick = false;
		ParallelTimeline parallelTimeline = new ParallelTimeline(new TimeSpan(0L), Duration.Forever);
		parallelTimeline.Freeze();
		_timeManagerClock = new ClockGroup(parallelTimeline);
		_timeManagerClock.MakeRoot(this);
	}

	public void Pause()
	{
		if (_timeState == TimeState.Running)
		{
			_pauseTime = _systemClock.CurrentTime;
			_timeState = TimeState.Paused;
		}
	}

	public void Restart()
	{
		TimeState timeState = _timeState;
		Stop();
		Start();
		_timeState = timeState;
		if (_timeState == TimeState.Paused)
		{
			_pauseTime = _startTime;
		}
	}

	public void Resume()
	{
		if (_timeState == TimeState.Paused)
		{
			_startTime = _startTime + _systemClock.CurrentTime - _pauseTime;
			_timeState = TimeState.Running;
			if (GetNextTickNeeded() >= TimeSpan.Zero)
			{
				NotifyNewEarliestFutureActivity();
			}
		}
	}

	public void Seek(int offset, TimeSeekOrigin origin)
	{
		if (_timeState < TimeState.Paused)
		{
			return;
		}
		switch (origin)
		{
		case TimeSeekOrigin.Duration:
			break;
		default:
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "TimeSeekOrigin"));
		case TimeSeekOrigin.BeginTime:
		{
			if (offset < 0)
			{
				offset = 0;
			}
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(offset);
			if (timeSpan != _globalTime)
			{
				_globalTime = timeSpan;
				_startTime = _systemClock.CurrentTime - _globalTime;
				_timeManagerClock.ComputeTreeState();
			}
			break;
		}
		}
	}

	public void Start()
	{
		if (_timeState == TimeState.Stopped)
		{
			_lastTickTime = TimeSpan.Zero;
			_startTime = _systemClock.CurrentTime;
			_globalTime = TimeSpan.Zero;
			_timeState = TimeState.Running;
			_timeManagerClock.RootActivate();
		}
	}

	public void Stop()
	{
		if (_timeState >= TimeState.Paused)
		{
			_timeManagerClock.RootDisable();
			_timeState = TimeState.Stopped;
		}
	}

	public void Tick()
	{
		try
		{
			_nextTickTimeQueried = false;
			_isDirty = false;
			if (_timeState == TimeState.Running)
			{
				_globalTime = GetCurrentGlobalTime();
				_isInTick = true;
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordAnimation, EventTrace.Event.WClientTimeManagerTickBegin, (_startTime + _globalTime).Ticks / 10000);
			if (_lastTimeState == TimeState.Stopped && _timeState == TimeState.Stopped)
			{
				_currentTickInterval = TimeIntervalCollection.CreateNullPoint();
			}
			else
			{
				_currentTickInterval = TimeIntervalCollection.CreateOpenClosedInterval(_lastTickTime, _globalTime);
				if (_lastTimeState == TimeState.Stopped || _timeState == TimeState.Stopped)
				{
					_currentTickInterval.AddNullPoint();
				}
			}
			_timeManagerClock.ComputeTreeState();
			_lastTimeState = _timeState;
			RaiseEnqueuedEvents();
		}
		finally
		{
			_isInTick = false;
			_lastTickTime = _globalTime;
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordAnimation, EventTrace.Event.WClientTimeManagerTickEnd);
		}
		CleanupClocks();
	}

	internal int GetMaxDesiredFrameRate()
	{
		return _timeManagerClock.GetMaxDesiredFrameRate();
	}

	private void CleanupClocks()
	{
		if (_needClockCleanup)
		{
			_needClockCleanup = false;
			_timeManagerClock.RootCleanChildren();
		}
	}

	internal void AddToEventQueue(Clock sender)
	{
		_eventQueue.Enqueue(sender.WeakReference);
	}

	internal TimeSpan GetCurrentGlobalTime()
	{
		switch (_timeState)
		{
		case TimeState.Stopped:
			return TimeSpan.Zero;
		case TimeState.Paused:
			return _pauseTime - _startTime;
		case TimeState.Running:
			if (_isInTick || _lockTickTime)
			{
				return _globalTime;
			}
			return _systemClock.CurrentTime - _startTime;
		default:
			return TimeSpan.Zero;
		}
	}

	internal void LockTickTime()
	{
		_lockTickTime = true;
	}

	internal void NotifyNewEarliestFutureActivity()
	{
		if (_nextTickTimeQueried && _userNeedTickSooner != null)
		{
			_nextTickTimeQueried = false;
			_userNeedTickSooner(this, EventArgs.Empty);
		}
	}

	private void RaiseEnqueuedEvents()
	{
		while (_eventQueue.Count > 0)
		{
			((Clock)_eventQueue.Dequeue().Target)?.RaiseAccumulatedEvents();
		}
	}

	internal void ScheduleClockCleanup()
	{
		_needClockCleanup = true;
	}

	internal void SetDirty()
	{
		_isDirty = true;
	}

	internal void UnlockTickTime()
	{
		_lockTickTime = false;
	}

	internal TimeSpan GetNextTickNeeded()
	{
		_nextTickTimeQueried = true;
		if (_timeState == TimeState.Running)
		{
			TimeSpan? internalNextTickNeededTime = _timeManagerClock.InternalNextTickNeededTime;
			if (internalNextTickNeededTime.HasValue)
			{
				TimeSpan timeSpan = _systemClock.CurrentTime - _startTime;
				TimeSpan timeSpan2 = internalNextTickNeededTime.Value - timeSpan;
				if (timeSpan2 <= TimeSpan.Zero)
				{
					return TimeSpan.Zero;
				}
				return timeSpan2;
			}
			return TimeSpan.FromTicks(-1L);
		}
		return TimeSpan.FromTicks(-1L);
	}
}
