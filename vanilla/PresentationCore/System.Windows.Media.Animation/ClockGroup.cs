using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>An assemblage of <see cref="T:System.Windows.Media.Animation.Clock" /> types with behavior based off of a <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</summary>
public class ClockGroup : Clock
{
	private List<Clock> _children;

	private List<WeakReference> _rootChildren;

	private TimeIntervalCollection _currentIntervals;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> object that dictates the behavior of this <see cref="T:System.Windows.Media.Animation.ClockGroup" /> instance.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> object.</returns>
	public new TimelineGroup Timeline => (TimelineGroup)base.Timeline;

	/// <summary>Gets the children collection of this <see cref="T:System.Windows.Media.Animation.ClockGroup" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.ClockCollection" /> that represents the children of this <see cref="T:System.Windows.Media.Animation.ClockGroup" />.</returns>
	public ClockCollection Children => new ClockCollection(this);

	internal List<Clock> InternalChildren => _children;

	internal List<WeakReference> InternalRootChildren => _rootChildren;

	internal override Clock FirstChild
	{
		get
		{
			Clock result = null;
			List<Clock> children = _children;
			if (children != null)
			{
				result = children[0];
			}
			return result;
		}
	}

	internal override Duration CurrentDuration
	{
		get
		{
			Duration duration = _timeline.Duration;
			if (duration != Duration.Automatic)
			{
				return duration;
			}
			Duration duration2 = TimeSpan.Zero;
			if (_children != null)
			{
				bool flag = false;
				bool flag2 = _syncData != null && _syncData.IsInSyncPeriod && !_syncData.SyncClockHasReachedEffectiveDuration;
				for (int i = 0; i < _children.Count; i++)
				{
					Duration endOfActivePeriod = _children[i].EndOfActivePeriod;
					if (endOfActivePeriod == Duration.Forever)
					{
						return Duration.Forever;
					}
					if (endOfActivePeriod == Duration.Automatic)
					{
						flag = true;
						continue;
					}
					if (flag2 && _syncData.SyncClock == this)
					{
						endOfActivePeriod += (Duration)TimeSpan.FromMilliseconds(50.0);
						flag2 = false;
					}
					if (endOfActivePeriod > duration2)
					{
						duration2 = endOfActivePeriod;
					}
				}
				if (flag)
				{
					return Duration.Automatic;
				}
			}
			return duration2;
		}
	}

	internal bool RootHasChildren => _rootChildren.Count > 0;

	internal TimeIntervalCollection CurrentIntervals => _currentIntervals;

	/// <summary>Instantiates a new instance of the <see cref="T:System.Windows.Media.Animation.ClockGroup" /> class.</summary>
	/// <param name="timelineGroup">The object defining the characteristics of the new class.</param>
	protected internal ClockGroup(TimelineGroup timelineGroup)
		: base(timelineGroup)
	{
	}

	internal override void BuildClockSubTreeFromTimeline(Timeline timeline, bool hasControllableRoot)
	{
		TimelineCollection children = (timeline as TimelineGroup).Children;
		if (children == null || children.Count <= 0)
		{
			return;
		}
		_children = new List<Clock>();
		for (int i = 0; i < children.Count; i++)
		{
			Clock clock = Clock.AllocateClock(children[i], hasControllableRoot);
			clock._parent = this;
			clock.BuildClockSubTreeFromTimeline(children[i], hasControllableRoot);
			_children.Add(clock);
			clock._childIndex = i;
		}
		if (!(_timeline is ParallelTimeline) || ((ParallelTimeline)_timeline).SlipBehavior != SlipBehavior.Slip)
		{
			return;
		}
		if (!base.IsRoot || _timeline.RepeatBehavior.HasDuration || _timeline.AutoReverse || _timeline.AccelerationRatio > 0.0 || _timeline.DecelerationRatio > 0.0)
		{
			throw new NotSupportedException(SR.Timing_SlipBehavior_SlipOnlyOnSimpleTimelines);
		}
		for (int j = 0; j < _children.Count; j++)
		{
			Clock clock2 = _children[j];
			if (clock2.CanSlip)
			{
				Duration resolvedDuration = clock2.ResolvedDuration;
				if ((!resolvedDuration.HasTimeSpan || resolvedDuration.TimeSpan > TimeSpan.Zero) && clock2._timeline.BeginTime.HasValue)
				{
					_syncData = new SyncData(clock2);
					clock2._syncData = null;
				}
				break;
			}
		}
	}

