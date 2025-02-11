using System.Collections.Specialized;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Contains state information and event data associated with a routed event. </summary>
public class RoutedEventArgs : EventArgs
{
	private RoutedEvent _routedEvent;

	private object _source;

	private object _originalSource;

	private BitVector32 _flags;

	private const int HandledIndex = 1;

	private const int UserInitiatedIndex = 2;

	private const int InvokingHandlerIndex = 4;

	/// <summary>Gets or sets the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> associated with this <see cref="T:System.Windows.RoutedEventArgs" /> instance. </summary>
	/// <returns>The identifier for the event that has been invoked.</returns>
	/// <exception cref="T:System.InvalidOperationException">Attempted to change the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" />   value while the event is being routed.</exception>
	public RoutedEvent RoutedEvent
	{
		get
		{
			return _routedEvent;
		}
		set
		{
			if (UserInitiated && InvokingHandler)
			{
				throw new InvalidOperationException(SR.RoutedEventCannotChangeWhileRouting);
			}
			_routedEvent = value;
		}
	}

	/// <summary>Gets or sets a value that indicates the present state of the event handling for a routed event as it travels the route. </summary>
	/// <returns>If setting, set to true if the event is to be marked handled; otherwise false. If reading this value, true indicates that either a class handler, or some instance handler along the route, has already marked this event handled. false.indicates that no such handler has marked the event handled.The default value is false.</returns>
	public bool Handled
	{
		get
		{
			return _flags[1];
		}
		set
		{
			if (_routedEvent == null)
			{
				throw new InvalidOperationException(SR.RoutedEventArgsMustHaveRoutedEvent);
			}
			if (TraceRoutedEvent.IsEnabled)
			{
				TraceRoutedEvent.TraceActivityItem(TraceRoutedEvent.HandleEvent, value, RoutedEvent.OwnerType.Name, RoutedEvent.Name, this);
			}
			_flags[1] = value;
		}
	}

	/// <summary>Gets or sets a reference to the object that raised the event. </summary>
	/// <returns>The object that raised the event.</returns>
	public object Source
	{
		get
		{
			return _source;
		}
		set
		{
			if (InvokingHandler && UserInitiated)
			{
				throw new InvalidOperationException(SR.RoutedEventCannotChangeWhileRouting);
			}
			if (_routedEvent == null)
			{
				throw new InvalidOperationException(SR.RoutedEventArgsMustHaveRoutedEvent);
			}
			if (_source == null && _originalSource == null)
			{
				_source = (_originalSource = value);
				OnSetSource(value);
			}
			else if (_source != value)
			{
				_source = value;
				OnSetSource(value);
			}
		}
	}

	/// <summary>Gets the original reporting source as determined by pure hit testing, before any possible <see cref="P:System.Windows.RoutedEventArgs.Source" /> adjustment by a parent class.</summary>
	/// <returns>The original reporting source, before any possible <see cref="P:System.Windows.RoutedEventArgs.Source" /> adjustment made by class handling, which may have been done to flatten composited element trees.</returns>
	public object OriginalSource => _originalSource;

	internal bool UserInitiated
	{
		[FriendAccessAllowed]
		get
		{
			if (_flags[2])
			{
				return true;
			}
			return false;
		}
	}

	private bool InvokingHandler
	{
		get
		{
			return _flags[4];
		}
		set
		{
			_flags[4] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class. </summary>
	public RoutedEventArgs()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class, using the supplied routed event identifier. </summary>
	/// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class.</param>
	public RoutedEventArgs(RoutedEvent routedEvent)
		: this(routedEvent, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class, using the supplied routed event identifier, and providing the opportunity to declare a different source for the event. </summary>
	/// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class.</param>
	/// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source" /> property.</param>
	public RoutedEventArgs(RoutedEvent routedEvent, object source)
	{
		_routedEvent = routedEvent;
		_source = (_originalSource = source);
	}

	internal void OverrideRoutedEvent(RoutedEvent newRoutedEvent)
	{
		_routedEvent = newRoutedEvent;
	}

	internal void OverrideSource(object source)
	{
		_source = source;
	}

	/// <summary>When overridden in a derived class, provides a notification callback entry point whenever the value of the <see cref="P:System.Windows.RoutedEventArgs.Source" /> property of an instance changes.</summary>
	/// <param name="source">The new value that <see cref="P:System.Windows.RoutedEventArgs.Source" /> is being set to.</param>
	protected virtual void OnSetSource(object source)
	{
	}

	/// <summary>When overridden in a derived class, provides a way to invoke event handlers in a type-specific way, which can increase efficiency over the base implementation.</summary>
	/// <param name="genericHandler">The generic handler / delegate implementation to be invoked.</param>
	/// <param name="genericTarget">The target on which the provided handler should be invoked.</param>
	protected virtual void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		if ((object)genericHandler == null)
		{
			throw new ArgumentNullException("genericHandler");
		}
		if (genericTarget == null)
		{
			throw new ArgumentNullException("genericTarget");
		}
		if (_routedEvent == null)
		{
			throw new InvalidOperationException(SR.RoutedEventArgsMustHaveRoutedEvent);
		}
		InvokingHandler = true;
		try
		{
			if (genericHandler is RoutedEventHandler)
			{
				((RoutedEventHandler)genericHandler)(genericTarget, this);
				return;
			}
			genericHandler.DynamicInvoke(genericTarget, this);
		}
		finally
		{
			InvokingHandler = false;
		}
	}

	internal void InvokeHandler(Delegate handler, object target)
	{
		InvokingHandler = true;
		try
		{
			InvokeEventHandler(handler, target);
		}
		finally
		{
			InvokingHandler = false;
		}
	}

	internal void MarkAsUserInitiated()
	{
		_flags[2] = true;
	}

	internal void ClearUserInitiated()
	{
		_flags[2] = false;
	}
}
