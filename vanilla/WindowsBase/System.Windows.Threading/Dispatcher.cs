using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.WindowsBase;
using MS.Utility;
using MS.Win32;

namespace System.Windows.Threading;

/// <summary>Provides services for managing the queue of work items for a thread.</summary>
public sealed class Dispatcher
{
	internal delegate void ShutdownCallback();

	private const int PROCESS_NONE = 0;

	private const int PROCESS_BACKGROUND = 1;

	private const int PROCESS_FOREGROUND = 2;

	private const int TIMERID_BACKGROUND = 1;

	private const int TIMERID_TIMERS = 2;

	private const int DELTA_BACKGROUND = 1;

	private static List<WeakReference> _dispatchers;

	private static WeakReference _possibleDispatcher;

	private static readonly object _globalLock;

	[ThreadStatic]
	private static Dispatcher _tlsDispatcher;

	private Thread _dispatcherThread;

	private int _frameDepth;

	internal bool _exitAllFrames;

	private bool _startingShutdown;

	internal bool _hasShutdownStarted;

	private SecurityCriticalDataClass<CulturePreservingExecutionContext> _shutdownExecutionContext;

	internal int _disableProcessingCount;

	private static PriorityRange _foregroundPriorityRange;

	private static PriorityRange _backgroundPriorityRange;

	private static PriorityRange _idlePriorityRange;

	private SecurityCriticalData<MessageOnlyHwndWrapper> _window;

	private HwndWrapperHook _hook;

	private int _postedProcessingType;

	private static WindowMessage _msgProcessQueue;

	private static ExceptionWrapper _exceptionWrapper;

	private static readonly object ExceptionDataKey;

	private DispatcherUnhandledExceptionEventArgs _unhandledExceptionEventArgs;

	private DispatcherUnhandledExceptionFilterEventHandler _unhandledExceptionFilter;

	private DispatcherUnhandledExceptionFilterEventArgs _exceptionFilterEventArgs;

	private object _reserved0;

	private object _reserved1;

	private object _reserved2;

	private object _reserved3;

	private object _reserved4;

	private object _reservedPtsCache;

	private object _reservedInputMethod;

	private object _reservedInputManager;

	internal DispatcherSynchronizationContext _defaultDispatcherSynchronizationContext;

	internal object _instanceLock = new object();

	private PriorityQueue<DispatcherOperation> _queue;

	private List<DispatcherTimer> _timers = new List<DispatcherTimer>();

	private long _timersVersion;

	private bool _dueTimeFound;

	private int _dueTimeInTicks;

	private bool _isWin32TimerSet;

	private bool _hasShutdownFinished;

	private bool _isTSFMessagePumpEnabled;

	private bool _hasRequestProcessingFailed;

	private DispatcherHooks _hooks;

	/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> for the thread currently executing and creates a new <see cref="T:System.Windows.Threading.Dispatcher" /> if one is not already associated with the thread. </summary>
	/// <returns>The dispatcher associated with the current thread.</returns>
	public static Dispatcher CurrentDispatcher
	{
		get
		{
			Dispatcher dispatcher = FromThread(Thread.CurrentThread);
			if (dispatcher == null)
			{
				dispatcher = new Dispatcher();
			}
			return dispatcher;
		}
	}

	/// <summary>Gets the thread this <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The thread.</returns>
	public Thread Thread => _dispatcherThread;

	/// <summary>Determines whether the <see cref="T:System.Windows.Threading.Dispatcher" /> is shutting down. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Threading.Dispatcher" /> has started shutting down; otherwise, false.</returns>
	public bool HasShutdownStarted => _hasShutdownStarted;

	/// <summary>Determines whether the <see cref="T:System.Windows.Threading.Dispatcher" /> has finished shutting down.</summary>
	/// <returns>true if the dispatcher has finished shutting down; otherwise, false.</returns>
	public bool HasShutdownFinished => _hasShutdownFinished;

	/// <summary>Gets the collection of hooks that provide additional event information about the <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
	/// <returns>The hooks associated with this <see cref="T:System.Windows.Threading.Dispatcher" />. </returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public DispatcherHooks Hooks
	{
		get
		{
			DispatcherHooks dispatcherHooks = null;
			lock (_instanceLock)
			{
				if (_hooks == null)
				{
					_hooks = new DispatcherHooks();
				}
				return _hooks;
			}
		}
	}

