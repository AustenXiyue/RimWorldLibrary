using MS.Internal.WindowsBase;

namespace System.Windows.Threading;

/// <summary>A timer that is integrated into the <see cref="T:System.Windows.Threading.Dispatcher" /> queue which is processed at a specified interval of time and at a specified priority. </summary>
public class DispatcherTimer
{
	private readonly object _instanceLock = new object();

	private Dispatcher _dispatcher;

	private DispatcherPriority _priority;

	private TimeSpan _interval;

	private object _tag;

	private DispatcherOperation _operation;

	private bool _isEnabled;

	internal int _dueTimeInTicks;

	/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> associated with this <see cref="T:System.Windows.Threading.DispatcherTimer" />. </summary>
	/// <returns>The dispatcher.</returns>
	public Dispatcher Dispatcher => _dispatcher;

	/// <summary>Gets or sets a value that indicates whether the timer is running. </summary>
	/// <returns>true if the timer is enabled; otherwise, false.  The default is false.</returns>
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			lock (_instanceLock)
			{
				if (!value && _isEnabled)
				{
					Stop();
				}
				else if (value && !_isEnabled)
				{
					Start();
				}
			}
		}
	}

	/// <summary>Gets or sets the period of time between timer ticks. </summary>
	/// <returns>The period of time between ticks. The default is 00:00:00.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="interval" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" /> milliseconds.</exception>
	public TimeSpan Interval
	{
		get
		{
			return _interval;
		}
		set
		{
			bool flag = false;
			if (value.TotalMilliseconds < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.TimeSpanPeriodOutOfRange_TooSmall);
			}
			if (value.TotalMilliseconds > 2147483647.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.TimeSpanPeriodOutOfRange_TooLarge);
			}
			lock (_instanceLock)
			{
				_interval = value;
				if (_isEnabled)
				{
					_dueTimeInTicks = Environment.TickCount + (int)_interval.TotalMilliseconds;
					flag = true;
				}
			}
			if (flag)
			{
				_dispatcher.UpdateWin32Timer();
			}
		}
	}

	/// <summary>Gets or sets a user-defined data object. </summary>
	/// <returns>The user-defined data.  The default is null.</returns>
	public object Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	/// <summary>Occurs when the timer interval has elapsed. </summary>
	public event EventHandler Tick;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherTimer" /> class.</summary>
	public DispatcherTimer()
		: this(DispatcherPriority.Background)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherTimer" /> class which processes timer events at the specified priority.</summary>
	/// <param name="priority">The priority at which to invoke the timer.</param>
	public DispatcherTimer(DispatcherPriority priority)
	{
		Initialize(Dispatcher.CurrentDispatcher, priority, TimeSpan.FromMilliseconds(0.0));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherTimer" /> class which runs on the specified <see cref="T:System.Windows.Threading.Dispatcher" /> at the specified priority.</summary>
	/// <param name="priority">The priority at which to invoke the timer.</param>
	/// <param name="dispatcher">The dispatcher the timer is associated with.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dispatcher" /> is null.</exception>
	public DispatcherTimer(DispatcherPriority priority, Dispatcher dispatcher)
	{
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		Initialize(dispatcher, priority, TimeSpan.FromMilliseconds(0.0));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherTimer" /> class which uses the specified time interval, priority, event handler, and <see cref="T:System.Windows.Threading.Dispatcher" />.</summary>
	/// <param name="interval">The period of time between ticks.</param>
	/// <param name="priority">The priority at which to invoke the timer.</param>
	/// <param name="callback">The event handler to call when the <see cref="E:System.Windows.Threading.DispatcherTimer.Tick" /> event occurs.</param>
	/// <param name="dispatcher">The dispatcher the timer is associated with.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dispatcher" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callback" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="interval" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public DispatcherTimer(TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		if (interval.TotalMilliseconds < 0.0)
		{
			throw new ArgumentOutOfRangeException("interval", SR.TimeSpanPeriodOutOfRange_TooSmall);
		}
		if (interval.TotalMilliseconds > 2147483647.0)
		{
			throw new ArgumentOutOfRangeException("interval", SR.TimeSpanPeriodOutOfRange_TooLarge);
		}
		Initialize(dispatcher, priority, interval);
		Tick += callback;
		Start();
	}

	/// <summary>Starts the <see cref="T:System.Windows.Threading.DispatcherTimer" />. </summary>
	public void Start()
	{
		lock (_instanceLock)
		{
			if (!_isEnabled)
			{
				_isEnabled = true;
				Restart();
			}
		}
	}

	/// <summary>Stops the <see cref="T:System.Windows.Threading.DispatcherTimer" />. </summary>
	public void Stop()
	{
		bool flag = false;
		lock (_instanceLock)
		{
			if (_isEnabled)
			{
				_isEnabled = false;
				flag = true;
				if (_operation != null)
				{
					_operation.Abort();
					_operation = null;
				}
			}
		}
		if (flag)
		{
			_dispatcher.RemoveTimer(this);
		}
	}

	private void Initialize(Dispatcher dispatcher, DispatcherPriority priority, TimeSpan interval)
	{
		Dispatcher.ValidatePriority(priority, "priority");
		if (priority == DispatcherPriority.Inactive)
		{
			throw new ArgumentException(SR.InvalidPriority, "priority");
		}
		_dispatcher = dispatcher;
		_priority = priority;
		_interval = interval;
	}

	private void Restart()
	{
		lock (_instanceLock)
		{
			if (_operation == null)
			{
				_operation = _dispatcher.BeginInvoke(DispatcherPriority.Inactive, (DispatcherOperationCallback)((object state) => ((DispatcherTimer)state).FireTick()), this);
				_dueTimeInTicks = Environment.TickCount + (int)_interval.TotalMilliseconds;
				if (_interval.TotalMilliseconds == 0.0 && _dispatcher.CheckAccess())
				{
					Promote();
				}
				else
				{
					_dispatcher.AddTimer(this);
				}
			}
		}
	}

	internal void Promote()
	{
		lock (_instanceLock)
		{
			if (_operation != null)
			{
				_operation.Priority = _priority;
			}
		}
	}

	private object FireTick()
	{
		_operation = null;
		if (this.Tick != null)
		{
			this.Tick(this, EventArgs.Empty);
		}
		if (_isEnabled)
		{
			Restart();
		}
		return null;
	}
}