	internal int GetMaxDesiredFrameRate()
	{
		int num = 0;
		WeakRefEnumerator<Clock> weakRefEnumerator = new WeakRefEnumerator<Clock>(_rootChildren);
		while (weakRefEnumerator.MoveNext())
		{
			Clock current = weakRefEnumerator.Current;
			if (current != null && current.CurrentState == ClockState.Active)
			{
				int? desiredFrameRate = current.DesiredFrameRate;
				if (desiredFrameRate.HasValue)
				{
					num = Math.Max(num, desiredFrameRate.Value);
				}
			}
		}
		return num;
	}

	internal void ComputeTreeState()
	{
		WeakRefEnumerator<Clock> weakRefEnumerator = new WeakRefEnumerator<Clock>(_rootChildren);
		while (weakRefEnumerator.MoveNext())
		{
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(weakRefEnumerator.Current, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				Clock current = prefixSubtreeEnumerator.Current;
				TimeSpan currentGlobalTime = base.CurrentGlobalTime;
				TimeSpan? internalNextTickNeededTime = current.InternalNextTickNeededTime;
				if (currentGlobalTime >= internalNextTickNeededTime)
				{
					current.ApplyDesiredFrameRateToGlobalTime();
					current.ComputeLocalState();
					current.ClipNextTickByParent();
					current.NeedsPostfixTraversal = current is ClockGroup || current.IsRoot;
				}
				else
				{
					prefixSubtreeEnumerator.SkipSubtree();
				}
			}
		}
		ComputeTreeStateRoot();
	}

	internal void ComputeTreeStateRoot()
	{
		TimeSpan? internalNextTickNeededTime = base.InternalNextTickNeededTime;
		base.InternalNextTickNeededTime = null;
		WeakRefEnumerator<Clock> weakRefEnumerator = new WeakRefEnumerator<Clock>(_rootChildren);
		while (weakRefEnumerator.MoveNext())
		{
			Clock current = weakRefEnumerator.Current;
			if (current.NeedsPostfixTraversal)
			{
				if (current is ClockGroup)
				{
					((ClockGroup)current).ComputeTreeStatePostfix();
				}
				current.ApplyDesiredFrameRateToNextTick();
				current.NeedsPostfixTraversal = false;
			}
			if (!base.InternalNextTickNeededTime.HasValue || (weakRefEnumerator.Current.InternalNextTickNeededTime.HasValue && weakRefEnumerator.Current.InternalNextTickNeededTime < base.InternalNextTickNeededTime))
			{
				base.InternalNextTickNeededTime = weakRefEnumerator.Current.InternalNextTickNeededTime;
			}
		}
		if (base.InternalNextTickNeededTime.HasValue && (!internalNextTickNeededTime.HasValue || internalNextTickNeededTime > base.InternalNextTickNeededTime))
		{
			_timeManager.NotifyNewEarliestFutureActivity();
		}
	}

	private void ComputeTreeStatePostfix()
	{
		if (_children == null)
		{
			return;
		}
		for (int i = 0; i < _children.Count; i++)
		{
			if (_children[i].NeedsPostfixTraversal)
			{
				(_children[i] as ClockGroup).ComputeTreeStatePostfix();
			}
		}
		ClipNextTickByChildren();
	}

	private void ClipNextTickByChildren()
	{
		for (int i = 0; i < _children.Count; i++)
		{
			if (!base.InternalNextTickNeededTime.HasValue || (_children[i].InternalNextTickNeededTime.HasValue && _children[i].InternalNextTickNeededTime < base.InternalNextTickNeededTime))
			{
				base.InternalNextTickNeededTime = _children[i].InternalNextTickNeededTime;
			}
		}
	}