	internal object Reserved0
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reserved0;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reserved0 = value;
		}
	}

	internal object Reserved1
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reserved1;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reserved1 = value;
		}
	}

	internal object Reserved2
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reserved2;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reserved2 = value;
		}
	}

	internal object Reserved3
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reserved3;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reserved3 = value;
		}
	}

	internal object Reserved4
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reserved4;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reserved4 = value;
		}
	}

	internal object PtsCache
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			if (!_hasRequestProcessingFailed)
			{
				return _reservedPtsCache;
			}
			if (!(_reservedPtsCache is Tuple<object, List<string>> tuple))
			{
				return _reservedPtsCache;
			}
			return tuple.Item1;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			if (!_hasRequestProcessingFailed)
			{
				_reservedPtsCache = value;
				return;
			}
			List<string> item = ((_reservedPtsCache is Tuple<object, List<string>> tuple) ? tuple.Item2 : new List<string>());
			_reservedPtsCache = new Tuple<object, List<string>>(value, item);
		}
	}

	internal object InputMethod
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reservedInputMethod;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reservedInputMethod = value;
		}
	}

	internal object InputManager
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _reservedInputManager;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			_reservedInputManager = value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsTSFMessagePumpEnabled
	{
		set
		{
			_isTSFMessagePumpEnabled = value;
		}
	}

	private bool HasUnhandledExceptionHandler => this.UnhandledException != null;

	/// <summary>Occurs when the <see cref="T:System.Windows.Threading.Dispatcher" /> begins to shut down. </summary>
	public event EventHandler ShutdownStarted;

	/// <summary>Occurs when the <see cref="T:System.Windows.Threading.Dispatcher" /> finishes shutting down. </summary>
	public event EventHandler ShutdownFinished;

	/// <summary>Occurs when a thread exception is thrown and uncaught during execution of a delegate by way of <see cref="Overload:System.Windows.Threading.Dispatcher.Invoke" /> or <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> when in the filter stage. </summary>
	public event DispatcherUnhandledExceptionFilterEventHandler UnhandledExceptionFilter
	{
		add
		{
			_unhandledExceptionFilter = (DispatcherUnhandledExceptionFilterEventHandler)Delegate.Combine(_unhandledExceptionFilter, value);
		}
		remove
		{
			_unhandledExceptionFilter = (DispatcherUnhandledExceptionFilterEventHandler)Delegate.Remove(_unhandledExceptionFilter, value);
		}
	}

	/// <summary>Occurs when a thread exception is thrown and uncaught during execution of a delegate by way of <see cref="Overload:System.Windows.Threading.Dispatcher.Invoke" /> or <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" />.</summary>
	public event DispatcherUnhandledExceptionEventHandler UnhandledException;

	static Dispatcher()
	{
		_foregroundPriorityRange = new PriorityRange(DispatcherPriority.Loaded, isMinInclusive: true, DispatcherPriority.Send, isMaxInclusive: true);
		_backgroundPriorityRange = new PriorityRange(DispatcherPriority.Background, isMinInclusive: true, DispatcherPriority.Input, isMaxInclusive: true);
		_idlePriorityRange = new PriorityRange(DispatcherPriority.SystemIdle, isMinInclusive: true, DispatcherPriority.ContextIdle, isMaxInclusive: true);
		ExceptionDataKey = new object();
		_msgProcessQueue = UnsafeNativeMethods.RegisterWindowMessage("DispatcherProcessQueue");
		_globalLock = new object();
		_dispatchers = new List<WeakReference>();
		_possibleDispatcher = new WeakReference(null);
		_exceptionWrapper = new ExceptionWrapper();
		_exceptionWrapper.Catch += CatchExceptionStatic;
		_exceptionWrapper.Filter += ExceptionFilterStatic;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> for the specified thread. </summary>
	/// <returns>The dispatcher for <paramref name="thread" />.</returns>
	/// <param name="thread">The thread to obtain the <see cref="T:System.Windows.Threading.Dispatcher" /> from.</param>
	public static Dispatcher FromThread(Thread thread)
	{
		lock (_globalLock)
		{
			Dispatcher dispatcher = null;
			if (thread != null)
			{
				dispatcher = _possibleDispatcher.Target as Dispatcher;
				if (dispatcher == null || dispatcher.Thread != thread)
				{
					dispatcher = null;
					for (int i = 0; i < _dispatchers.Count; i++)
					{
						if (_dispatchers[i].Target is Dispatcher dispatcher2)
						{
							if (dispatcher2.Thread == thread)
							{
								dispatcher = dispatcher2;
							}
						}
						else
						{
							_dispatchers.RemoveAt(i);
							i--;
						}
					}
					if (dispatcher != null)
					{
						if (_possibleDispatcher.IsAlive)
						{
							_possibleDispatcher.Target = dispatcher;
						}
						else
						{
							_possibleDispatcher = new WeakReference(dispatcher);
						}
					}
				}
			}
			return dispatcher;
		}
	}

	/// <summary>Determines whether the calling thread is the thread associated with this <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
	/// <returns>true if the calling thread is the thread associated with this <see cref="T:System.Windows.Threading.Dispatcher" />; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool CheckAccess()
	{
		return Thread == Thread.CurrentThread;
	}

	/// <summary>Determines whether the calling thread has access to this <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
	/// <exception cref="T:System.InvalidOperationException">The calling thread does not have access to this <see cref="T:System.Windows.Threading.Dispatcher" />.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void VerifyAccess()
	{
		if (!CheckAccess())
		{
			ThrowVerifyAccess();
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		[DoesNotReturn]
		static void ThrowVerifyAccess()
		{
			throw new InvalidOperationException(SR.VerifyAccess);
		}
	}

	/// <summary>Initiates shutdown of the <see cref="T:System.Windows.Threading.Dispatcher" /> asynchronously. </summary>
	/// <param name="priority">The priority at which to begin shutting down the dispatcher.</param>
	public void BeginInvokeShutdown(DispatcherPriority priority)
	{
		BeginInvoke(priority, new ShutdownCallback(ShutdownCallbackInternal));
	}

	/// <summary>Initiates the shutdown process of the <see cref="T:System.Windows.Threading.Dispatcher" /> synchronously.</summary>
	public void InvokeShutdown()
	{
		CriticalInvokeShutdown();
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void CriticalInvokeShutdown()
	{
		Invoke(DispatcherPriority.Send, new ShutdownCallback(ShutdownCallbackInternal));
	}

	/// <summary>Pushes the main execution frame on the event queue of the <see cref="T:System.Windows.Threading.Dispatcher" />. </summary>
	public static void Run()
	{
		PushFrame(new DispatcherFrame());
	}

	/// <summary>Enters an execute loop.</summary>
	/// <param name="frame">The frame for the dispatcher to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="frame" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Threading.Dispatcher.HasShutdownFinished" /> is true-or-<paramref name="frame" /> is running on a different <see cref="T:System.Windows.Threading.Dispatcher" />.-or-Dispatcher processing has been disabled.</exception>
	public static void PushFrame(DispatcherFrame frame)
	{
		if (frame == null)
		{
			throw new ArgumentNullException("frame");
		}
		Dispatcher currentDispatcher = CurrentDispatcher;
		if (currentDispatcher._hasShutdownFinished)
		{
			throw new InvalidOperationException(SR.DispatcherHasShutdown);
		}
		if (frame.Dispatcher != currentDispatcher)
		{
			throw new InvalidOperationException(SR.MismatchedDispatchers);
		}
		if (currentDispatcher._disableProcessingCount > 0)
		{
			throw new InvalidOperationException(SR.DispatcherProcessingDisabled);
		}
		currentDispatcher.PushFrameImpl(frame);
	}

	/// <summary>Requests that all frames exit, including nested frames.</summary>
	public static void ExitAllFrames()
	{
		Dispatcher currentDispatcher = CurrentDispatcher;
		if (currentDispatcher._frameDepth > 0)
		{
			currentDispatcher._exitAllFrames = true;
			currentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)delegate
			{
			});
		}
	}

	/// <summary>Creates an awaitable object that asynchronously yields control back to the current dispatcher and provides an opportunity for the dispatcher to process other events.</summary>
	/// <returns>An awaitable object that asynchronously yields control back to the current dispatcher and provides an opportunity for the dispatcher to process other events.</returns>
	public static DispatcherPriorityAwaitable Yield()
	{
		return Yield(DispatcherPriority.Background);
	}

	/// <summary>Creates an awaitable object that asynchronously yields control back to the current dispatcher and provides an opportunity for the dispatcher to process other events.  The work that occurs when control returns to the code awaiting the result of this method is scheduled with the specified priority.</summary>
	/// <returns>An awaitable object that asynchronously yields control back to the current dispatcher and provides an opportunity for the dispatcher to process other events.</returns>
	/// <param name="priority">The priority at which to schedule the continuation.</param>
	public static DispatcherPriorityAwaitable Yield(DispatcherPriority priority)
	{
		ValidatePriority(priority, "priority");
		return new DispatcherPriorityAwaitable(FromThread(Thread.CurrentThread) ?? throw new InvalidOperationException(SR.DispatcherYieldNoAvailableDispatcher), priority);
	}

	/// <summary>Executes the specified delegate asynchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">The delegate to a method that takes no arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid <see cref="T:System.Windows.Threading.DispatcherPriority" />.</exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method)
	{
		return LegacyBeginInvokeImpl(priority, method, null, 0);
	}

	/// <summary>Executes the specified delegate asynchronously at the specified priority and with the specified argument on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">A delegate to a method that takes one argument, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">The object to pass as an argument to the specified method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid <see cref="T:System.Windows.Threading.DispatcherPriority" />.</exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, object arg)
	{
		return LegacyBeginInvokeImpl(priority, method, arg, 1);
	}

	/// <summary>Executes the specified delegate asynchronously at the specified priority and with the specified array of arguments on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> is called, that can be used to interact with the delegate as it is pending execution in the <see cref="T:System.Windows.Threading.Dispatcher" /> queue.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">A delegate to a method that takes multiple arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">The object to pass as an argument to the specified method.</param>
	/// <param name="args">An array of objects to pass as arguments to the specified method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <see cref="T:System.Windows.Threading.DispatcherPriority" /> is not a valid priority.</exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, object arg, params object[] args)
	{
		return LegacyBeginInvokeImpl(priority, method, CombineParameters(arg, args), -1);
	}

	/// <summary>Executes the specified delegate asynchronously with the specified arguments on the thread that the <see cref="T:System.Windows.Threading.Dispatcher" /> was created on.</summary>
	/// <returns>An object, which is returned immediately after <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="method">The delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public DispatcherOperation BeginInvoke(Delegate method, params object[] args)
	{
		return LegacyBeginInvokeImpl(DispatcherPriority.Normal, method, args, -1);
	}

	/// <summary>Executes the specified delegate asynchronously with the specified arguments, at the specified priority, on the thread that the <see cref="T:System.Windows.Threading.Dispatcher" /> was created on.</summary>
	/// <returns>An object, which is returned immediately after <see cref="Overload:System.Windows.Threading.Dispatcher.BeginInvoke" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="method">The delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public DispatcherOperation BeginInvoke(Delegate method, DispatcherPriority priority, params object[] args)
	{
		return LegacyBeginInvokeImpl(priority, method, args, -1);
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	public void Invoke(Action callback)
	{
		Invoke(callback, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	public void Invoke(Action callback, DispatcherPriority priority)
	{
		Invoke(callback, priority, CancellationToken.None, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the action.</param>
	public void Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken)
	{
		Invoke(callback, priority, cancellationToken, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the action.</param>
	/// <param name="timeout">The minimum amount of time to wait for the operation to start.</param>
	public void Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken, TimeSpan timeout)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ValidatePriority(priority, "priority");
		if (timeout.TotalMilliseconds < 0.0 && timeout != TimeSpan.FromMilliseconds(-1.0))
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		if (!cancellationToken.IsCancellationRequested && priority == DispatcherPriority.Send && CheckAccess())
		{
			SynchronizationContext current = SynchronizationContext.Current;
			try
			{
				DispatcherSynchronizationContext synchronizationContext = (BaseCompatibilityPreferences.GetReuseDispatcherSynchronizationContextInstance() ? _defaultDispatcherSynchronizationContext : ((!BaseCompatibilityPreferences.GetFlowDispatcherSynchronizationContextPriority()) ? new DispatcherSynchronizationContext(this, DispatcherPriority.Normal) : new DispatcherSynchronizationContext(this, priority)));
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				callback();
				return;
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(current);
			}
		}
		DispatcherOperation operation = new DispatcherOperation(this, priority, callback);
		InvokeImpl(operation, cancellationToken, timeout);
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public TResult Invoke<TResult>(Func<TResult> callback)
	{
		return Invoke(callback, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public TResult Invoke<TResult>(Func<TResult> callback, DispatcherPriority priority)
	{
		return Invoke(callback, priority, CancellationToken.None, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public TResult Invoke<TResult>(Func<TResult> callback, DispatcherPriority priority, CancellationToken cancellationToken)
	{
		return Invoke(callback, priority, cancellationToken, TimeSpan.FromMilliseconds(-1.0));
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
	/// <param name="timeout">The minimum amount of time to wait for the operation to start.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public TResult Invoke<TResult>(Func<TResult> callback, DispatcherPriority priority, CancellationToken cancellationToken, TimeSpan timeout)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ValidatePriority(priority, "priority");
		if (timeout.TotalMilliseconds < 0.0 && timeout != TimeSpan.FromMilliseconds(-1.0))
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		if (!cancellationToken.IsCancellationRequested && priority == DispatcherPriority.Send && CheckAccess())
		{
			SynchronizationContext current = SynchronizationContext.Current;
			try
			{
				DispatcherSynchronizationContext synchronizationContext = (BaseCompatibilityPreferences.GetReuseDispatcherSynchronizationContextInstance() ? _defaultDispatcherSynchronizationContext : ((!BaseCompatibilityPreferences.GetFlowDispatcherSynchronizationContextPriority()) ? new DispatcherSynchronizationContext(this, DispatcherPriority.Normal) : new DispatcherSynchronizationContext(this, priority)));
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				return callback();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(current);
			}
		}
		DispatcherOperation<TResult> operation = new DispatcherOperation<TResult>(this, priority, callback);
		return (TResult)InvokeImpl(operation, cancellationToken, timeout);
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> asynchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync(System.Action)" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	public DispatcherOperation InvokeAsync(Action callback)
	{
		return InvokeAsync(callback, DispatcherPriority.Normal, CancellationToken.None);
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> asynchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync(System.Action,System.Windows.Threading.DispatcherPriority)" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority)
	{
		return InvokeAsync(callback, priority, CancellationToken.None);
	}

	/// <summary>Executes the specified <see cref="T:System.Action" /> asynchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync(System.Action,System.Windows.Threading.DispatcherPriority,System.Threading.CancellationToken)" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the action.</param>
	public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority, CancellationToken cancellationToken)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ValidatePriority(priority, "priority");
		DispatcherOperation dispatcherOperation = new DispatcherOperation(this, priority, callback);
		InvokeAsyncImpl(dispatcherOperation, cancellationToken);
		return dispatcherOperation;
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> asynchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync``1(System.Func{``0})" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback)
	{
		return InvokeAsync(callback, DispatcherPriority.Normal, CancellationToken.None);
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> asynchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync``1(System.Func{``0},System.Windows.Threading.DispatcherPriority)" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback, DispatcherPriority priority)
	{
		return InvokeAsync(callback, priority, CancellationToken.None);
	}

	/// <summary>Executes the specified <see cref="T:System.Func`1" /> synchronously at the specified priority on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>An object, which is returned immediately after <see cref="M:System.Windows.Threading.Dispatcher.InvokeAsync``1(System.Func{``0},System.Windows.Threading.DispatcherPriority,System.Threading.CancellationToken)" /> is called, that can be used to interact with the delegate as it is pending execution in the event queue.</returns>
	/// <param name="callback">A delegate to invoke through the dispatcher.</param>
	/// <param name="priority">The priority that determines in what order the specified callback is invoked relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" />.</param>
	/// <param name="cancellationToken">An object that indicates whether to cancel the operation.</param>
	/// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
	public DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback, DispatcherPriority priority, CancellationToken cancellationToken)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		ValidatePriority(priority, "priority");
		DispatcherOperation<TResult> dispatcherOperation = new DispatcherOperation<TResult>(this, priority, callback);
		InvokeAsyncImpl(dispatcherOperation, cancellationToken);
		return dispatcherOperation;
	}

	private DispatcherOperation LegacyBeginInvokeImpl(DispatcherPriority priority, Delegate method, object args, int numArgs)
	{
		ValidatePriority(priority, "priority");
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		DispatcherOperation dispatcherOperation = new DispatcherOperation(this, method, priority, args, numArgs);
		InvokeAsyncImpl(dispatcherOperation, CancellationToken.None);
		return dispatcherOperation;
	}

	private void InvokeAsyncImpl(DispatcherOperation operation, CancellationToken cancellationToken)
	{
		DispatcherHooks dispatcherHooks = null;
		bool flag = false;
		lock (_instanceLock)
		{
			if (!cancellationToken.IsCancellationRequested && !_hasShutdownFinished && !Environment.HasShutdownStarted)
			{
				operation._item = _queue.Enqueue(operation.Priority, operation);
				flag = RequestProcessing();
				if (flag)
				{
					dispatcherHooks = _hooks;
				}
				else
				{
					_queue.RemoveItem(operation._item);
				}
			}
		}
		if (flag)
		{
			if (cancellationToken.CanBeCanceled)
			{
				CancellationTokenRegistration cancellationRegistration = cancellationToken.Register(delegate(object? s)
				{
					((DispatcherOperation)s).Abort();
				}, operation);
				operation.Aborted += delegate
				{
					cancellationRegistration.Dispose();
				};
				operation.Completed += delegate
				{
					cancellationRegistration.Dispose();
				};
			}
			dispatcherHooks?.RaiseOperationPosted(this, operation);
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextPost, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info, operation.Priority, operation.Name, operation.Id);
			}
		}
		else
		{
			operation._status = DispatcherOperationStatus.Aborted;
			operation._taskSource.SetCanceled();
		}
	}

	/// <summary>Executes the specified delegate synchronously at the specified priority on the thread on which the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with. </summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">A delegate to a method that takes no arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="priority" /> is equal to <see cref="F:System.Windows.Threading.DispatcherPriority.Inactive" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid priority.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, Delegate method)
	{
		return LegacyInvokeImpl(priority, TimeSpan.FromMilliseconds(-1.0), method, null, 0);
	}

	/// <summary>Executes the specified delegate at the specified priority with the specified argument synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">A delegate to a method that takes one argument, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">An object to pass as an argument to the given method.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="priority" /> is equal to <see cref="F:System.Windows.Threading.DispatcherPriority.Inactive" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid priority.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, Delegate method, object arg)
	{
		return LegacyInvokeImpl(priority, TimeSpan.FromMilliseconds(-1.0), method, arg, 1);
	}

	/// <summary>Executes the specified delegate at the specified priority with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="method">A delegate to a method that takes multiple arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">An object to pass as an argument to the given method.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="priority" /> is equal to <see cref="F:System.Windows.Threading.DispatcherPriority.Inactive" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid priority.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, Delegate method, object arg, params object[] args)
	{
		return LegacyInvokeImpl(priority, TimeSpan.FromMilliseconds(-1.0), method, CombineParameters(arg, args), -1);
	}

	/// <summary>Executes the specified delegate synchronously at the specified priority and with the specified time-out value on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> was created.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="timeout">The maximum time to wait for the operation to finish.</param>
	/// <param name="method">The delegate to a method that takes no arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, TimeSpan timeout, Delegate method)
	{
		return LegacyInvokeImpl(priority, timeout, method, null, 0);
	}

	/// <summary>Executes the specified delegate at the specified priority with the specified argument synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="timeout">The maximum time to wait for the operation to finish.</param>
	/// <param name="method">A delegate to a method that takes multiple arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">An object to pass as an argument to the given method. This can be null if no arguments are needed.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="priority" /> is equal to <see cref="F:System.Windows.Threading.DispatcherPriority.Inactive" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid priority.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg)
	{
		return LegacyInvokeImpl(priority, timeout, method, arg, 1);
	}

	/// <summary>Executes the specified delegate at the specified priority with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="timeout">The maximum time to wait for the operation to finish.</param>
	/// <param name="method">A delegate to a method that takes multiple arguments, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="arg">An object to pass as an argument to the specified method.</param>
	/// <param name="args">An array of objects to pass as arguments to the specified method. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="priority" /> is equal to <see cref="F:System.Windows.Threading.DispatcherPriority.Inactive" />.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid <see cref="T:System.Windows.Threading.DispatcherPriority" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="method" /> is null. </exception>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object Invoke(DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg, params object[] args)
	{
		return LegacyInvokeImpl(priority, timeout, method, CombineParameters(arg, args), -1);
	}

	/// <summary>Executes the specified delegate with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="method">A delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public object Invoke(Delegate method, params object[] args)
	{
		return LegacyInvokeImpl(DispatcherPriority.Normal, TimeSpan.FromMilliseconds(-1.0), method, args, -1);
	}

	/// <summary>Executes the specified delegate at the specified priority with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="method">A delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public object Invoke(Delegate method, DispatcherPriority priority, params object[] args)
	{
		return LegacyInvokeImpl(priority, TimeSpan.FromMilliseconds(-1.0), method, args, -1);
	}

	/// <summary>Executes the specified delegate within the designated time span at the specified priority with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="method">A delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="timeout">The maximum amount of time to wait for the operation to complete.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public object Invoke(Delegate method, TimeSpan timeout, params object[] args)
	{
		return LegacyInvokeImpl(DispatcherPriority.Normal, timeout, method, args, -1);
	}

	/// <summary>Executes the specified delegate within the designated time span at the specified priority with the specified arguments synchronously on the thread the <see cref="T:System.Windows.Threading.Dispatcher" /> is associated with.</summary>
	/// <returns>The return value from the delegate being invoked or null if the delegate has no return value.</returns>
	/// <param name="method">A delegate to a method that takes parameters specified in <paramref name="args" />, which is pushed onto the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.</param>
	/// <param name="timeout">The maximum amount of time to wait for the operation to complete.</param>
	/// <param name="priority">The priority, relative to the other pending operations in the <see cref="T:System.Windows.Threading.Dispatcher" /> event queue, the specified method is invoked.</param>
	/// <param name="args">An array of objects to pass as arguments to the given method. Can be null.</param>
	public object Invoke(Delegate method, TimeSpan timeout, DispatcherPriority priority, params object[] args)
	{
		return LegacyInvokeImpl(priority, timeout, method, args, -1);
	}

	internal object LegacyInvokeImpl(DispatcherPriority priority, TimeSpan timeout, Delegate method, object args, int numArgs)
	{
		ValidatePriority(priority, "priority");
		if (priority == DispatcherPriority.Inactive)
		{
			throw new ArgumentException(SR.InvalidPriority, "priority");
		}
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (timeout.TotalMilliseconds < 0.0 && timeout != TimeSpan.FromMilliseconds(-1.0))
		{
			if (!CheckAccess())
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			timeout = TimeSpan.FromMilliseconds(-1.0);
		}
		if (priority == DispatcherPriority.Send && CheckAccess())
		{
			SynchronizationContext current = SynchronizationContext.Current;
			try
			{
				DispatcherSynchronizationContext synchronizationContext = (BaseCompatibilityPreferences.GetReuseDispatcherSynchronizationContextInstance() ? _defaultDispatcherSynchronizationContext : ((!BaseCompatibilityPreferences.GetFlowDispatcherSynchronizationContextPriority()) ? new DispatcherSynchronizationContext(this, DispatcherPriority.Normal) : new DispatcherSynchronizationContext(this, priority)));
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				return WrappedInvoke(method, args, numArgs, null);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(current);
			}
		}
		DispatcherOperation operation = new DispatcherOperation(this, method, priority, args, numArgs);
		return InvokeImpl(operation, CancellationToken.None, timeout);
	}

	private object InvokeImpl(DispatcherOperation operation, CancellationToken cancellationToken, TimeSpan timeout)
	{
		object result = null;
		if (!cancellationToken.IsCancellationRequested)
		{
			InvokeAsyncImpl(operation, cancellationToken);
			CancellationToken cancellationToken2 = CancellationToken.None;
			CancellationTokenRegistration cancellationTokenRegistration = default(CancellationTokenRegistration);
			CancellationTokenSource cancellationTokenSource = null;
			if (timeout.TotalMilliseconds >= 0.0)
			{
				cancellationTokenSource = new CancellationTokenSource(timeout);
				cancellationToken2 = cancellationTokenSource.Token;
				cancellationTokenRegistration = cancellationToken2.Register(delegate(object? s)
				{
					((DispatcherOperation)s).Abort();
				}, operation);
			}
			try
			{
				operation.Wait();
				result = operation.Result;
			}
			catch (OperationCanceledException)
			{
				if (cancellationToken2.IsCancellationRequested)
				{
					throw new TimeoutException();
				}
				throw;
			}
			finally
			{
				cancellationTokenRegistration.Dispose();
				cancellationTokenSource?.Dispose();
			}
		}
		return result;
	}

	/// <summary>Disables processing of the <see cref="T:System.Windows.Threading.Dispatcher" /> queue.</summary>
	/// <returns>A structure used to re-enable dispatcher processing.</returns>
	public DispatcherProcessingDisabled DisableProcessing()
	{
		VerifyAccess();
		_disableProcessingCount++;
		DispatcherProcessingDisabled result = default(DispatcherProcessingDisabled);
		result._dispatcher = this;
		return result;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Threading.DispatcherPriority" /> is a valid priority. </summary>
	/// <param name="priority">The priority to check.</param>
	/// <param name="parameterName">A string that will be returned by the exception that occurs if the priority is invalid.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="priority" /> is not a valid <see cref="T:System.Windows.Threading.DispatcherPriority" />.</exception>
	public static void ValidatePriority(DispatcherPriority priority, string parameterName)
	{
		if (!_foregroundPriorityRange.Contains(priority) && !_backgroundPriorityRange.Contains(priority) && !_idlePriorityRange.Contains(priority) && priority != 0)
		{
			throw new InvalidEnumArgumentException(parameterName, (int)priority, typeof(DispatcherPriority));
		}
	}

	private Dispatcher()
	{
		_queue = new PriorityQueue<DispatcherOperation>();
		_tlsDispatcher = this;
		_dispatcherThread = Thread.CurrentThread;
		lock (_globalLock)
		{
			_dispatchers.Add(new WeakReference(this));
		}
		_unhandledExceptionEventArgs = new DispatcherUnhandledExceptionEventArgs(this);
		_exceptionFilterEventArgs = new DispatcherUnhandledExceptionFilterEventArgs(this);
		_defaultDispatcherSynchronizationContext = new DispatcherSynchronizationContext(this);
		MessageOnlyHwndWrapper value = new MessageOnlyHwndWrapper();
		_window = new SecurityCriticalData<MessageOnlyHwndWrapper>(value);
		_hook = WndProcHook;
		_window.Value.AddHook(_hook);
		AccessibilitySwitches.VerifySwitches(this);
	}

	internal Dispatcher(bool isSentinel)
	{
		_dispatcherThread = null;
		_startingShutdown = true;
		_hasShutdownStarted = true;
		_hasShutdownFinished = true;
	}

	private void StartShutdownImpl()
	{
		if (!_startingShutdown)
		{
			_startingShutdown = true;
			if (this.ShutdownStarted != null)
			{
				this.ShutdownStarted(this, EventArgs.Empty);
			}
			_hasShutdownStarted = true;
			CulturePreservingExecutionContext value = CulturePreservingExecutionContext.Capture();
			_shutdownExecutionContext = new SecurityCriticalDataClass<CulturePreservingExecutionContext>(value);
			if (_frameDepth <= 0)
			{
				ShutdownImpl();
			}
		}
	}

	private void ShutdownImpl()
	{
		if (!_hasShutdownFinished)
		{
			if (_shutdownExecutionContext != null && _shutdownExecutionContext.Value != null)
			{
				CulturePreservingExecutionContext.Run(_shutdownExecutionContext.Value, ShutdownImplInSecurityContext, null);
			}
			else
			{
				ShutdownImplInSecurityContext(null);
			}
			_shutdownExecutionContext = null;
		}
	}

	private void ShutdownImplInSecurityContext(object state)
	{
		if (this.ShutdownFinished != null)
		{
			this.ShutdownFinished(this, EventArgs.Empty);
		}
		MessageOnlyHwndWrapper messageOnlyHwndWrapper = null;
		lock (_instanceLock)
		{
			messageOnlyHwndWrapper = _window.Value;
			_window = new SecurityCriticalData<MessageOnlyHwndWrapper>(null);
		}
		messageOnlyHwndWrapper.Dispose();
		lock (_instanceLock)
		{
			_hasShutdownFinished = true;
		}
		DispatcherOperation dispatcherOperation = null;
		do
		{
			lock (_instanceLock)
			{
				dispatcherOperation = ((_queue.MaxPriority == DispatcherPriority.Invalid) ? null : _queue.Peek());
			}
			dispatcherOperation?.Abort();
		}
		while (dispatcherOperation != null);
		lock (_instanceLock)
		{
			_queue = null;
			_timers = null;
			_reserved0 = null;
			_reserved1 = null;
			_reserved2 = null;
			_reserved3 = null;
			_reserved4 = null;
			_reservedInputMethod = null;
			_reservedInputManager = null;
		}
	}

	internal bool SetPriority(DispatcherOperation operation, DispatcherPriority priority)
	{
		bool flag = false;
		DispatcherHooks dispatcherHooks = null;
		lock (_instanceLock)
		{
			if (_queue != null && operation._item.IsQueued)
			{
				_queue.ChangeItemPriority(operation._item, priority);
				flag = true;
				if (flag)
				{
					RequestProcessing();
					dispatcherHooks = _hooks;
				}
			}
		}
		if (flag)
		{
			dispatcherHooks?.RaiseOperationPriorityChanged(this, operation);
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextPromote, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info, priority, operation.Name, operation.Id);
			}
		}
		return flag;
	}

	internal bool Abort(DispatcherOperation operation)
	{
		bool flag = false;
		DispatcherHooks dispatcherHooks = null;
		lock (_instanceLock)
		{
			if (_queue != null && operation._item.IsQueued)
			{
				_queue.RemoveItem(operation._item);
				operation._status = DispatcherOperationStatus.Aborted;
				flag = true;
				dispatcherHooks = _hooks;
			}
		}
		if (flag)
		{
			dispatcherHooks?.RaiseOperationAborted(this, operation);
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextAbort, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info, operation.Priority, operation.Name, operation.Id);
			}
		}
		return flag;
	}

	private void ProcessQueue()
	{
		DispatcherPriority dispatcherPriority = DispatcherPriority.Invalid;
		DispatcherOperation dispatcherOperation = null;
		DispatcherHooks dispatcherHooks = null;
		lock (_instanceLock)
		{
			_postedProcessingType = 0;
			bool flag = !IsInputPending();
			dispatcherPriority = _queue.MaxPriority;
			if (dispatcherPriority != DispatcherPriority.Invalid && dispatcherPriority != 0 && (_foregroundPriorityRange.Contains(dispatcherPriority) || flag))
			{
				dispatcherOperation = _queue.Dequeue();
				dispatcherHooks = _hooks;
			}
			dispatcherPriority = _queue.MaxPriority;
			RequestProcessing();
		}
		if (dispatcherOperation == null)
		{
			return;
		}
		bool flag2 = false;
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextDispatchBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info, dispatcherOperation.Priority, dispatcherOperation.Name, dispatcherOperation.Id);
			flag2 = true;
		}
		dispatcherHooks?.RaiseOperationStarted(this, dispatcherOperation);
		dispatcherOperation.Invoke();
		dispatcherHooks?.RaiseOperationCompleted(this, dispatcherOperation);
		if (flag2)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextDispatchEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info);
			if (_idlePriorityRange.Contains(dispatcherPriority))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIContextIdle, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordDispatcher, EventTrace.Level.Info);
			}
		}
		dispatcherOperation.InvokeCompletions();
	}

	private void ShutdownCallbackInternal()
	{
		StartShutdownImpl();
	}

	private void PushFrameImpl(DispatcherFrame frame)
	{
		SynchronizationContext synchronizationContext = null;
		MSG msg = default(MSG);
		_frameDepth++;
		try
		{
			synchronizationContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this));
			try
			{
				while (frame.Continue && GetMessage(ref msg, IntPtr.Zero, 0, 0))
				{
					TranslateAndDispatchMessage(ref msg);
				}
				if (_frameDepth == 1 && _hasShutdownStarted)
				{
					ShutdownImpl();
				}
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
			}
		}
		finally
		{
			_frameDepth--;
			if (_frameDepth == 0)
			{
				_exitAllFrames = false;
			}
		}
	}

	private bool GetMessage(ref MSG msg, nint hwnd, int minMessage, int maxMessage)
	{
		UnsafeNativeMethods.ITfMessagePump messagePump = GetMessagePump();
		try
		{
			if (messagePump == null)
			{
				return UnsafeNativeMethods.GetMessageW(ref msg, new HandleRef(this, hwnd), minMessage, maxMessage);
			}
			messagePump.GetMessageW(ref msg, hwnd, minMessage, maxMessage, out var result);
			return result switch
			{
				-1 => throw new Win32Exception(), 
				0 => false, 
				_ => true, 
			};
		}
		finally
		{
			if (messagePump != null)
			{
				Marshal.ReleaseComObject(messagePump);
			}
		}
	}

	private UnsafeNativeMethods.ITfMessagePump GetMessagePump()
	{
		UnsafeNativeMethods.ITfMessagePump result = null;
		if (_isTSFMessagePumpEnabled && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && TextServicesLoader.ServicesInstalled)
		{
			UnsafeNativeMethods.ITfThreadMgr tfThreadMgr = TextServicesLoader.Load();
			if (tfThreadMgr != null)
			{
				result = tfThreadMgr as UnsafeNativeMethods.ITfMessagePump;
			}
		}
		return result;
	}

	private void TranslateAndDispatchMessage(ref MSG msg)
	{
		if (!ComponentDispatcher.RaiseThreadMessage(ref msg))
		{
			UnsafeNativeMethods.TranslateMessage(ref msg);
			UnsafeNativeMethods.DispatchMessage(ref msg);
		}
	}

	private nint WndProcHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (_disableProcessingCount > 0)
		{
			throw new InvalidOperationException(SR.DispatcherProcessingDisabledButStillPumping);
		}
		if (msg == 2)
		{
			if (!_hasShutdownStarted && !_hasShutdownFinished)
			{
				ShutdownImpl();
			}
		}
		else if (msg == (int)_msgProcessQueue)
		{
			ProcessQueue();
		}
		else if (msg == 275 && (int)wParam == 1)
		{
			SafeNativeMethods.KillTimer(new HandleRef(this, hwnd), 1);
			ProcessQueue();
		}
		else if (msg == 275 && (int)wParam == 2)
		{
			KillWin32Timer();
			PromoteTimers(Environment.TickCount);
		}
		DispatcherHooks dispatcherHooks = null;
		bool flag = false;
		lock (_instanceLock)
		{
			flag = _postedProcessingType < 1;
			if (flag)
			{
				dispatcherHooks = _hooks;
			}
		}
		if (flag)
		{
			dispatcherHooks?.RaiseDispatcherInactive(this);
			ComponentDispatcher.RaiseIdle();
		}
		return IntPtr.Zero;
	}

	private bool IsInputPending()
	{
		return UnsafeNativeMethods.MsgWaitForMultipleObjectsEx(0, null, 0, 8207, 4) == 0;
	}

	private bool RequestProcessing()
	{
		return CriticalRequestProcessing(force: false);
	}

	internal bool CriticalRequestProcessing(bool force)
	{
		bool result = true;
		if (IsWindowNull())
		{
			return false;
		}
		DispatcherPriority maxPriority = _queue.MaxPriority;
		if (maxPriority != DispatcherPriority.Invalid && maxPriority != 0)
		{
			if (force)
			{
				if (_postedProcessingType == 1)
				{
					SafeNativeMethods.KillTimer(new HandleRef(this, _window.Value.Handle), 1);
				}
				else if (_postedProcessingType == 2)
				{
					nint messageExtraInfo = UnsafeNativeMethods.GetMessageExtraInfo();
					MSG msg = default(MSG);
					UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(this, _window.Value.Handle), _msgProcessQueue, _msgProcessQueue, 1);
					UnsafeNativeMethods.SetMessageExtraInfo(messageExtraInfo);
				}
				_postedProcessingType = 0;
			}
			result = ((!_foregroundPriorityRange.Contains(maxPriority)) ? RequestBackgroundProcessing() : RequestForegroundProcessing());
		}
		return result;
	}

	private bool IsWindowNull()
	{
		if (_window.Value == null)
		{
			return true;
		}
		return false;
	}

	private bool RequestForegroundProcessing()
	{
		if (_postedProcessingType < 2)
		{
			if (_postedProcessingType == 1)
			{
				SafeNativeMethods.KillTimer(new HandleRef(this, _window.Value.Handle), 1);
			}
			_postedProcessingType = 2;
			bool num = UnsafeNativeMethods.TryPostMessage(new HandleRef(this, _window.Value.Handle), _msgProcessQueue, IntPtr.Zero, IntPtr.Zero);
			if (!num)
			{
				OnRequestProcessingFailure("TryPostMessage");
			}
			return num;
		}
		return true;
	}

	private bool RequestBackgroundProcessing()
	{
		bool flag = true;
		if (_postedProcessingType < 1)
		{
			if (IsInputPending())
			{
				_postedProcessingType = 1;
				flag = SafeNativeMethods.TrySetTimer(new HandleRef(this, _window.Value.Handle), 1, 1);
				if (!flag)
				{
					OnRequestProcessingFailure("TrySetTimer");
				}
			}
			else
			{
				flag = RequestForegroundProcessing();
			}
		}
		return flag;
	}

	private void OnRequestProcessingFailure(string methodName)
	{
		if (!_hasRequestProcessingFailed)
		{
			_reservedPtsCache = new Tuple<object, List<string>>(_reservedPtsCache, new List<string>());
			_hasRequestProcessingFailed = true;
		}
		if (_reservedPtsCache is Tuple<object, List<string>> { Item2: var item })
		{
			item.Add(string.Format(CultureInfo.InvariantCulture, "{0:O} {1} failed", DateTime.Now, methodName));
			if (item.Count > 1000)
			{
				item.RemoveRange(100, item.Count - 200);
				item.Insert(100, "... entries removed to conserve memory ...");
			}
		}
		switch (BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure)
		{
		case BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Throw:
			throw new InvalidOperationException(SR.DispatcherRequestProcessingFailed);
		case BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Reset:
			_postedProcessingType = 0;
			break;
		case BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Continue:
			break;
		}
	}

	internal void PromoteTimers(int currentTimeInTicks)
	{
		try
		{
			List<DispatcherTimer> list = null;
			long num = 0L;
			lock (_instanceLock)
			{
				if (!_hasShutdownFinished && _dueTimeFound && _dueTimeInTicks - currentTimeInTicks <= 0)
				{
					list = _timers;
					num = _timersVersion;
				}
			}
			if (list == null)
			{
				return;
			}
			DispatcherTimer dispatcherTimer = null;
			int i = 0;
			do
			{
				lock (_instanceLock)
				{
					dispatcherTimer = null;
					if (num != _timersVersion)
					{
						num = _timersVersion;
						i = 0;
					}
					for (; i < _timers.Count; i++)
					{
						if (list[i]._dueTimeInTicks - currentTimeInTicks <= 0)
						{
							dispatcherTimer = list[i];
							list.RemoveAt(i);
							break;
						}
					}
				}
				dispatcherTimer?.Promote();
			}
			while (dispatcherTimer != null);
		}
		finally
		{
			UpdateWin32Timer();
		}
	}

	internal void AddTimer(DispatcherTimer timer)
	{
		lock (_instanceLock)
		{
			if (!_hasShutdownFinished)
			{
				_timers.Add(timer);
				_timersVersion++;
			}
		}
		UpdateWin32Timer();
	}

	internal void RemoveTimer(DispatcherTimer timer)
	{
		lock (_instanceLock)
		{
			if (!_hasShutdownFinished)
			{
				_timers.Remove(timer);
				_timersVersion++;
			}
		}
		UpdateWin32Timer();
	}

	internal void UpdateWin32Timer()
	{
		if (CheckAccess())
		{
			UpdateWin32TimerFromDispatcherThread(null);
		}
		else
		{
			BeginInvoke(DispatcherPriority.Send, new DispatcherOperationCallback(UpdateWin32TimerFromDispatcherThread), null);
		}
	}

	private object UpdateWin32TimerFromDispatcherThread(object unused)
	{
		lock (_instanceLock)
		{
			if (!_hasShutdownFinished)
			{
				bool dueTimeFound = _dueTimeFound;
				int dueTimeInTicks = _dueTimeInTicks;
				_dueTimeFound = false;
				_dueTimeInTicks = 0;
				if (_timers.Count > 0)
				{
					for (int i = 0; i < _timers.Count; i++)
					{
						DispatcherTimer dispatcherTimer = _timers[i];
						if (!_dueTimeFound || dispatcherTimer._dueTimeInTicks - _dueTimeInTicks < 0)
						{
							_dueTimeFound = true;
							_dueTimeInTicks = dispatcherTimer._dueTimeInTicks;
						}
					}
				}
				if (_dueTimeFound)
				{
					if (!_isWin32TimerSet || !dueTimeFound || dueTimeInTicks != _dueTimeInTicks)
					{
						SetWin32Timer(_dueTimeInTicks);
					}
				}
				else if (dueTimeFound)
				{
					KillWin32Timer();
				}
			}
		}
		return null;
	}

	private void SetWin32Timer(int dueTimeInTicks)
	{
		if (!IsWindowNull())
		{
			int num = dueTimeInTicks - Environment.TickCount;
			if (num < 1)
			{
				num = 1;
			}
			SafeNativeMethods.SetTimer(new HandleRef(this, _window.Value.Handle), 2, num);
			_isWin32TimerSet = true;
		}
	}

	private void KillWin32Timer()
	{
		if (!IsWindowNull())
		{
			SafeNativeMethods.KillTimer(new HandleRef(this, _window.Value.Handle), 2);
			_isWin32TimerSet = false;
		}
	}

	private static bool ExceptionFilterStatic(object source, Exception e)
	{
		return ((Dispatcher)source).ExceptionFilter(e);
	}

	private bool ExceptionFilter(Exception e)
	{
		if (!e.Data.Contains(ExceptionDataKey))
		{
			e.Data.Add(ExceptionDataKey, null);
			bool flag = HasUnhandledExceptionHandler;
			if (_unhandledExceptionFilter != null)
			{
				_exceptionFilterEventArgs.Initialize(e, flag);
				bool flag2 = false;
				try
				{
					_unhandledExceptionFilter(this, _exceptionFilterEventArgs);
					flag2 = true;
				}
				finally
				{
					if (flag2)
					{
						flag = _exceptionFilterEventArgs.RequestCatch;
					}
				}
			}
			return flag;
		}
		return false;
	}

	private static bool CatchExceptionStatic(object source, Exception e)
	{
		return ((Dispatcher)source).CatchException(e);
	}

	private bool CatchException(Exception e)
	{
		bool result = false;
		if (this.UnhandledException != null)
		{
			_unhandledExceptionEventArgs.Initialize(e, handled: false);
			bool flag = false;
			try
			{
				this.UnhandledException(this, _unhandledExceptionEventArgs);
				result = _unhandledExceptionEventArgs.Handled;
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					result = false;
				}
			}
		}
		return result;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal object WrappedInvoke(Delegate callback, object args, int numArgs, Delegate catchHandler)
	{
		return _exceptionWrapper.TryCatchWhen(this, callback, args, numArgs, catchHandler);
	}

	private object[] CombineParameters(object arg, object[] args)
	{
		object[] array = new object[1 + ((args == null) ? 1 : args.Length)];
		array[0] = arg;
		if (args != null)
		{
			Array.Copy(args, 0, array, 1, args.Length);
		}
		else
		{
			array[1] = null;
		}
		return array;
	}
}
