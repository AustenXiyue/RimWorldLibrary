using System;
using System.Threading;
using System.Windows.Threading;

namespace MS.Internal;

internal class CleanupHelper : DispatcherObject
{
	private class GCDetector
	{
		private CleanupHelper _parent;

		internal GCDetector(CleanupHelper parent)
		{
			_parent = parent;
		}

		~GCDetector()
		{
			_parent._waitingForGC = false;
		}
	}

	private DispatcherTimer _cleanupTimer;

	private DispatcherTimer _starvationTimer;

	private DispatcherTimer _defaultCleanupTimer;

	private DispatcherPriority _cleanupTimerPriority;

	private int _cleanupRequests;

	private bool _waitingForGC;

	private Func<bool, bool> _cleanupCallback;

	private TimeSpan _basePollingInterval;

	private TimeSpan _maxPollingInterval;

	internal CleanupHelper(Func<bool, bool> callback, int pollingInterval = 400, int promotionInterval = 10000, int maxInterval = 5000)
	{
		_cleanupCallback = callback;
		_basePollingInterval = TimeSpan.FromMilliseconds(pollingInterval);
		_maxPollingInterval = TimeSpan.FromMilliseconds(maxInterval);
		_cleanupTimerPriority = DispatcherPriority.ContextIdle;
		_defaultCleanupTimer = new DispatcherTimer(_cleanupTimerPriority);
		_defaultCleanupTimer.Interval = _basePollingInterval;
		_defaultCleanupTimer.Tick += OnCleanupTick;
		_starvationTimer = new DispatcherTimer(DispatcherPriority.Normal);
		_starvationTimer.Interval = TimeSpan.FromMilliseconds(promotionInterval);
		_starvationTimer.Tick += OnStarvationTick;
		_cleanupTimer = _defaultCleanupTimer;
	}

	internal void ScheduleCleanup()
	{
		if (Interlocked.Increment(ref _cleanupRequests) == 1)
		{
			_cleanupTimer = _defaultCleanupTimer;
			_cleanupTimerPriority = DispatcherPriority.ContextIdle;
			_waitingForGC = true;
			_cleanupTimer.Start();
			_starvationTimer.Start();
			new GCDetector(this);
		}
	}

	internal bool DoCleanup(bool forceCleanup = false)
	{
		_cleanupTimer.Stop();
		_starvationTimer.Stop();
		Interlocked.Exchange(ref _cleanupRequests, 0);
		bool num = _cleanupCallback(forceCleanup);
		if (num)
		{
			_defaultCleanupTimer.Interval = _basePollingInterval;
			return num;
		}
		if (_cleanupTimer.Interval < _maxPollingInterval)
		{
			_cleanupTimer.Interval += _basePollingInterval;
		}
		_defaultCleanupTimer.Interval = _cleanupTimer.Interval;
		return num;
	}

	private void OnCleanupTick(object sender, EventArgs e)
	{
		if (!_waitingForGC)
		{
			DoCleanup();
		}
	}

	private void OnStarvationTick(object sender, EventArgs e)
	{
		if (_cleanupTimerPriority < DispatcherPriority.Render)
		{
			_cleanupTimer.Stop();
			_cleanupTimer = new DispatcherTimer(_cleanupTimer.Interval, ++_cleanupTimerPriority, OnCleanupTick, _cleanupTimer.Dispatcher);
			_waitingForGC = false;
		}
		else
		{
			_starvationTimer.Stop();
		}
	}
}