	internal void MakeRoot(TimeManager timeManager)
	{
		base.IsTimeManager = true;
		_rootChildren = new List<WeakReference>();
		_timeManager = timeManager;
		_depth = 0;
		base.InternalCurrentIteration = 1;
		base.InternalCurrentProgress = 0.0;
		base.InternalCurrentGlobalSpeed = 1.0;
		base.InternalCurrentClockState = ClockState.Active;
	}

	internal override void ResetNodesWithSlip()
	{
		if (_children != null)
		{
			for (int i = 0; i < _children.Count; i++)
			{
				Clock clock = _children[i];
				if (clock._syncData != null)
				{
					clock._beginTime = clock._timeline.BeginTime;
					clock._syncData.IsInSyncPeriod = false;
					clock._syncData.UpdateClockBeginTime();
				}
			}
		}
		base.ResetNodesWithSlip();
	}

	internal void RootActivate()
	{
		TimeIntervalCollection internalCurrentIntervals = TimeIntervalCollection.CreatePoint(_timeManager.InternalCurrentGlobalTime);
		internalCurrentIntervals.AddNullPoint();
		_timeManager.InternalCurrentIntervals = internalCurrentIntervals;
		ComputeTreeState();
	}

	internal void RootCleanChildren()
	{
		WeakRefEnumerator<Clock> weakRefEnumerator = new WeakRefEnumerator<Clock>(_rootChildren);
		while (weakRefEnumerator.MoveNext())
		{
		}
	}

	internal void RootDisable()
	{
		WeakRefEnumerator<Clock> weakRefEnumerator = new WeakRefEnumerator<Clock>(_rootChildren);
		while (weakRefEnumerator.MoveNext())
		{
			PrefixSubtreeEnumerator prefixSubtreeEnumerator = new PrefixSubtreeEnumerator(weakRefEnumerator.Current, processRoot: true);
			while (prefixSubtreeEnumerator.MoveNext())
			{
				if (prefixSubtreeEnumerator.Current.InternalCurrentClockState != ClockState.Stopped)
				{
					prefixSubtreeEnumerator.Current.ResetCachedStateToStopped();
					prefixSubtreeEnumerator.Current.RaiseCurrentStateInvalidated();
					prefixSubtreeEnumerator.Current.RaiseCurrentTimeInvalidated();
					prefixSubtreeEnumerator.Current.RaiseCurrentGlobalSpeedInvalidated();
				}
				else
				{
					prefixSubtreeEnumerator.SkipSubtree();
				}
			}
		}
	}

	internal override void UpdateDescendantsWithUnresolvedDuration()
	{
		if (!base.HasDescendantsWithUnresolvedDuration || !base.HasResolvedDuration)
		{
			return;
		}
		if (_children != null)
		{
			for (int i = 0; i < _children.Count; i++)
			{
				_children[i].UpdateDescendantsWithUnresolvedDuration();
				if (_children[i].HasDescendantsWithUnresolvedDuration)
				{
					return;
				}
			}
		}
		base.HasDescendantsWithUnresolvedDuration = false;
	}

	internal override void ClearCurrentIntervalsToNull()
	{
		_currentIntervals.Clear();
		_currentIntervals.AddNullPoint();
	}

	internal override void AddNullPointToCurrentIntervals()
	{
		_currentIntervals.AddNullPoint();
	}

	internal override void ComputeCurrentIntervals(TimeIntervalCollection parentIntervalCollection, TimeSpan beginTime, TimeSpan? endTime, Duration fillDuration, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
		_currentIntervals.Clear();
		parentIntervalCollection.ProjectOntoPeriodicFunction(ref _currentIntervals, beginTime, endTime, fillDuration, period, appliedSpeedRatio, accelRatio, decelRatio, isAutoReversed);
	}

	internal override void ComputeCurrentFillInterval(TimeIntervalCollection parentIntervalCollection, TimeSpan beginTime, TimeSpan endTime, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
		_currentIntervals.Clear();
		parentIntervalCollection.ProjectPostFillZone(ref _currentIntervals, beginTime, endTime, period, appliedSpeedRatio, accelRatio, decelRatio, isAutoReversed);
	}
}
