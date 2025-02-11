using System.Threading;
using MS.Win32;

namespace System.Windows.Threading;

/// <summary>Provides a synchronization context for Windows Presentation Foundation (WPF).</summary>
public sealed class DispatcherSynchronizationContext : SynchronizationContext
{
	internal Dispatcher _dispatcher;

	private DispatcherPriority _priority;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> class by using the current <see cref="T:System.Windows.Threading.Dispatcher" />.</summary>
	public DispatcherSynchronizationContext()
		: this(Dispatcher.CurrentDispatcher, DispatcherPriority.Normal)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> class by using the specified <see cref="T:System.Windows.Threading.Dispatcher" />.</summary>
	/// <param name="dispatcher">The <see cref="T:System.Windows.Threading.Dispatcher" /> to associate this <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> with.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dispatcher" /> is null.</exception>
	public DispatcherSynchronizationContext(Dispatcher dispatcher)
		: this(dispatcher, DispatcherPriority.Normal)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> class by using the specified <see cref="T:System.Windows.Threading.Dispatcher" />.</summary>
	/// <param name="dispatcher">The <see cref="T:System.Windows.Threading.Dispatcher" /> to associate this <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" /> with.</param>
	/// <param name="priority">The priority used to send and post callback methods.</param>
	public DispatcherSynchronizationContext(Dispatcher dispatcher, DispatcherPriority priority)
	{
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		Dispatcher.ValidatePriority(priority, "priority");
		_dispatcher = dispatcher;
		_priority = priority;
		SetWaitNotificationRequired();
	}

	/// <summary>Invokes the callback in the synchronization context synchronously.</summary>
	/// <param name="d">The delegate to call.</param>
	/// <param name="state">The object passed to the delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public override void Send(SendOrPostCallback d, object state)
	{
		if (BaseCompatibilityPreferences.GetInlineDispatcherSynchronizationContextSend() && _dispatcher.CheckAccess())
		{
			_dispatcher.Invoke(DispatcherPriority.Send, d, state);
		}
		else
		{
			_dispatcher.Invoke(_priority, d, state);
		}
	}

	/// <summary>Invokes the callback in the synchronization context asynchronously. </summary>
	/// <param name="d">The delegate to call.</param>
	/// <param name="state">The object passed to the delegate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null.</exception>
	public override void Post(SendOrPostCallback d, object state)
	{
		_dispatcher.BeginInvoke(_priority, d, state);
	}

	public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
	{
		if (_dispatcher._disableProcessingCount > 0)
		{
			return UnsafeNativeMethods.WaitForMultipleObjectsEx(waitHandles.Length, waitHandles, waitAll, millisecondsTimeout, bAlertable: false);
		}
		return SynchronizationContext.WaitHelper(waitHandles, waitAll, millisecondsTimeout);
	}

	/// <summary>Creates a copy of this <see cref="T:System.Windows.Threading.DispatcherSynchronizationContext" />. </summary>
	/// <returns>The copy of this synchronization context.</returns>
	public override SynchronizationContext CreateCopy()
	{
		if (BaseCompatibilityPreferences.GetReuseDispatcherSynchronizationContextInstance())
		{
			return this;
		}
		if (BaseCompatibilityPreferences.GetFlowDispatcherSynchronizationContextPriority())
		{
			return new DispatcherSynchronizationContext(_dispatcher, _priority);
		}
		return new DispatcherSynchronizationContext(_dispatcher, DispatcherPriority.Normal);
	}
}
